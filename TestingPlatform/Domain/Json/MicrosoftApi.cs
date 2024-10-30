using Newtonsoft.Json;

namespace TestingPlatform.Domain.Json
{
    public record Me(
        [property: JsonProperty("@odata.context")] string? Context,
        [property: JsonProperty("displayName")] string? DisplayName,
        [property: JsonProperty("givenName")] string? GivenName,
        [property: JsonProperty("jobTitle")] string? JobTitle,
        [property: JsonProperty("mail")] string? Mail,
        [property: JsonProperty("mobilePhone")] string? MobilePhone,
        [property: JsonProperty("officeLocation")] string? OfficeLocation,
        [property: JsonProperty("preferredLanguage")] string? PreferredLanguage,
        [property: JsonProperty("surname")] string? Surname,
        [property: JsonProperty("userPrincipalName")] string? UserPrincipalName,
        [property: JsonProperty("id")] string? ID);
}