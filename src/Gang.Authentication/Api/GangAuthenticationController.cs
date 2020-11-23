using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Gang.Authentication.Api
{
    public class GangAuthenticationController :
        Controller
    {
        readonly ILogger<GangAuthenticationController> _logger;
        readonly IGangAuthenticationService _authService;

        public GangAuthenticationController(
            ILogger<GangAuthenticationController> logger,
            IGangAuthenticationService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost]
        [Route(GangAuthenticationRoutes.REQUEST_LINK)]
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
        [Route(GangAuthenticationRoutes.LINK)]
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
