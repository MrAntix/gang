using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gang.Auth.Api
{
    public class GangAuthController :
        Controller
    {
        readonly ILogger<GangAuthController> _logger;
        readonly IGangAuthService _authService;

        public GangAuthController(
            ILogger<GangAuthController> logger,
            IGangAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost]
        [Route(GangAuthRoutes.REQUEST_LINK)]
        public async Task<IActionResult> RequestLinkAsync(
            [FromBody] string identity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identity))
                    return BadRequest();

                await _authService.RequestLink(identity);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request Link Error");
                throw;
            }
        }

        [HttpGet]
        [Route(GangAuthRoutes.LINK)]
        public async Task<IActionResult> LinkAsync(
            [FromRoute] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest();

                var sessionToken = await _authService.Link(token);

                return Ok(sessionToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Link Error");
                throw;
            }
        }
    }
}
