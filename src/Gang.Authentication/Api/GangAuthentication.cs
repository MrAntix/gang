namespace Gang.Authentication.Api
{
    public sealed class GangAuthentication
    {
        public GangAuthentication(
            string credentialId,
            string clientData,
            string authenticatorData,
            string signature
            )
        {
            CredentialId = credentialId;
            ClientData = clientData;
            AuthenticatorData = authenticatorData;
            Signature = signature;
        }

        public string CredentialId { get; }
        public string ClientData { get; }
        public string AuthenticatorData { get; }
        public string Signature { get; }
    }
}
