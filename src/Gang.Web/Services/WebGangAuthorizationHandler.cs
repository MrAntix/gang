using Gang.Contracts;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public class WebGangAuthorizationHandler :
        IGangAuthorizationHandler
    {
        private readonly IGangHandler _gangHandler;

        public WebGangAuthorizationHandler(
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
