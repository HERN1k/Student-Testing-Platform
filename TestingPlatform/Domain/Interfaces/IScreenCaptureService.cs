namespace TestingPlatform.Domain.Interfaces
{
    public interface IScreenCaptureService
    {
        bool OneWorkingScreen();
        string CreateScreenCapture();
    }
}