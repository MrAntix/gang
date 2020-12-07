namespace Gang.Authentication
{
    public interface IGangAuthenticationSettings
    {        /// <summary>
             /// Keep this safe, used for hashing and verifying tokens
             /// </summary>
        string Secret { get; }
        /// <summary>
        /// Link token minutes, default 15
        /// </summary>
        int? LinkExpiryMinutes { get; }
        /// <summary>
        /// Session token minutes, default 14 days
        /// </summary>
        int? SessionExpiryMinutes { get; }
    }
}
