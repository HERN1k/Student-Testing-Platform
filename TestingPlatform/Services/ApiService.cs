using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Domain.Json;
using TestingPlatform.Utilities;

namespace TestingPlatform.Services
{
    public class ApiService : IApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IGraphService _graph;
        private readonly ApiConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };
        private bool disposedValue;

        public ApiService(
                IHttpClientFactory httpClientFactory,
                IGraphService graph,
                IOptions<ApiConfiguration> options,
                ILogger<ApiService> logger
            )
        {
            _httpClient = httpClientFactory.CreateClient(Constants.ApiHttpClient) ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _configuration = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ArgumentException.ThrowIfNullOrWhiteSpace(_configuration.Uri, nameof(_configuration.Uri));
        }

        public async Task<DateTime> GetTimeAsync()
        {
            var qb = new QueryBuilder([new KeyValuePair<string, string>("utc", bool.TrueString)]);
            var uri = new Uri($"{_configuration.Uri}{Constants.ApiClientTime}{qb}");
            var response = await HttpGetAsync<ApiClientTimeModel>(uri);
            return response.Time ?? DateTime.UnixEpoch;
        }

        private static void ThrowIfIncorrectUri(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri, nameof(uri));
            ArgumentException.ThrowIfNullOrWhiteSpace(uri.ToString(), nameof(uri));

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException("The URI must be absolute.", nameof(uri));
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException("The URI must use either HTTP or HTTPS scheme.", nameof(uri));
            }
        }

        private async Task SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            ArgumentNullException.ThrowIfNull(requestMessage, nameof(requestMessage));

            string token = await SecureStorage.GetAsync(Constants.AccessToken) ?? throw new ArgumentNullException("Access token not found");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.ApiAuthScheme, token);
        }

        private void SetRequestBody<TValue>(HttpRequestMessage requestMessage, TValue body) where TValue : class
        {
            ArgumentNullException.ThrowIfNull(requestMessage, nameof(requestMessage));
            ArgumentNullException.ThrowIfNull(body, nameof(body));

            string requestbody = JsonSerializer.Serialize<TValue>(body) ?? throw new ArgumentNullException("Request body is empty");

            requestMessage.Content = new StringContent(requestbody, Encoding.UTF8, "application/json");
        }

        private async Task<TModel> SendRequestWithRetryAsync<TModel>(HttpRequestMessage message) where TModel : class
        {
            var maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                using (var response = await _httpClient.SendAsync(message))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await JsonSerializer.DeserializeAsync<TModel>(response.Content.ReadAsStream(), _jsonOptions)
                               ?? throw new ArgumentNullException("No data received");
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await _graph.UpdateToken();
                        await SetAuthorizationHeader(message);
                        retryCount++;
                        continue;
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }

            throw new InvalidOperationException("Request failed after multiple attempts.");
        }

        private async Task<TModel> HttpGetAsync<TModel>(Uri uri) where TModel : class
        {
            try
            {
                ThrowIfIncorrectUri(uri);

                var message = new HttpRequestMessage(HttpMethod.Get, uri);
                await SetAuthorizationHeader(message);

                return await SendRequestWithRetryAsync<TModel>(message);
            }
            catch (ArgumentNullException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw;
            }
            catch (ArgumentException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw;
            }
            catch (HttpRequestException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                _logger.LogError(ex, "An error occurred during operation");
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                _logger.LogError(ex, "An error occurred during operation");
                throw;
            }
        }

        private async Task<TModel> HttpPostAsync<TModel, TValue>(Uri uri, TValue body) where TModel : class where TValue : class
        {
            try
            {
                ThrowIfIncorrectUri(uri);

                var message = new HttpRequestMessage(HttpMethod.Post, uri);
                await SetAuthorizationHeader(message);
                SetRequestBody(message, body);

                return await SendRequestWithRetryAsync<TModel>(message);
            }
            catch (ArgumentNullException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw;
            }
            catch (ArgumentException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                throw;
            }
            catch (HttpRequestException ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                _logger.LogError(ex, "An error occurred during operation");
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                _logger.LogError(ex, "An error occurred during operation");
                throw;
            }
        }

        #region Disposing
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                { }

                disposedValue = true;
            }
        }

        //~ApiService()
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