using Gang.Authentication.Users;
using Gang.Demo.Web.Properties;
using Gang.Events;
using Gang.Management.Events;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services
{
    public sealed class AuthenticationUserLinkHandler :
        IGangEventHandler<GangManagerEvent<GangUserLink>>
    {
        readonly AppSettings _app;
        readonly ISmtpService _smtp;

        public AuthenticationUserLinkHandler(
            AppSettings app,
            ISmtpService smtp)
        {
            _app = app;
            _smtp = smtp;
        }

        async Task IGangEventHandler<GangManagerEvent<GangUserLink>>.HandleAsync(
            GangManagerEvent<GangUserLink> e)
        {
            var message = new MailMessage
            {
                Subject = "Gang Chat: Invite"
            };

            message.To.Add(new MailAddress(e.Data.EmailAddress, e.Data.Name));

            var uri = QueryHelpers.AddQueryString(
                _app.RootUrl + "/", new Dictionary<string, string>{
                            {"link-code", e.Data.OneTimeCode.Value }
                });

            message.Body =
$@"Hi {e.Data.Name},

An access code was requested for {e.Data.EmailAddress}

click the link below to gain access
{uri}";

            await _smtp.SendAsync(message);
        }
    }
}
