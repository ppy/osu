// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

namespace osu.Desktop.Linux
{
    [SupportedOSPlatform("linux")]
    public static class XdgUtils
    {
        [DllImport("libc", SetLastError = true)]
        private static extern uint getuid();

        /// <summary>
        /// Get current Linux user ID.
        /// </summary>
        public static uint GetUserID()
        {
            return getuid();
        }

        /// <summary>
        /// Get XDG Runtime Directory.
        /// </summary>
        public static string GetXdgRuntimeDir()
        {
            string? runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");

            return runtimeDir ?? $"/run/user/{GetUserID()}";
        }
    }
}
