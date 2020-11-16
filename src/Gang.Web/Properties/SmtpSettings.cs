namespace Gang.Web.Properties
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string FromAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
}