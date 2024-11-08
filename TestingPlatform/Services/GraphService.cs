using System.Diagnostics;

using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

using Constants = TestingPlatform.Utilities.Constants;

namespace TestingPlatform.Services
{
    public class GraphService : IGraphService, IDisposable
    {
        private InteractiveBrowserCredential? _interactiveCredential;

        public InteractiveBrowserCredential InteractiveCredential
        {
            get
            {
                if (_interactiveCredential == null)
                {
                    _interactiveCredential = InitializeInteractiveBrowser();
                }

                return _interactiveCredential;
            }
        }

        private GraphServiceClient? _client;

        public GraphServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = InitializeClientAsync().GetAwaiter().GetResult();
                }

                return _client;
            }
        }

        private readonly Msal _msal;
        private readonly ILogger<GraphService> _logger;
        private bool disposedValue;

        public GraphService(IOptions<Msal> options, ILogger<GraphService> logger)
        {
            _msal = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ArgumentException.ThrowIfNullOrWhiteSpace(_msal.ClientId, nameof(_msal.ClientId));
            ArgumentException.ThrowIfNullOrWhiteSpace(_msal.Tenant, nameof(_msal.Tenant));
            ArgumentException.ThrowIfNullOrWhiteSpace(_msal.RedirectUri, nameof(_msal.RedirectUri));
            ArgumentException.ThrowIfNullOrWhiteSpace(_msal.Instance, nameof(_msal.Instance));
            ArgumentException.ThrowIfNullOrWhiteSpace(_msal.GraphAPIEndpoint, nameof(_msal.GraphAPIEndpoint));
            if (_msal.Scopes == null || !_msal.Scopes.Any())
            {
                throw new ArgumentNullException(nameof(_msal.Scopes));
            }
            ArgumentException.ThrowIfNullOrWhiteSpace(_msal.Scopes.ToArray()[0], nameof(_msal.Scopes));
        }

        public async Task AuthorizationAsync()
        {
            try
            {
                User user = await Client.Me.GetAsync() ?? throw new InvalidOperationException("Error loading user details");

                Preferences.Clear();
                Preferences.Set(Constants.UserDisplayName, user.DisplayName);
                Preferences.Set(Constants.UserGivenName, user.GivenName);
                Preferences.Set(Constants.UserJobTitle, user.JobTitle);
                Preferences.Set(Constants.UserMail, user.Mail);
                Preferences.Set(Constants.UserSurname, user.Surname);
                Preferences.Set(Constants.UserID, user.Id);
            }
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
                _logger.LogError(ex, "An error occurred during operation");
#endif
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Error loading user details: {ex}");
#endif
                throw;
            }
        }

        public async Task UpdateToken()
        {
            try
            {
                AccessToken token = await InteractiveCredential
                    .GetTokenAsync(new TokenRequestContext(_msal.Scopes.ToArray()));

                if (token.Token == null || string.IsNullOrWhiteSpace(token.Token))
                {
                    throw new InvalidOperationException("Error updating authorization token");
                }

                await SecureStorage.SetAsync(Constants.AccessToken, token.Token);
            }
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Debug.WriteLine($"Error updating authorization token: {ex}");
                _logger.LogError(ex, "Error updating authorization token");
#endif
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Error updating authorization token: {ex}");
                _logger.LogError(ex, "Error updating authorization token");
#endif
                throw new InvalidOperationException("Error updating authorization token");
            }
        }

        public async Task<byte[]> GetMyPhotoAsync()
        {
            try
            {
                using (var stream = await Client.Me.Photo.Content.GetAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        if (stream is null)
                        {
                            return new byte[0];
                        }

                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Error loading user details: {ex}");
#endif
                throw;
            }
        }

        private InteractiveBrowserCredential InitializeInteractiveBrowser()
        {
            if (OperatingSystem.IsWindows())
            {
                var options = new InteractiveBrowserCredentialOptions
                {
                    TenantId = _msal.Tenant,
                    ClientId = _msal.ClientId,
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                    RedirectUri = new Uri(_msal.RedirectUri),
                    TokenCachePersistenceOptions = new()
                };

                return new(options);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private async Task<GraphServiceClient> InitializeClientAsync()
        {
            try
            {
                SecureStorage.RemoveAll();

                AccessToken token = await InteractiveCredential
                    .GetTokenAsync(new TokenRequestContext(_msal.Scopes.ToArray()));

                if (token.Token == null || string.IsNullOrWhiteSpace(token.Token))
                {
                    throw new InvalidOperationException("Error creating authorization client");
                }

                await SecureStorage.SetAsync(Constants.AccessToken, token.Token);

                return new(InteractiveCredential, _msal.Scopes.ToArray());
            }
            catch (InvalidOperationException ex)
            {
#if DEBUG
                Debug.WriteLine($"Error creating authorization client: {ex}");
                _logger.LogError(ex, "Error creating authorization client");
#endif
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Error creating authorization client: {ex}");
                _logger.LogError(ex, "Error creating authorization client");
#endif
                throw new InvalidOperationException("Error creating authorization client");
            }
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