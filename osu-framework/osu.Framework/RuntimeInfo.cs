//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace osu.Framework
{
    public static class RuntimeInfo
    {
        [DllImport(@"kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public static bool Is32Bit { get; private set; }
        public static bool Is64Bit { get; private set; }
        public static bool IsMono { get; private set; }
        public static bool IsWindows { get; private set; }
        public static bool IsUnix { get; private set; }
        public static bool IsLinux { get; private set; }
        public static bool IsMacOSX { get; private set; }
        public static bool IsWine { get; private set; }

        static RuntimeInfo()
        {
            IsMono = Type.GetType("Mono.Runtime") != null;
            int p = (int)Environment.OSVersion.Platform;
            IsUnix = (p == 4) || (p == 6) || (p == 128);
            IsWindows = Path.DirectorySeparatorChar == '\\';

            Is32Bit = IntPtr.Size == 4;
            Is64Bit = IntPtr.Size == 8;

            if (IsUnix)
            {
                Process uname = new Process();
                uname.StartInfo.FileName = "uname";
                uname.StartInfo.UseShellExecute = false;
                uname.StartInfo.RedirectStandardOutput = true;
                uname.Start();
                string output = uname.StandardOutput.ReadToEnd();
                uname.WaitForExit();

                output = output.ToUpper().Replace("\n", "").Trim();

                IsMacOSX = output == "DARWIN";
                IsLinux = output == "LINUX";
            }
            else
            {
                IsMacOSX = false;
                IsLinux = false;
            }

            if (IsWindows)
            {
                IntPtr hModule = GetModuleHandle(@"ntdll.dll");
                if (hModule == IntPtr.Zero)
                    IsWine = false;
                else
                {
                    IntPtr fptr = GetProcAddress(hModule, @"wine_get_version");
                    IsWine = fptr != IntPtr.Zero;
                }
            }
        }
    }
}

