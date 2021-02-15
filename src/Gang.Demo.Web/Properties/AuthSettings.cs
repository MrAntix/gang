using Gang.Authentication;

namespace Gang.Demo.Web.Properties
{
    public class AuthSettings : IGangAuthenticationSettings
    {
        public bool Enabled { get; set; }
        public string Secret { get; set; }
        public int? LinkParts { get; set; }
        public int? LinkExpiryMinutes { get; set; }
        public int? SessionExpiryMinutes { get; set; }
        public int? CredentialExpiryDays { get; set; }
    }
}
