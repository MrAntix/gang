using Gang.Contracts;
using Gang.Management;
using Gang.Management.Events;
using Gang.Members;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangManager : IGangManager
    {
        public IObservable<GangManagerEvent> Events =>
            Observable.Never<GangManagerEvent>();

        public void Dispose()
        {
        }

        public GangMemberCollection GangById(string gangId)
        {
            return null;
        }

        public Task<GangMemberConnectionState> ManageAsync(
            GangParameters parameters,
            IGangMember gangMember,
            byte[] authToken = null)
        {
            var state = new GangMemberConnectionState();
            state.Disconnected();

            gangMember.ConnectAsync(null, null);

            return Task.FromResult(
                state
                );
        }
    }
}
