namespace TestingPlatform.Domain.Interfaces
{
    public interface IApiService : IDisposable
    {
        Task AuthenticationAsync();

        Task<string> TimeAsync(bool utc);
    }
}