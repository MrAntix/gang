using Gang.Contracts;
using System;
using System.Threading.Tasks;

namespace Gang.WebSockets
{
    public interface IWebSocketGangAutherticator
    {
        Task ExecuteAsync(
            GangParameters parameters,
            Func<GangAuth, Task<IGangMember>> getMemberAsync);
    }
}
