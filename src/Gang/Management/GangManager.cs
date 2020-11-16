using Antix.Handlers;
using Gang.Contracts;
using Gang.Management.Contracts;
using Gang.Members;
using Gang.Serialization;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.Management
{
    public sealed class GangManager :
        IGangManager
    {
        readonly GangCollection _gangs;
        readonly IGangSerializationService _serializer;
        readonly IGangManagerEventSequenceNumberProvider _eventSequenceNumber;
        readonly Executor<IGangManagerEvent> _eventHandlerExecutor;
        readonly Subject<IGangManagerEvent> _events;

        public GangManager(
            GangCollection gangs,
            IGangSerializationService serializer,
            IGangManagerEventSequenceNumberProvider eventSequenceNumber,
            Executor<IGangManagerEvent> eventHandlerExecutor = null
            )
        {
            _gangs = gangs;
            _serializer = serializer;
            _eventSequenceNumber = eventSequenceNumber;
            _eventHandlerExecutor = eventHandlerExecutor;
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
                _eventHandlerExecutor?.ExecuteAsync(e)
                    .ContinueWith(t =>
                    {

                        if (t.Exception != null) throw t.Exception;
                    });
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

            if (gangMember.Auth?.Token != null)
                await gangMember.SendAsync(GangMessageTypes.Authenticated, gangMember.Auth.Token.GangToBytes());

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