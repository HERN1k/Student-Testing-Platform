using Azure.Identity;

using Microsoft.Graph;

namespace TestingPlatform.Domain.Interfaces
{
    public interface IGraphService
    {
        InteractiveBrowserCredential InteractiveCredential { get; }

        GraphServiceClient Client { get; }

        Task AuthorizationAsync();

        Task UpdateToken();

        Task<byte[]> GetMyPhotoAsync();
    }
}