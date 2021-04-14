using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Gang.Validation.GangLiterals;

namespace Gang.Authentication.Api
{
    public sealed class GangAuthenticationController :
        Controller
    {
        readonly IGangAuthenticationService _auth;

        public GangAuthenticationController(
            IGangAuthenticationService auth
            )
        {
            _auth = auth;
        }

        [HttpPost]
        [Route(GangAuthenticationRoutes.REQUEST_LINK)]
        public async Task<IActionResult> RequestLinkAsync(
            [FromBody] string identity
            )
        {
            if (
                identity == NullOrWhiteSpace
                    || identity != EmailAddress
                )
                return BadRequest("invalid email address");

            await _auth.RequestLinkAsync(identity);

            return Ok();
        }

        [HttpPost]
        [Route(GangAuthenticationRoutes.VALIDATE_LINK)]
        public async Task<IActionResult> ValidateLinkAsync(
            [FromBody] GangLink data
            )
        {
            if (data?.Email == NullOrWhiteSpace)
                return BadRequest("invalid email");
            if (data?.Code == NullOrWhiteSpace)
                return BadRequest("invalid code");

            var sessionToken = await _auth.ValidateLinkAsync(data);

            return sessionToken != null
                ? Ok(sessionToken)
                : BadRequest();
        }

        [HttpPost]
        [Route(GangAuthenticationRoutes.REQUEST_CHALLENGE)]
        public async Task<IActionResult> RequestChallengeAsync(
            [FromHeader] string authorization
            )
        {
            var challenge = await _auth.RequestChallengeAsync(authorization);

            return Ok(challenge);
        }

        [HttpPost]
        [Route(GangAuthenticationRoutes.REGISTER_CREDENTIAL)]
        public async Task<IActionResult> RegisterCredentialAsync(
            [FromHeader] string authorization,
            [FromBody] GangCredentialRegistration data
            )
        {
            if (authorization == NullOrWhiteSpace)
                return BadRequest("invalid token");

            return await _auth.RegisterCredentialAsync(authorization, data)
                   ? Ok()
                   : BadRequest();
        }

        [HttpPost]
        [Route(GangAuthenticationRoutes.VALIDATE_CREDENTIAL)]
        public async Task<IActionResult> ValidateCredentialAsync(
            [FromBody] GangAuthentication data
            )
        {
            var sessionToken = await _auth.ValidateCredentialAsync(data);

            return sessionToken != null
                ? Ok(sessionToken)
                : BadRequest();
        }
    }
}
