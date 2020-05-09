using Gang.Contracts;
using Gang.Events;
using System;
using System.Threading.Tasks;

namespace Gang
{
    public interface IGangHandler : IDisposable
    {
        Task<GangMemberConnectionState> HandleAsync(GangParameters parameters, IGangMember gangMember);
        IObservable<GangEvent> Events { get; }
        GangMemberCollection GangById(string gangId);
    }
}