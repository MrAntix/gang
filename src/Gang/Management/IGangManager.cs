using Gang.Contracts;
using Gang.Management.Events;
using Gang.Members;
using System;
using System.Threading.Tasks;

namespace Gang.Management
{
    public interface IGangManager : IDisposable
    {
        Task<GangMemberConnectionState> ManageAsync(GangParameters parameters, IGangMember gangMember, byte[] authToken = null);
        IObservable<GangManagerEvent> Events { get; }
        GangMemberCollection GangById(string gangId);
    }
}