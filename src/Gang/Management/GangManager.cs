using Gang.Events;
using Gang.Management.Events;
using Gang.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.Management
{
    public sealed class GangManager :
        IGangManager
    {
        public const string RESULT_DENIED = "denied";

        readonly ILogger<GangManager> _logger;
        readonly IGangSettings _settings;
        readonly GangCollection _gangs;
        readonly IGangControllerFactory _controllerFactory;
        readonly IGangSerializationService _serializer;
        readonly IGangManagerSequenceProvider _sequence;
        readonly GangEventExecutor<IGangManagerEvent> _eventExecutor;
        readonly Subject<IGangManagerEvent> _events;

        public GangManager(
            ILogger<GangManager> logger,
            IGangSettings settings,
            GangCollection gangs,
            IGangControllerFactory controllerFactory,
            IGangSerializationService serializer,
            IGangManagerSequenceProvider sequence,
            GangEventExecutor<IGangManagerEvent> eventExecutor = null
            )
        {
            _logger = logger;
            _settings = settings;
            _gangs = gangs;
            _controllerFactory = controllerFactory;
            _serializer = serializer;
            _sequence = sequence;
            _eventExecutor = eventExecutor;
            _events = new Subject<IGangManagerEvent>();
        }

        IObservable<IGangManagerEvent> IGangManager.Events => _events;

        public void RaiseEvent<TEventData>(
            TEventData data,
            string gangId,
            byte[] memberId = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            lock (_events)
            {
                var e = new GangManagerEvent<TEventData>(
                    data,
                    new GangAudit(gangId, _sequence.Next(), memberId)
                    );

                _events.OnNext(e);

                _eventExecutor?.ExecuteAsync(e)
                    .ContinueWith(
                        t =>
                        {
                            var ex = t.Exception.InnerException;

                            _logger.LogError(ex, "Gang Manager Event Error In Handler");
                            RaiseEvent(new GangError(data, ex), gangId);
                        },
                        TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        GangMemberCollection IGangManager.GangById(
            string gangId)
        {
            return _gangs[gangId];
        }

        async Task<GangMemberConnectionState> IGangManager.ManageAsync(
            GangParameters parameters, IGangMember member)
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            var audit = new GangAudit(parameters.GangId, null, member.Id, member.Session?.User.Id);
            var auth = _serializer.Serialize(
                new GangAuth(member.Id,
                    member.Session?.Token, _settings.Application)
                );

            if (member.Session == null)
            {
                await member.HandleAsync(GangMessageTypes.Denied, auth, audit);
                await member.DisconnectAsync(RESULT_DENIED);

                return GangMemberConnectionState.Disconnected;
            }

            var gangId = parameters.GangId;
            var gang = _gangs
                .AddMemberToGang(parameters.GangId, member, _ => RaiseEvent(new GangAdded(), gangId));

            if (gang.HostMember == member)
            {
                await member.HandleAsync(GangMessageTypes.Host, auth, audit);
            }
            else
            {
                await member.HandleAsync(GangMessageTypes.Member, auth, audit);
                await gang.HostMember.HandleAsync(GangMessageTypes.Connect, null, audit);
            }

            RaiseEvent(new GangMemberAdded(), gangId, member.Id);

            var controller = _controllerFactory.Create(
                this, parameters.GangId, member,
                async (data) =>
                {
                    if (member == gang.HostMember)
                        throw new GangException("host member should not receive data");

                    var sequenceNumber = BitConverter.ToUInt32(data.AsSpan()[0..4]);

                    var audit = new GangAudit(parameters.GangId, sequenceNumber, member.Id, member.Session?.User.Id)
;
                    await gang.HostMember
                        .HandleAsync(GangMessageTypes.Command, data[4..], audit);

                    RaiseEvent(new GangMemberData(data), gangId, member.Id);
                },
                async (type, data, audit, memberIds) =>
                {
                    var gang = _gangs[parameters.GangId];
                    if (member != gang.HostMember)
                        throw new GangException("Only host member can send data to members");

                    var members = memberIds == null
                        ? gang.OtherMembers
                        : gang.OtherMembers
                            .Where(m => memberIds.Any(mId => mId.SequenceEqual(m.Id)));

                    var tasks = members.ToArray()
                        .Select(m => m
                            .HandleAsync(type ?? GangMessageTypes.State, data, audit))
                        .ToArray();

                    await Task.WhenAll(tasks);
                });

            var state = new GangMemberConnectionState();
            await member.ConnectAsync(controller, async () =>
                {
                    _gangs.RemoveMemberFromGang(parameters.GangId, member);

                    gang = _gangs[parameters.GangId];
                    if (gang != null)
                    {
                        await gang.HostMember
                            .HandleAsync(GangMessageTypes.Disconnect, null, audit.SetOn());
                    }

                    RaiseEvent(new GangMemberRemoved(), gangId, member.Id);

                    state.SetDisconnected();
                });

            return state;
        }

        void IDisposable.Dispose()
        {
        }
    }
}