using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services
{
    public interface ISmtpService : IDisposable
    {
        Task SendAsync(MailMessage message);
    }
}
