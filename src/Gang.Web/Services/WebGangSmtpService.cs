using Gang.Web.Properties;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public sealed class WebGangSmtpService :
        IWebGangSmtpService
    {
        readonly SmtpClient _client;
        readonly SmtpSettings _settings;

        public WebGangSmtpService(
            SmtpSettings settings)
        {
            _client = new SmtpClient(
                settings.Host, settings.Port
                );

            if (!string.IsNullOrWhiteSpace(settings.UserName))
                _client.Credentials = new NetworkCredential(
                    settings.UserName,
                    settings.Password);

            else
                _client.UseDefaultCredentials
                    = settings.UseDefaultCredentials;
            _settings = settings;
        }

        Task IWebGangSmtpService.SendAsync(MailMessage message)
        {
            if (message.From == null)
                message.From = new MailAddress(_settings.FromAddress);

            return _client.SendMailAsync(message);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
