// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using osu.Framework.Logging;

namespace osu.Desktop.Linux
{
    [SupportedOSPlatform("linux")]
    public static class XDGUtils
    {
        [DllImport("libc", SetLastError = true)]
        private static extern uint getuid();

        public static uint GetUserID()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Exception notSupportedException = new PlatformNotSupportedException();
                Logger.Error(notSupportedException, "You're not running this code on Linux!");
                throw notSupportedException;
            }

            return getuid();
        }

        public static string GetXdg
    }
}
