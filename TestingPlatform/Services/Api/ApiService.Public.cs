using Microsoft.AspNetCore.Http.Extensions;

using TestingPlatform.Domain.Dto;
using TestingPlatform.Utilities;

namespace TestingPlatform.Services.Api
{
    public partial class ApiService
    {
        public async Task AuthenticationAsync()
        {
            var uri = new Uri($"{_configuration.Uri}{Constants.ApiClientAuthentication}");

            var requestBody = new ApiClientRequest.Authentication(
                    Id: Preferences.Get(Constants.UserID, string.Empty),
                    DisplayName: Preferences.Get(Constants.UserDisplayName, string.Empty),
                    Name: Preferences.Get(Constants.UserGivenName, string.Empty),
                    Surname: Preferences.Get(Constants.UserSurname, string.Empty),
                    Mail: Preferences.Get(Constants.UserMail, string.Empty)
                );

            await HttpPostAsync<ApiClientResponse.Authentication, ApiClientRequest.Authentication>(uri, requestBody);
        }

        public async Task<string> TimeAsync(bool utc)
        {
            var qb = new QueryBuilder([new KeyValuePair<string, string>("utc", utc.ToString())]);
            var uri = new Uri($"{_configuration.Uri}{Constants.ApiClientTime}{qb}");
            var response = await HttpGetAsync<ApiClientResponse.Time>(uri);
            return response.Data?.Time ?? DateTime.UnixEpoch.ToString();
        }
    }
}