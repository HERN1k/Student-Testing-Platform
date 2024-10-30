using Microsoft.Identity.Client;

namespace TestingPlatform.Domain.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult?> SignInAsync();
        Task SignOutAsync();
        Task<TModel?> HttpGet<TModel>(string endpoint) where TModel : class;
        Task<byte[]> HttpGetArrayByte(string endpoint);
        IPublicClientApplication PublicClientApp { get; }
    }
}