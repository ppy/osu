//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using osu.Framework.Framework;

namespace osu.Framework.Desktop.OS.Windows.Native
{
    static class Desktop
    {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DeviceMode devMode);

        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettingsEx(string deviceName, ref DeviceMode devMode, IntPtr hWnd, int flags, IntPtr lParam);

        /// <summary>
        /// Prototype required for providing the null pointer as devmode for resolution reset.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettingsEx(string deviceName, IntPtr devMode, IntPtr hWnd, int flags, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo monitorInfo);

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        const int MONITOR_DEFAULTTONULL = 0;
        const int MONITOR_DEFAULTTOPRIMARY = 1;
        const int MONITOR_DEFAULTTONEAREST = 2;

        const int ENUM_REGISTRY_SETTINGS = -2;

        internal const int CDS_FULLSCREEN = 0x04;
        internal const int CDS_TEST = 0x02;

        internal const int DISP_CHANGE_FAILED = -1;
        internal const int DISP_CHANGE_RESTART = 1;
        internal const int DISP_CHANGE_SUCCESSFUL = 0;
        internal const int ENUM_CURRENT_SETTINGS = -1;

        internal const int DM_BITSPERPEL = 0x00040000;
        internal const int DM_PELSWIDTH = 0x00080000;
        internal const int DM_PELSHEIGHT = 0x00100000;
        internal const int DM_DISPLAYFREQUENCY = 0x00400000;

        private static Window window;

        internal static void InitializeWindow(Window window)
        {
            Desktop.window = window;
        }

        internal static bool ChangeResolution(int width, int height, int? refreshRate = null, bool testOnly = false)
        {
            MonitorInfo monitorInfo = new MonitorInfo() { Size = 72 };
            IntPtr monitor = MonitorFromWindow(Game.Window.Handle, MONITOR_DEFAULTTONEAREST);

            GetMonitorInfo(monitor, ref monitorInfo);

            DeviceMode dm = new DeviceMode();
            dm.dmSize = (short)Marshal.SizeOf(dm);

            bool success = false;

            if (!EnumDisplaySettings(monitorInfo.DeviceName, ENUM_CURRENT_SETTINGS, ref dm))
                return false;

            // Are we already on the desired resolution? If yes we don't need to do anything
            if (dm.dmPelsWidth == width && dm.dmPelsHeight == height && (!refreshRate.HasValue || refreshRate.Value == dm.dmDisplayFrequency))
                return true;

            // At this point we are sure we need a custom resolution change.
            dm.dmPelsWidth = width;
            dm.dmPelsHeight = height;

            dm.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;

            if (refreshRate.HasValue)
            {
                dm.dmDisplayFrequency = refreshRate.Value;
                dm.dmFields |= DM_DISPLAYFREQUENCY;
            }

            success = ChangeDisplaySettingsEx(monitorInfo.DeviceName, ref dm, IntPtr.Zero, CDS_TEST, IntPtr.Zero) == DISP_CHANGE_SUCCESSFUL;

            if (testOnly)
                return success;

            success &= ChangeDisplaySettingsEx(monitorInfo.DeviceName, ref dm, IntPtr.Zero, CDS_FULLSCREEN, IntPtr.Zero) == DISP_CHANGE_SUCCESSFUL;
            return success;
        }

        internal static bool ResetResolution()
        {
            MonitorInfo monitorInfo = new MonitorInfo() { Size = 72 };
            IntPtr monitor = MonitorFromWindow(window.Handle, MONITOR_DEFAULTTONEAREST);

            GetMonitorInfo(monitor, ref monitorInfo);

            DeviceMode dmCurrent = new DeviceMode();
            dmCurrent.dmSize = (short)Marshal.SizeOf(dmCurrent);

            DeviceMode dmRegistry = new DeviceMode();
            dmRegistry.dmSize = (short)Marshal.SizeOf(dmRegistry);

            if (!EnumDisplaySettings(monitorInfo.DeviceName, ENUM_CURRENT_SETTINGS, ref dmCurrent) || !EnumDisplaySettings(monitorInfo.DeviceName, ENUM_REGISTRY_SETTINGS, ref dmRegistry))
                return false;

            // No need to reset if we already have the settings that we want.
            if (dmCurrent.dmPelsWidth == dmRegistry.dmPelsWidth && dmCurrent.dmPelsHeight == dmRegistry.dmPelsHeight && dmCurrent.dmDisplayFrequency == dmRegistry.dmDisplayFrequency)
                return true;

            return ChangeDisplaySettingsEx(monitorInfo.DeviceName, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero) == DISP_CHANGE_SUCCESSFUL;
        }

