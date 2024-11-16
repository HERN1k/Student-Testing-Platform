using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using TestingPlatform.Domain.Interfaces;

namespace TestingPlatform.Services.Screen
{
    public sealed partial class ScreenCaptureService : IScreenCaptureService
    {
        [LibraryImport("user32.dll", EntryPoint = "EnumDisplayMonitors", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        /*
            Here we use DllImport because the code generation for P/Invoke 
            in .NET does not support marshalling of complex types.
         */
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfo", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
#pragma warning restore

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        private readonly ILogger<ScreenCaptureService> _logger;

        private readonly string _tempFilePath = Path.Combine(Path.GetTempPath(), "ScreenData.png");
#pragma warning disable IDE0290
        public ScreenCaptureService(ILogger<ScreenCaptureService> logger)
#pragma warning restore
        {
            ValidateConstructorArguments(logger);
            _logger = logger;
        }

        private static void ValidateConstructorArguments(ILogger<ScreenCaptureService> logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        }

        private bool OneWorkingScreenWindows()
        {
            int monitorCount = 0;

            bool CallBack(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                monitorCount++;
                return true;
            }

            if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, CallBack, IntPtr.Zero))
            {
                throw new InvalidOperationException("Failed to get information about monitor/monitors");
            }

            if (monitorCount > 1)
            {
                return false;
            }

            return true;
        }

        private ScreenBounds GetTotalScreenBoundsOrDefault()
        {
            int monitorCount = 0;
            Rect totalBounds = new()
            {
                Left = int.MaxValue,
                Top = int.MaxValue,
                Right = int.MinValue,
                Bottom = int.MinValue
            };

            bool CallBack(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                var monitorInfo = new MONITORINFOEX() { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };

                if (GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    monitorCount++;

                    totalBounds.Left = Math.Min(totalBounds.Left, monitorInfo.rcMonitor.Left);
                    totalBounds.Top = Math.Min(totalBounds.Top, monitorInfo.rcMonitor.Top);
                    totalBounds.Right = Math.Max(totalBounds.Right, monitorInfo.rcMonitor.Right);
                    totalBounds.Bottom = Math.Max(totalBounds.Bottom, monitorInfo.rcMonitor.Bottom);

                    return true;
                }

                return false;
            }

            if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, CallBack, IntPtr.Zero))
            {
                throw new InvalidOperationException("Failed to get information about monitor/monitors");
            }

            if (monitorCount > 1)
            {
                throw new ApplicationException("During testing, only 1 screen should work");
            }

            return new ScreenBounds()
            {
                Position = new System.Drawing.Point(totalBounds.Left, totalBounds.Top),
                Height = totalBounds.Bottom - totalBounds.Top,
                Width = totalBounds.Right - totalBounds.Left
            };
        }

        private string WindowsScreenCaptureImplAsync()
        {
            try
            {
                var bounds = GetTotalScreenBoundsOrDefault();

#pragma warning disable CA1416
                using var bitmap = new Bitmap(bounds.Width, bounds.Height);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.CopyFromScreen(
                    upperLeftSource: bounds.Position,
                    upperLeftDestination: bounds.Position,
                    blockRegionSize: new System.Drawing.Size(bounds.Width, bounds.Height));

                bitmap.Save(_tempFilePath);
#pragma warning restore

                return _tempFilePath;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
                return string.Empty;
            }
        }
    }
}