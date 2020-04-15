using Gang.Contracts;
using Gang.Events;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang
{
    public class GangHandler :
        IGangHandler
    {
        readonly GangCollection _gangs;
        readonly Subject<GangEvent> _events;

        public GangHandler(
            GangCollection gangs
            )
        {
            _gangs = gangs;
            _events = new Subject<GangEvent>();
        }

        IObservable<GangEvent> IGangHandler.Events => _events;

        GangMemberCollection IGangHandler.GangById(
            string gangId)
        {
            return _gangs[gangId];
        }

        async Task IGangHandler.HandleAsync(
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

            await gangMember.ConnectAsync(parameters.GangId,
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
                        if (type != null && type != GangMessageTypes.Command)
                        {
                            await gangMember.DisconnectAsync("non-command");
                            return;
                        }

                        await gang.HostMember
                            .SendAsync(GangMessageTypes.Command, data, gangMember.Id);
                    }

                    _events.OnNext(new GangMemberDataEvent(parameters.GangId, gangMember, data));
                });

            _gangs.RemoveMemberFromGang(parameters.GangId, gangMember);

            await gangMember.DisconnectAsync();

            gang = _gangs[parameters.GangId];
            if (gang != null)
                await gang.HostMember
                    .SendAsync(GangMessageTypes.Disconnect, gangMember.Id);

            _events.OnNext(new GangMemberRemovedEvent(parameters.GangId, gangMember));
        }

        void IDisposable.Dispose()
        {
        }
    }
}