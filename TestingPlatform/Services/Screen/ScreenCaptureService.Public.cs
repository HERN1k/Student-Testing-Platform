using System.Runtime.InteropServices;

namespace TestingPlatform.Services.Screen
{
    public sealed partial class ScreenCaptureService
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Int32 Left;
            public Int32 Top;
            public Int32 Right;
            public Int32 Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public Int32 cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public Int32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        public struct ScreenBounds
        {
            public System.Drawing.Point Position;
            public Int32 Width;
            public Int32 Height;
        }

        public bool OneWorkingScreen()
        {
            if (OperatingSystem.IsWindows())
            {
                return OneWorkingScreenWindows();
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                throw new NotImplementedException();
            }
            else if (OperatingSystem.IsLinux())
            {
                throw new NotImplementedException();
            }
            else if (OperatingSystem.IsAndroid())
            {
                return true;
            }
            else if (OperatingSystem.IsIOS())
            {
                return true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string CreateScreenCapture()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsScreenCaptureImplAsync();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}