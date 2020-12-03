using Gang.Authentication;

namespace Gang.Demo.Web.Properties
{
    public class AuthSettings :
        GangAuthenticationSettings
    {
        public bool Enabled { get; set; }
    }
}
