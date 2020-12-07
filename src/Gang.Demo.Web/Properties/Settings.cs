using Gang.Authentication;

namespace Gang.Demo.Web.Properties
{
    public sealed class Settings : IGangSettings, IGangAuthenticationSettings
    {
        public AppSettings App { get; set; }
        public SmtpSettings Smtp { get; set; }
        public AuthSettings Auth { get; set; }

        GangApplication IGangSettings.Application => new(App.Id, App.Name);
        string IGangAuthenticationSettings.Secret => Auth?.Secret;
        int? IGangAuthenticationSettings.LinkExpiryMinutes => Auth?.LinkExpiryMinutes;
        int? IGangAuthenticationSettings.SessionExpiryMinutes => Auth.SessionExpiryMinutes;
    }
}
