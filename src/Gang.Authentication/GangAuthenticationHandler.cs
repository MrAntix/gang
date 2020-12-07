using System.Threading.Tasks;

namespace Gang.Authentication
{
    public class GangAuthenticationHandler :
        IGangAuthenticationHandler
    {
        readonly IGangAuthenticationService _service;

        public GangAuthenticationHandler(
            IGangAuthenticationService service)
        {
            _service = service;
        }

        async Task<GangSession> IGangAuthenticationHandler
            .HandleAsync(GangParameters parameters)
        {
            if (parameters is null)
                throw new System.ArgumentNullException(nameof(parameters));

            if (string.IsNullOrWhiteSpace(parameters.GangId)
                || string.IsNullOrWhiteSpace(parameters.Token))
                return default;

            return await _service.AuthenticateAsync(parameters.Token);
        }
    }
}
