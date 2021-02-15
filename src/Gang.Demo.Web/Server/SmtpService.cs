using Gang.Demo.Web.Properties;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Server
{
    public sealed class SmtpService :
        ISmtpService
    {
        readonly SmtpClient _client;
        readonly SmtpSettings _settings;

        public SmtpService(
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

        Task ISmtpService.SendAsync(MailMessage message)
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
