using Gang.Contracts;
using Gang.Management.Events;
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
        readonly Subject<GangManagerEvent> _events;

        public GangManager(
            GangCollection gangs,
            IGangSerializationService serializer
            )
        {
            _gangs = gangs;
            _serializer = serializer;
            _events = new Subject<GangManagerEvent>();
        }

        IObservable<GangManagerEvent> IGangManager.Events => _events;

        GangMemberCollection IGangManager.GangById(
            string gangId)
        {
            return _gangs[gangId];
        }

        async Task<GangMemberConnectionState> IGangManager.ManageAsync(
            GangParameters parameters, IGangMember gangMember, GangAuth auth)
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (gangMember is null)
                throw new ArgumentNullException(nameof(gangMember));

            var gang = _gangs[parameters.GangId];

            gang = _gangs.AddMemberToGang(
               parameters.GangId, gangMember,
               _ => _events.OnNext(new GangAddedManagerEvent(parameters.GangId)));

            if (gang.HostMember == gangMember)
            {
                await gangMember.SendAsync(GangMessageTypes.Host, gangMember.Id);
            }
            else
            {
                await gangMember.SendAsync(GangMessageTypes.Member, gangMember.Id);
                await gang.HostMember.SendAsync(GangMessageTypes.Connect, gangMember.Id);
            }

            if (auth?.Token != null)
                await gangMember.SendAsync(GangMessageTypes.Authenticated, auth.Token);

            _events.OnNext(new GangMemberAddedManagerEvent(parameters.GangId, gangMember));
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

                    _events.OnNext(new GangMemberDataManagerEvent(parameters.GangId, gangMember, data));
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

                _events.OnNext(new GangMemberRemovedManagerEvent(parameters.GangId, gangMember));

                state.Disconnected();
            });

            return state;
        }

        void IDisposable.Dispose()
        {
        }
    }
}