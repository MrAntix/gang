using System;

namespace Gang.Authentication.Users
{
    public sealed class GangUserLinkCode
    {
        public GangUserLinkCode(
            string value,
            DateTimeOffset? expiry = null)
        {
            Value = value;
            Expires = expiry ?? DateTimeOffset.Now.AddMinutes(5);
        }

        public string Value { get; }
        public DateTimeOffset? Expires { get; }
    }
}
