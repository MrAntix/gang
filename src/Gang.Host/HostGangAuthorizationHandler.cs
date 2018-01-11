using Gang;
using Gang.Contracts;
using System.Threading.Tasks;

namespace Antix.Gang.Host
{
    public class HostGangAuthorizationHandler :
        IGangAuthorizationHandler
    {
        private readonly IGangHandler _gangHandler;

        public HostGangAuthorizationHandler(
            IGangHandler gangHandler)
        {
            _gangHandler = gangHandler;
        }

        Task<bool> IGangAuthorizationHandler.AuthorizeAsync(
            GangParameters parameters)
        {
            var allow = false;

            if (parameters.GangId == "demo")
            {
                var gang = _gangHandler.GangById(parameters.GangId);
                allow = gang == null
                    || gang.Members.Count < 3;
            }

            return Task.FromResult(allow);
        }
    }
}
