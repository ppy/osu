//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;

namespace osu.Framework.Desktop.OS.Windows.Native
{
    internal static class Architecture
    {
        private static string nativeIncludePath => $@"{Environment.CurrentDirectory}/{arch}/";
        private static string arch => Is64Bit ? @"x64" : @"x86";

        internal static bool Is64Bit => IntPtr.Size == 8;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetDllDirectory(string lpPathName);

        internal static void SetIncludePath()
        {
            SetDllDirectory(nativeIncludePath);
        }
    }
}
