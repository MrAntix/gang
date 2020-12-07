namespace Gang.Authentication.Crypto
{
    public interface IGangCryptoSettings
    {
        /// <summary>
        /// Keep this safe, used for hashing and verifying tokens
        /// </summary>
        string Secret { get; }
    }
}
