using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gang.Auth.Api
{
    public class GangAuthController :
        Controller
    {
        readonly IGangAuthService _authService;

        public GangAuthController(
            IGangAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route(GangAuthRoutes.REQUEST_LINK)]
        public async Task<IActionResult> RequestLinkAsync(
            [FromBody] string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
                return BadRequest();

            await _authService.RequestLink(identity);

            return Ok();
        }

        [HttpGet]
        [Route(GangAuthRoutes.LINK)]
        public async Task<IActionResult> LinkAsync(
            [FromRoute] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest();

            var sessionToken = await _authService.Link(token);

            return Ok(sessionToken);
        }
    }
}
