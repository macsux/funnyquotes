namespace FunnyQuotesCommon
{
    public class FunnyQuotesConfiguration
    {
        public string ClientType { get; set; } = "local";
        public string FailedMessage { get; set; }
        public bool EnableSecurity { get; set; }
    }
}
