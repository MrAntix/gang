using Gang.Contracts;
using Gang.Events;
using Gang.Serialization;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang
{
    public sealed class GangHandler :
        IGangHandler
    {
        readonly GangCollection _gangs;
        readonly IGangSerializationService _serializer;
        readonly Subject<GangEvent> _events;

        public GangHandler(
            GangCollection gangs,
            IGangSerializationService serializer
            )
        {
            _gangs = gangs;
            _serializer = serializer;
            _events = new Subject<GangEvent>();
        }

        IObservable<GangEvent> IGangHandler.Events => _events;

        GangMemberCollection IGangHandler.GangById(
            string gangId)
        {
            return _gangs[gangId];
        }

        async Task<GangMemberConnectionState> IGangHandler.HandleAsync(
            GangParameters parameters, IGangMember gangMember)
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (gangMember is null)
                throw new ArgumentNullException(nameof(gangMember));

            var gang = _gangs[parameters.GangId];

            gang = _gangs.AddMemberToGang(
               parameters.GangId, gangMember,
               _ => _events.OnNext(new GangAddedEvent(parameters.GangId)));

            if (gang.HostMember == gangMember)
            {
                await gangMember.SendAsync(GangMessageTypes.Host, gangMember.Id);
            }
            else
            {
                await gangMember.SendAsync(GangMessageTypes.Member, gangMember.Id);
                await gang.HostMember.SendAsync(GangMessageTypes.Connect, gangMember.Id);
            }

            _events.OnNext(new GangMemberAddedEvent(parameters.GangId, gangMember));

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

                        var tasks = members
                            .Select(member => member
                                .SendAsync(type ?? GangMessageTypes.State, data))
                            .ToArray();

                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        await gang.HostMember
                            .SendAsync(GangMessageTypes.Command, data, gangMember.Id);
                    }

                    _events.OnNext(new GangMemberDataEvent(parameters.GangId, gangMember, data));
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

                _events.OnNext(new GangMemberRemovedEvent(parameters.GangId, gangMember));

                state.Disconnected();
            });

            return state;
        }

        void IDisposable.Dispose()
        {
        }
    }
}