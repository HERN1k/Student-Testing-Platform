using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TestingPlatform.Domain.Dto;
using TestingPlatform.Domain.Interfaces;
using TestingPlatform.Utilities;

namespace TestingPlatform.Services.Api
{
    public partial class ApiService : IApiService
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
        private readonly int _maxRetries = 3;
        private bool disposedValue;

        public ApiService(IHttpClientFactory httpClientFactory, IGraphService graph, IOptions<ApiConfiguration> options, ILogger<ApiService> logger)
        {
            ValidateConstructorArguments(httpClientFactory, graph, options, logger);
            _configuration = SetApiConfiguration(options);
            _httpClient = httpClientFactory.CreateClient(Constants.ApiHttpClient);
            _graph = graph;
            _logger = logger;
        }

        private static void ValidateConstructorArguments(IHttpClientFactory httpClientFactory, IGraphService graph, IOptions<ApiConfiguration> options, ILogger<ApiService> logger)
        {
            ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));
            ArgumentNullException.ThrowIfNull(graph, nameof(graph));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        }

        private static ApiConfiguration SetApiConfiguration(IOptions<ApiConfiguration> options)
        {
            ApiConfiguration configuration = options.Value;

            ArgumentException.ThrowIfNullOrWhiteSpace(configuration.Uri, nameof(options));

            return configuration;
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

        private static void SetRequestBody<TValue>(HttpRequestMessage requestMessage, TValue body) where TValue : class
        {
            ArgumentNullException.ThrowIfNull(requestMessage, nameof(requestMessage));
            ArgumentNullException.ThrowIfNull(body, nameof(body));

            string requestbody = JsonSerializer.Serialize(body) ?? throw new ArgumentNullException("Request body is empty");

            requestMessage.Content = new StringContent(requestbody, Encoding.UTF8, "application/json");
        }

        private static async Task SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            ArgumentNullException.ThrowIfNull(requestMessage, nameof(requestMessage));

            string token = await SecureStorage.GetAsync(Constants.AccessToken) ?? throw new ArgumentNullException("Access token not found");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.ApiAuthScheme, token);
        }

        private async Task<TModel> SendRequestWithRetryAsync<TModel>(HttpRequestMessage message) where TModel : ApiClientResponse.ResponseBase
        {
            int retryCount = 0;

            while (retryCount < _maxRetries)
            {
                using (var response = await _httpClient.SendAsync(message))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        TModel data = await JsonSerializer.DeserializeAsync<TModel>(response.Content.ReadAsStream(), _jsonOptions)
                            ?? throw new InvalidOperationException("No data received");

                        if (!int.TryParse(data.Status, CultureInfo.InvariantCulture, out int code))
                        {
                            throw new InvalidOperationException("No data received");
                        }

                        if (code == 401)
                        {
                            await _graph.UpdateTokenAsync();
                            await SetAuthorizationHeader(message);
                            retryCount++;
                            continue;
                        }

                        if (code != 200)
                        {
                            throw new InvalidOperationException("No data received");
                        }

                        return data;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await _graph.UpdateTokenAsync();
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

        private async Task<TModel> HttpGetAsync<TModel>(Uri uri) where TModel : ApiClientResponse.ResponseBase
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

        private async Task<TModel> HttpPostAsync<TModel, TValue>(Uri uri, TValue body) where TModel : ApiClientResponse.ResponseBase where TValue : class
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