        internal static List<Size> GetResolutions()
        {
            MonitorInfo monitorInfo = new MonitorInfo() { Size = 72 };
            IntPtr monitor = MonitorFromWindow(window.Handle, MONITOR_DEFAULTTONEAREST);
            GetMonitorInfo(monitor, ref monitorInfo);

            List<Size> results = new List<Size>();

            DeviceMode vDevMode = new DeviceMode();

            int i = 0;
            while (EnumDisplaySettings(monitorInfo.DeviceName, i++, ref vDevMode))
                results.Add(new Size(vDevMode.dmPelsWidth, vDevMode.dmPelsHeight));

            return results;
        }

        internal static Size FindNativeResolution()
        {
            List<Size> resolutions = GetResolutions();

            Size native_res = new Size(640, 480);
            foreach (Size res in resolutions)
                if ((res.Width > native_res.Width) || ((res.Width == native_res.Width) && (res.Height > native_res.Height))) native_res = res;

            return native_res;
        }

        // http://www.dotnetspark.com/kb/1948-change-display-settings-programmatically.aspx
        private static DeviceMode? GetCurrentSettings()
        {
            MonitorInfo monitorInfo = new MonitorInfo() { Size = 72 };
            IntPtr monitor = MonitorFromWindow(window.Handle, MONITOR_DEFAULTTONEAREST);
            GetMonitorInfo(monitor, ref monitorInfo);

            DeviceMode mode = new DeviceMode();
            mode.dmSize = (short)Marshal.SizeOf(mode);

            if (EnumDisplaySettings(monitorInfo.DeviceName, ENUM_CURRENT_SETTINGS, ref mode) != true)
                return null;

            return mode;
        }

        internal static int GetCurrentRefreshRate()
        {
            DeviceMode? mode = GetCurrentSettings();
            if (mode == null) return 0;

            return ((DeviceMode)mode).dmDisplayFrequency;
        }

        internal static Size GetDesktopResolution()
        {
            MonitorInfo monitorInfo = new MonitorInfo() { Size = 40 };

            IntPtr monitor = MonitorFromWindow(window.Handle, MONITOR_DEFAULTTONEAREST);

            GetMonitorInfo(monitor, ref monitorInfo);
            return new Size(monitorInfo.Monitor.Right - monitorInfo.Monitor.Left, monitorInfo.Monitor.Bottom - monitorInfo.Monitor.Top);
        }

        internal static Point GetDesktopPosition()
        {
            MonitorInfo monitorInfo = new MonitorInfo() { Size = 40 };

            IntPtr monitor = MonitorFromWindow(window.Handle, MONITOR_DEFAULTTONEAREST);

            GetMonitorInfo(monitor, ref monitorInfo);

            return new Point(monitorInfo.Monitor.Left, monitorInfo.Monitor.Top);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MonitorInfo
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFO) (40) before calling the GetMonitorInfo function. 
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            internal int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            internal RectStruct Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications, 
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor. 
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            internal RectStruct WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            /// 
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
            internal uint Flags;

            /// <summary>
            /// The monitor device name.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal String DeviceName;

            internal void Init()
            {
                this.Size = 72;
            }
        }

        /// <summary>
        /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
        /// <remarks>
        /// By convention, the right and bottom edges of the rectangle are normally considered exclusive. 
        /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle. 
        /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including, 
        /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct RectStruct
        {
            /// <summary>
            /// The x-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            internal int Left;

            /// <summary>
            /// The y-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            internal int Top;

            /// <summary>
            /// The x-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            internal int Right;

            /// <summary>
            /// The y-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            internal int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DeviceMode
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string dmDeviceName;
            internal short dmSpecVersion;
            internal short dmDriverVersion;
            internal short dmSize;
            internal short dmDriverExtra;
            internal int dmFields;

            internal short dmOrientation;
            internal short dmPaperSize;
            internal short dmPaperLength;
            internal short dmPaperWidth;

            internal short dmScale;
            internal short dmCopies;
            internal short dmDefaultSource;
            internal short dmPrintQuality;
            internal short dmColor;
            internal short dmDuplex;
            internal short dmYResolution;
            internal short dmTTOption;
            internal short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string dmFormName;
            internal short dmLogPixels;
            internal short dmBitsPerPel;
            internal int dmPelsWidth;
            internal int dmPelsHeight;

            internal int dmDisplayFlags;
            internal int dmDisplayFrequency;

            internal int dmICMMethod;
            internal int dmICMIntent;
            internal int dmMediaType;
            internal int dmDitherType;
            internal int dmReserved1;
            internal int dmReserved2;

            internal int dmPanningWidth;
            internal int dmPanningHeight;
        }
    }
}
