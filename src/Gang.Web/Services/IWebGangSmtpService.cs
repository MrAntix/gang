using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Gang.Web.Services
{
    public interface IWebGangSmtpService : IDisposable
    {
        Task SendAsync(MailMessage message);
    }
}
