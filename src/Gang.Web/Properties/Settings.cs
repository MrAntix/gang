using Gang.Authentication;

namespace Gang.Web.Properties
{
    public sealed class Settings
    {
        public AppSettings App { get; set; }
        public SmtpSettings Smtp { get; set; }
        public GangAuthenticationSettings Auth { get; set; }
    }
}
