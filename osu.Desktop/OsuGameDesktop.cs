// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using osu.Desktop.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game;
using OpenTK.Input;
using Microsoft.Win32;
using osu.Desktop.Updater;
using osu.Framework;
using osu.Framework.Platform.Windows;

namespace osu.Desktop
{
    internal class OsuGameDesktop : OsuGame
    {
        private readonly bool noVersionOverlay;

        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
            noVersionOverlay = args?.Any(a => a == "--no-version-overlay") ?? false;
        }

        public override Storage GetStorageForStableInstall()
        {
            try
            {
                return new StableStorage();
            }
            catch
            {
                return null;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!noVersionOverlay)
            {
                LoadComponentAsync(new VersionManager { Depth = int.MinValue }, v =>
                {
                    Add(v);
                    v.State = Visibility.Visible;
                });

                if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                    Add(new SquirrelUpdateManager());
                else
                    Add(new SimpleUpdateManager());
            }
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            var desktopWindow = host.Window as DesktopGameWindow;
            if (desktopWindow != null)
            {
                desktopWindow.CursorState |= CursorState.Hidden;

                desktopWindow.SetIconFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico"));
                desktopWindow.Title = Name;

                desktopWindow.FileDrop += fileDrop;
            }
        }

        private void fileDrop(object sender, FileDropEventArgs e)
        {
            var filePaths = new[] { e.FileName };

            var firstExtension = Path.GetExtension(filePaths.First());

            if (filePaths.Any(f => Path.GetExtension(f) != firstExtension)) return;

            Task.Factory.StartNew(() => Import(filePaths), TaskCreationOptions.LongRunning);
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
                        stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(String.Empty).ToString().Split('"')[1].Replace("osu!.exe", "");

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

            public StableStorage()
                : base(string.Empty, null)
            {
            }
        }
    }
}
