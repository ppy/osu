// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using Microsoft.Win32;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;

namespace osu.Desktop.Platform.Windows
{
    internal class WindowsOsuGameDesktop : OsuGameDesktop
    {
        public WindowsOsuGameDesktop(string[] args = null)
            : base(args)
        {
        }

        public override Storage GetStorageForStableInstall()
        {
            try
            {
                if (Host is DesktopGameHost desktopHost)
                    return new StableStorage(desktopHost);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while searching for stable install");
            }

            return null;
        }

        /// <summary>
        /// A method of accessing an osu-stable install in a controlled fashion.
        /// </summary>
        private class StableStorage : WindowsStorage
        {
            protected override string LocateBasePath()
            {
                bool checkExists(string p) => Directory.Exists(Path.Combine(p, "Songs"));

                string stableInstallPath;

                try
                {
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                        stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty).ToString().Split('"')[1].Replace("osu!.exe", "");

                    if (checkExists(stableInstallPath))
                        return stableInstallPath;
                }
                catch
                {
                }

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                return null;
            }

            public StableStorage(DesktopGameHost host)
                : base(string.Empty, host)
            {
            }
        }
    }
}
