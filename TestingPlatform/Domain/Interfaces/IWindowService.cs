namespace TestingPlatform.Domain.Interfaces
{
    public interface IWindowService : IDisposable
    {
        bool IsMaximize { get; }

        bool IsHookInstalled { get; }

        bool KeyboardSetHook();

        bool KeyboardUnhook();

        bool SetFullScreen<TPage>(TPage? element) where TPage : Element;

        bool RestoreWindow<TPage>(TPage? element) where TPage : Element;
    }
}