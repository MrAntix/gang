namespace Gang.Authentication.Api
{
    public static class GangAuthenticationRoutes
    {
        public const string ROOT = "api/gang/auth";
        public const string REQUEST_LINK = ROOT + "/request-link";
        public const string LINK = ROOT + "/link/{token}";
    }
}
