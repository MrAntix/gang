using Gang.Contracts;
using Gang.Management;
using Gang.Management.Contracts;
using Gang.Members;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.Tests
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
                    new GangAudit(gangId, memberId, ++_lastEventSequenceNumber)
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

            gangMember.ConnectAsync(null, null);

            return Task.FromResult(
                state
                );
        }

        public void Dispose()
        {
        }
    }
}
