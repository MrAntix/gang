namespace Gang.Authentication.Api
{
    public static class GangAuthenticationRoutes
    {
        public const string ROOT = "api/gang/auth";

        public const string REQUEST_LINK = ROOT + "/request-link";
        public const string VALIDATE_LINK = ROOT + "/validate-link";

        public const string REQUEST_CHALLENGE = ROOT + "/request-challenge";
        public const string REGISTER_CREDENTIAL = ROOT + "/register-credential";
        public const string VALIDATE_CREDENTIAL = ROOT + "/validate-credential";
    }
}
