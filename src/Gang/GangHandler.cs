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

            await gangMember.ConnectAsync(async data =>
                {
                    if (data?.Length == 0) return;

                    var gang = _gangs[parameters.GangId];
                    if (gangMember == gang.HostMember)
                    {
                        var tasks = gang.OtherMembers
                            .Select(member => member
                                .SendAsync(GangMessageTypes.State, data))
                            .ToArray();

                        await Task.WhenAll(tasks);
                    }
                    else
                    {
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