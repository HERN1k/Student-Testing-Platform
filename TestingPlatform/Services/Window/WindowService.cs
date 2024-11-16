using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;
#if WINDOWS
using Microsoft.UI.Xaml.Controls;
#endif

using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Services.Window
{
    public partial class WindowService : IWindowService
    {
#if WINDOWS
        [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
        private static partial IntPtr SetWindowsHookEx(Int32 idHook, CBTProc lpfn, IntPtr hMod, UInt32 dwThreadId);

        [LibraryImport("user32.dll", EntryPoint = "UnhookWindowsHookEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnhookWindowsHookEx(IntPtr hhk);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr CallNextHookEx(IntPtr hhk, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr GetModuleHandle(string lpModuleName);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial short GetAsyncKeyState(Int32 vKey);

        private delegate IntPtr CBTProc(Int32 nCode, IntPtr wParam, IntPtr lParam);

        private static CBTProc _callback = new(HookCallback);

        private static IntPtr _hookID = IntPtr.Zero;
#endif

#pragma warning disable IDE0044
        private bool _isMaximize = false;

        private bool _isHookInstalled = false;

        private static DangerousKeyPressedArgs? _dangerousKey;

        private bool disposedValue;
#pragma warning restore
        private readonly ILogger<WindowService> _logger;

        #region Constants
        private const Int32 WH_KEYBOARD_LL = 13;
        private const Int32 WM_KEYDOWN = 0x100;
        private const Int32 WM_SYSKEYDOWN = 0x104;
        private const Int32 VK_LWIN = 0x5B;
        private const Int32 VK_RWIN = 0x5C;
        private const Int32 VK_MENU = 0x12;
        private const Int32 VK_TAB = 0x09;
        #endregion

#pragma warning disable IDE0290
        public WindowService(ILogger<WindowService> logger)
#pragma warning restore
        {
            ValidateConstructorArguments(logger);
            _logger = logger;
        }

        private static void ValidateConstructorArguments(ILogger<WindowService> logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        }

        private static void OnDangerousKeyPressed()
        {
            DangerousKeyPressed?.Invoke(null, DangerousKey);
        }

#if WINDOWS
        private static IntPtr HookCallback(Int32 nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                Int32 vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == VK_LWIN || vkCode == VK_RWIN)
                {
                    DangerousKey = new(Key.Windows);
                }
                else if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0 && vkCode == VK_TAB)
                {
                    DangerousKey = new(Key.Alt_Plus_Tab);
                }
            }

            if (_hookID != IntPtr.Zero)
            {
                IntPtr result = CallNextHookEx(_hookID, nCode, wParam, lParam);

                Marshal.GetLastWin32Error();

                return result;
            }

            return IntPtr.Zero;
        }
#endif

        #region Disposing
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dangerousKey = null;
                }
#if WINDOWS
                if (_isHookInstalled)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }
#endif

                disposedValue = true;
            }
        }

        ~WindowService()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}