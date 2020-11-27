using Gang.Management;
using Gang.Management.Events;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.Tests.Management.Fakes
{
    public sealed class FakeGangManager : IGangManager
    {
        uint _lastEventSequenceNumber;
        readonly Subject<IGangManagerEvent> _events = new();
        public IObservable<IGangManagerEvent> Events => _events;

        public void RaiseEvent<TEventData>(
            TEventData data,
            string gangId,
            byte[] memberId = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            lock (_events)
            {
                _events.OnNext(new GangManagerEvent<TEventData>(
                    data,
                    new GangAudit(gangId, ++_lastEventSequenceNumber, memberId)
                    ));
            }
        }

        public GangMemberCollection GangById(string gangId)
        {
            return null;
        }

        public Task<GangMemberConnectionState> ManageAsync(
            GangParameters parameters,
            IGangMember gangMember)
        {
            var state = new GangMemberConnectionState();
            state.Disconnected();

            gangMember.ConnectAsync(
                new GangController(
                    parameters.GangId,
                    this,
                    null, null,
                    null
                    ),
                () => Task.CompletedTask);

            return Task.FromResult(
                state
                );
        }

        public void Dispose()
        {
        }
    }
}
