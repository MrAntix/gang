namespace Gang.Authentication
{
    public sealed class GangAuthenticationSettings
    {
        /// <summary>
        /// Keep this safe, used for hashing and verifying tokens
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// Link token minutes, default 15
        /// </summary>
        public int LinkTokenExpiryMinutes { get; set; } = 15;
        /// <summary>
        /// Session token minutes, default 14 days
        /// </summary>
        public int SessionTokenExpiryMinutes { get; set; } = 60 * 24 * 14;
    }
}
