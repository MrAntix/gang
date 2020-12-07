using Gang.Authentication.Crypto;

namespace Gang.Authentication
{
    public interface IGangAuthenticationSettings : IGangCryptoSettings
    {
        /// <summary>
        /// Link token minutes, default 15
        /// </summary>
        int? LinkParts { get; }
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
