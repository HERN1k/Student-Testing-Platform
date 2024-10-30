using System.Net.Http.Headers;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

using Newtonsoft.Json;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Msal _msal;
        private readonly IPublicClientApplication _app;

        public IPublicClientApplication PublicClientApp { get => _app; }

        public AuthenticationService(IOptions<Msal> options)
        {
            _msal = options.Value;
            _app = CreatePublicClientApplication();
        }

        public async Task<AuthenticationResult?> SignInAsync()
        {
            AuthenticationResult? authResult = null;

            IAccount firstAccount = (await _app.GetAccountsAsync()).FirstOrDefault()
                ?? PublicClientApplication.OperatingSystemAccount;

            try
            {
                authResult = await _app.AcquireTokenSilent(_msal.Scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                authResult = await _app.AcquireTokenInteractive(_msal.Scopes)
                    .WithAccount(firstAccount)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
            }

            if (authResult != null)
                await SecureStorage.SetAsync(Constants.AccessToken, authResult.AccessToken);

            return authResult;
        }

        public async Task SignOutAsync()
        {
            IEnumerable<IAccount> accounts = await _app.GetAccountsAsync();
            if (accounts.Any())
            {
                await _app.RemoveAsync(accounts.FirstOrDefault());
            }
        }

        public async Task<TModel?> HttpGet<TModel>(string endpoint) where TModel : class
        {
            if (string.IsNullOrEmpty(endpoint))
                return null;

            string? accessToken = await SecureStorage.GetAsync(Constants.AccessToken);

            if (accessToken is null)
                return null;

            try
            {
                using (HttpClient httpClient = new())
                {
                    HttpResponseMessage response;
                    HttpRequestMessage request = new(HttpMethod.Get, $"{_msal.GraphAPIEndpoint}{endpoint}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TModel>(responseBody);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<byte[]> HttpGetArrayByte(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                return new byte[0];

            string? accessToken = await SecureStorage.GetAsync(Constants.AccessToken);

            if (accessToken is null)
                return new byte[0];

            try
            {
                using (HttpClient httpClient = new())
                {
                    HttpResponseMessage response;
                    HttpRequestMessage request = new(HttpMethod.Get, $"{_msal.GraphAPIEndpoint}{endpoint}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        private IPublicClientApplication CreatePublicClientApplication()
        {
            IPublicClientApplication app = PublicClientApplicationBuilder.Create(_msal.ClientId)
                .WithAuthority($"{_msal.Instance}{_msal.Tenant}")
                .WithRedirectUri(_msal.RedirectUri)
                .Build();

            MsalCacheHelper cacheHelper = CreateCacheHelperAsync().GetAwaiter().GetResult();

            cacheHelper.RegisterCache(app.UserTokenCache);

            return app;
        }

        private async Task<MsalCacheHelper> CreateCacheHelperAsync()
        {
            StorageCreationProperties storageProperties = new StorageCreationPropertiesBuilder(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".msalcache.bin",
                    MsalCacheHelper.UserRootDirectory
                ).Build();

            MsalCacheHelper cacheHelper = await MsalCacheHelper.CreateAsync(
                    storageProperties,
                    new System.Diagnostics.TraceSource("MSAL.CacheTrace")
                ).ConfigureAwait(false);

            return cacheHelper;
        }
    }
}