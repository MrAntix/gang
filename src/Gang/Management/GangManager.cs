using Antix.Handlers;
using Gang.Contracts;
using Gang.Management.Contracts;
using Gang.Members;
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
        readonly ILogger<GangManager> _logger;
        readonly GangCollection _gangs;
        readonly IGangSerializationService _serializer;
        readonly IGangManagerEventSequenceNumberProvider _eventSequenceNumber;
        readonly Executor<IGangManagerEvent> _eventExecutor;
        readonly Subject<IGangManagerEvent> _events;

        public GangManager(
            ILogger<GangManager> logger,
            GangCollection gangs,
            IGangSerializationService serializer,
            IGangManagerEventSequenceNumberProvider eventSequenceNumber,
            Executor<IGangManagerEvent> eventExecutor = null
            )
        {
            _logger = logger;
            _gangs = gangs;
            _serializer = serializer;
            _eventSequenceNumber = eventSequenceNumber;
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
                    new GangAudit(gangId, memberId, _eventSequenceNumber.Next())
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
            GangParameters parameters, IGangMember gangMember)
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (gangMember is null)
                throw new ArgumentNullException(nameof(gangMember));

            var gangId = parameters.GangId;
            var gang = _gangs[gangId];

            gang = _gangs.AddMemberToGang(
               parameters.GangId, gangMember,
               _ => RaiseEvent(new GangAdded(), gangId));

            if (gang.HostMember == gangMember)
            {
                await gangMember.SendAsync(GangMessageTypes.Host, gangMember.Id);
            }
            else
            {
                await gang.HostMember.SendAsync(GangMessageTypes.Connect, gangMember.Id);
                await gangMember.SendAsync(GangMessageTypes.Member, gangMember.Id);
            }

            if (gangMember.Auth != null)
                await gangMember.SendAsync(GangMessageTypes.Authenticate, gangMember.Auth?.Token?.GangToBytes());

            RaiseEvent(new GangMemberAdded(), gangId, gangMember.Id);
            uint? commandSequenceNumber = 0;

            var controller = new GangController(
                parameters.GangId, this,
                async (data, type, memberIds) =>
                {
                    if (data?.Length == 0) return;

                    var gang = _gangs[parameters.GangId];
                    if (gangMember == gang.HostMember)
                    {
                        var members = memberIds == null
                            ? gang.OtherMembers
                            : gang.OtherMembers
                                .Where(m => memberIds.Any(mId => mId.SequenceEqual(m.Id)));

                        var sequenceNumber = type == GangMessageTypes.Command
                            ? ++commandSequenceNumber
                            : null;

                        var tasks = members
                            .Select(member => member
                                .SendAsync(type ?? GangMessageTypes.State, data, null, sequenceNumber))
                            .ToArray();

                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        var sequenceNumber = BitConverter.ToUInt32(data.AsSpan()[0..4]);

                        await gang.HostMember
                            .SendAsync(GangMessageTypes.Command, data[4..], gangMember.Id, sequenceNumber);
                    }

                    RaiseEvent(new GangMemberData(data), gangId, gangMember.Id);
                },
                _serializer
                );

            var state = new GangMemberConnectionState();
            await gangMember.ConnectAsync(controller, async () =>
                {
                    _gangs.RemoveMemberFromGang(parameters.GangId, gangMember);

                    gang = _gangs[parameters.GangId];
                    if (gang != null)
                        await gang.HostMember
                        .SendAsync(GangMessageTypes.Disconnect, gangMember.Id);

                    RaiseEvent(new GangMemberRemoved(), gangId, gangMember.Id);

                    state.Disconnected();
                });

            return state;
        }

        void IDisposable.Dispose()
        {
        }
    }
}