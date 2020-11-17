using Antix.Handlers;
using Gang.Authentication.Contracts;
using Gang.Management.Contracts;
using Gang.Web.Properties;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public sealed class WebGangAuthenticationUserLinkHandler :
        IHandler<GangManagerEvent<GangUserLink>>
    {
        readonly AppSettings _app;
        readonly IWebGangSmtpService _smtp;

        public WebGangAuthenticationUserLinkHandler(
            AppSettings app,
            IWebGangSmtpService smtp)
        {
            _app = app;
            _smtp = smtp;
        }

        async Task IHandler<GangManagerEvent<GangUserLink>>.HandleAsync(
            GangManagerEvent<GangUserLink> e)
        {
            var message = new MailMessage
            {
                Subject = "Gang Chat: Access code request"
            };

            message.To.Add(new MailAddress(e.Data.EmailAddress, e.Data.Name));

            var uri = QueryHelpers.AddQueryString(
                _app.RootUrl + "/", new Dictionary<string, string>{
                            {"link-token", e.Data.Token.Value }
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
