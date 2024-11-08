namespace TestingPlatform.Domain.Interfaces
{
    public interface IApiService
    {
        Task<DateTime> GetTimeAsync();
    }
}