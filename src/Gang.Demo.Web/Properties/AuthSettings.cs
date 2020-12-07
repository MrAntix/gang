namespace Gang.Demo.Web.Properties
{
    public class AuthSettings
    {
        /// <summary>
        /// Auth services enabled, otherwise falls back to simple token
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Keep this safe, used for hashing and verifying tokens
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// Link parts, default 2
        /// </summary>
        public int? LinkParts { get; set; }
        /// <summary>
        /// Link expiry minutes, default 15
        /// </summary>
        public int? LinkExpiryMinutes { get; set; }
        /// <summary>
        /// Session minutes, default 14 days
        /// </summary>
        public int? SessionExpiryMinutes { get; set; }
    }
}
