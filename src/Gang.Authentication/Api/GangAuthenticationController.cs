using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Gang.GangPredicate;

namespace Gang.Authentication.Api
{
    public sealed class GangAuthenticationController :
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
                if (Is.NullOrWhiteSpace(identity)
                    || IsNot.EmailAddress(identity))
                    return BadRequest("invalid email address");

                await _authService.RequestLinkAsync(identity);

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
                if (Is.NullOrWhiteSpace(token))
                    return BadRequest("invalid token");

                var sessionToken = await _authService.LinkAsync(token);

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
