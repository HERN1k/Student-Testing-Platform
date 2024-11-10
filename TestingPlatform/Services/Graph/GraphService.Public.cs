using System.Diagnostics;

using Azure.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

using Constants = TestingPlatform.Utilities.Constants;

namespace TestingPlatform.Services.Graph
{
    public partial class GraphService
    {
        public async Task AuthenticationAsync()
        {
            try
            {
                User user = await Client.Me.GetAsync() ?? throw new InvalidOperationException("Error loading user details");

                Preferences.Set(Constants.UserDisplayName, user.DisplayName);
                Preferences.Set(Constants.UserGivenName, user.GivenName);
                Preferences.Set(Constants.UserJobTitle, user.JobTitle);
                Preferences.Set(Constants.UserMail, user.Mail);
                Preferences.Set(Constants.UserSurname, user.Surname);
                Preferences.Set(Constants.UserID, user.Id);

                AccessToken token = await _interactiveCredential
                    .GetTokenAsync(new TokenRequestContext(
                        scopes: _apiScopes,
                        tenantId: _azureAd.Tenant
                    ));

                if (token.Token == null || string.IsNullOrWhiteSpace(token.Token))
                {
                    throw new InvalidOperationException("Error creating authorization client");
                }

                await SecureStorage.SetAsync(Constants.AccessToken, token.Token);
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
                _logger.LogError(ex, "An error occurred during operation");
#endif
                throw;
            }
        }

        public async Task UpdateTokenAsync()
        {
            try
            {
                AccessToken token = await _interactiveCredential
                    .GetTokenAsync(new TokenRequestContext(
                        scopes: _apiScopes,
                        tenantId: _azureAd.Tenant
                    ));

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
                throw;
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
                _logger.LogError(ex, "Error loading user details");
#endif
                throw;
            }
        }
    }
}