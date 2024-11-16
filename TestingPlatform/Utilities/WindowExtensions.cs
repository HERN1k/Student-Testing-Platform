#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;

using WinRT.Interop;
#endif

namespace TestingPlatform.Utilities
{
    public static class WindowExtensions
    {
#if WINDOWS
        public static MauiWinUIWindow GetParentWindow<TPage>(this TPage? element) where TPage : Element
        {
            var page = element as ContentPage;

            ArgumentNullException.ThrowIfNull(page, nameof(element));

            var window = page.GetParentWindow();

            var mauiWindow = window.Handler.PlatformView as MauiWinUIWindow;

            ArgumentNullException.ThrowIfNull(mauiWindow, nameof(mauiWindow));

            return mauiWindow;
        }

        public static AppWindow GetAppWindow(this MauiWinUIWindow window)
        {
            ArgumentNullException.ThrowIfNull(window, nameof(window));

            return window
                .GetWindowHandle()
                .GetWindowIdFromWindow()
                .GetFromWindowId();
        }

        public static OverlappedPresenter GetWindowPresenter(this AppWindow window)
        {
            ArgumentNullException.ThrowIfNull(window, nameof(window));

            var presenter = window.Presenter as OverlappedPresenter;

            ArgumentNullException.ThrowIfNull(presenter, nameof(presenter));

            return presenter;
        }

        public static IntPtr GetWindowHandle(this MauiWinUIWindow window) => WindowNative.GetWindowHandle(window);

        public static WindowId GetWindowIdFromWindow(this IntPtr hwnd) => Win32Interop.GetWindowIdFromWindow(hwnd);

        public static AppWindow GetFromWindowId(this WindowId id) => AppWindow.GetFromWindowId(id);
#endif
    }
}