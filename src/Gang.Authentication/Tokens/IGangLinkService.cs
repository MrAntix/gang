namespace Gang.Authentication.Tokens
{
    public interface IGangLinkService
    {
        /// <summary>
        /// Gets a random string for use as a one time code
        /// </summary>
        /// <returns>Random string</returns>
        string CreateCode();
    }
}
