using System.Diagnostics;

using Microsoft.Extensions.Logging;

using TestingPlatform.Utilities;

namespace TestingPlatform.Services.Window
{
    public partial class WindowService
    {
        public enum Key
        {
            None = 0,
            Windows = 2,
            Alt_Plus_Tab = 4
        }

        public sealed class DangerousKeyPressedArgs
        {
            public Key Key { get; init; }

            public DateTime Time { get; init; } = DateTime.UtcNow;
#pragma warning disable IDE0290
            public DangerousKeyPressedArgs(Key key)
#pragma warning restore
            {
                Key = key;
            }
        }

        public bool IsMaximize
        {
            get => _isMaximize;
        }

        public bool IsHookInstalled
        {
            get => _isHookInstalled;
        }

        public static DangerousKeyPressedArgs DangerousKey
        {
            get
            {
                if (_dangerousKey == null)
                {
                    _dangerousKey = new(Key.None);
                }

                return _dangerousKey;
            }
            set
            {
                if (value == null)
                {
                    _dangerousKey = new(Key.None);
                    return;
                }

                _dangerousKey = value;
                OnDangerousKeyPressed();
            }
        }

        public static event EventHandler<DangerousKeyPressedArgs>? DangerousKeyPressed;

        public bool KeyboardSetHook()
        {
            try
            {
#if WINDOWS
                if (_hookID != IntPtr.Zero)
                {
                    bool unhookResult = UnhookWindowsHookEx(_hookID);
                    if (!unhookResult)
                    {
                        _logger.LogError("Failed to unhook the previous hook.");
                    }
                    _hookID = IntPtr.Zero;
                }

                string? moduleName = Process.GetCurrentProcess().MainModule?.ModuleName;

                if (string.IsNullOrEmpty(moduleName))
                {
                    return false;
                }

                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _callback, GetModuleHandle(moduleName), 0);

                _isHookInstalled = _hookID != IntPtr.Zero;

                _logger.LogInformation("Keyboard hook is set");

                return _isHookInstalled;
#else
                throw new NotImplementedException();
#endif
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                return false;
            }
        }

        public bool KeyboardUnhook()
        {
            try
            {
#if WINDOWS
                bool result = UnhookWindowsHookEx(_hookID);

                if (result)
                {
                    _hookID = IntPtr.Zero;
                    _isHookInstalled = false;
                    _dangerousKey = null;
                    _logger.LogInformation("Keyboard hook is unset");
                }

                return result;
#else
                throw new NotImplementedException();
#endif
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                return false;
            }
        }

        public bool SetFullScreen<TPage>(TPage? element) where TPage : Element
        {
            try
            {
#if WINDOWS
                var presenter = element
                    .GetParentWindow()
                    .GetAppWindow()
                    .GetWindowPresenter();

                presenter.IsMaximizable = !presenter.IsMaximizable;
                presenter.IsMinimizable = !presenter.IsMinimizable;
                presenter.SetBorderAndTitleBar(false, false);
                presenter.Maximize();

                _isMaximize = true;

                return true;
#else
                throw new NotImplementedException();
#endif
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                return false;
            }
        }

        public bool RestoreWindow<TPage>(TPage? element) where TPage : Element
        {
            try
            {
#if WINDOWS
                var presenter = element
                    .GetParentWindow()
                    .GetAppWindow()
                    .GetWindowPresenter();

                presenter.IsMaximizable = !presenter.IsMaximizable;
                presenter.IsMinimizable = !presenter.IsMinimizable;
                presenter.SetBorderAndTitleBar(true, true);
                presenter.Restore();

                _isMaximize = false;

                return true;
#else
                throw new NotImplementedException();
#endif
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                return false;
            }
        }
    }
}