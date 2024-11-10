using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Services.Graph
{
    public partial class GraphService : IGraphService
    {
        private readonly AzureAd _azureAd;
        private readonly ILogger<GraphService> _logger;
        private readonly InteractiveBrowserCredential _interactiveCredential;
        private readonly string[] _graphScopes;
        private readonly string[] _apiScopes;
        private GraphServiceClient? _client;
        private GraphServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = InitializationGraphClient();
                }

                return _client;
            }
        }
        private bool disposedValue;

        public GraphService(IOptions<AzureAd> options, ILogger<GraphService> logger)
        {
            ValidateConstructorArguments(options, logger);
            _azureAd = SetAzureAdConfiguration(options);
            _logger = logger;
            (_graphScopes, _apiScopes) = InitializationTokenScopes(_azureAd.Scopes.ToArray());
            _interactiveCredential = InitializationInteractiveBrowser();
        }

        private static void ValidateConstructorArguments(IOptions<AzureAd> options, ILogger<GraphService> logger)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        }

        private static AzureAd SetAzureAdConfiguration(IOptions<AzureAd> options)
        {
            AzureAd configuration = options.Value;

            ArgumentException.ThrowIfNullOrWhiteSpace(configuration.ClientId, nameof(options));
            ArgumentException.ThrowIfNullOrWhiteSpace(configuration.Tenant, nameof(options));
            ArgumentException.ThrowIfNullOrWhiteSpace(configuration.RedirectUri, nameof(options));
            ArgumentException.ThrowIfNullOrWhiteSpace(configuration.Instance, nameof(options));
            if (configuration.Scopes == null || !configuration.Scopes.Any())
            {
                throw new ArgumentNullException(nameof(_azureAd.Scopes));
            }

            return configuration;
        }

        private static (string[], string[]) InitializationTokenScopes(string[] scopes)
        {
            if (scopes.Length != 2)
            {
                throw new ArgumentNullException(nameof(scopes));
            }
            ArgumentException.ThrowIfNullOrWhiteSpace(scopes[0], nameof(scopes));
            ArgumentException.ThrowIfNullOrWhiteSpace(scopes[1], nameof(scopes));
            return ([scopes[0]], [scopes[1]]);
        }

        private InteractiveBrowserCredential InitializationInteractiveBrowser()
        {
            if (OperatingSystem.IsWindows())
            {
                var options = new InteractiveBrowserCredentialOptions
                {
                    TenantId = _azureAd.Tenant,
                    ClientId = _azureAd.ClientId,
                    AuthorityHost = new Uri($"{_azureAd.Instance}{_azureAd.Tenant}/v2.0"),
                    RedirectUri = new Uri(_azureAd.RedirectUri),
                    TokenCachePersistenceOptions = new()
                };

                return new(options);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private GraphServiceClient InitializationGraphClient()
        {
            SecureStorage.RemoveAll();

            return new(_interactiveCredential, _graphScopes);
        }

        #region Disposing
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                { }

                _client?.Dispose();

                disposedValue = true;
            }
        }

        //~GraphService()
        //{
        //    Dispose(disposing: false);
        //}

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}