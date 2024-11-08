namespace TestingPlatform.Utilities
{
    public record Msal
    {
        public Msal() { }

        public Msal(
            string ClientId,
            string Tenant,
            string RedirectUri,
            string Instance,
            string GraphAPIEndpoint,
            IEnumerable<string> Scopes
          )
        {
            this.ClientId = ClientId;
            this.Tenant = Tenant;
            this.RedirectUri = RedirectUri;
            this.Instance = Instance;
            this.GraphAPIEndpoint = GraphAPIEndpoint;
            this.Scopes = Scopes;
        }

        public string ClientId { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public string GraphAPIEndpoint { get; set; } = string.Empty;
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