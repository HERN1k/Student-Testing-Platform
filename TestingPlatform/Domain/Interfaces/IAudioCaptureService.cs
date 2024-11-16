namespace TestingPlatform.Domain.Interfaces
{
    public interface IAudioCaptureService
    {
        Task<string> RecordAsync();
    }
}