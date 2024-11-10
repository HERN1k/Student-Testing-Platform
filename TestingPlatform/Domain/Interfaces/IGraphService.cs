namespace TestingPlatform.Domain.Interfaces
{
    public interface IGraphService : IDisposable
    {
        Task AuthenticationAsync();

        Task UpdateTokenAsync();

        Task<byte[]> GetMyPhotoAsync();
    }
}