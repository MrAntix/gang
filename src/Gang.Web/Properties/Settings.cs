using Gang.Auth;

namespace Gang.Web.Properties
{
    public class Settings
    {
        public AppSettings App { get; set; }
        public SmtpSettings Smtp { get; set; }
        public GangAuthSettings Auth { get; set; }
    }
}
