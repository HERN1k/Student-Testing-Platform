namespace TestingPlatform.Utilities
{
    public record AzureAd
    {
        public AzureAd() { }

        public AzureAd(
            string ClientId,
            string Tenant,
            string RedirectUri,
            string SuccessMessage,
            string Instance,
            IEnumerable<string> Scopes
          )
        {
            this.ClientId = ClientId;
            this.Tenant = Tenant;
            this.RedirectUri = RedirectUri;
            this.SuccessMessage = SuccessMessage;
            this.Instance = Instance;
            this.Scopes = Scopes;
        }

        public string ClientId { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public IEnumerable<string> Scopes { get; set; } = [];
    }

    public record ApiConfiguration
    {
        public ApiConfiguration() { }

        public ApiConfiguration(string uri)
        {
            this.Uri = uri;
        }

        public string Uri { get; set; } = string.Empty;
    }
}