// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using osu.Desktop.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game;
using osuTK.Input;
using Microsoft.Win32;
using osu.Desktop.Updater;
using osu.Framework;
using osu.Framework.Logging;
using osu.Framework.Platform.Windows;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;

namespace osu.Desktop
{
    internal class OsuGameDesktop : OsuGame
    {
        private readonly bool noVersionOverlay;
        private VersionManager versionManager;

        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
            noVersionOverlay = args?.Any(a => a == "--no-version-overlay") ?? false;
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!noVersionOverlay)
            {
                LoadComponentAsync(versionManager = new VersionManager { Depth = int.MinValue }, v =>
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

        protected override void ScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            base.ScreenChanged(lastScreen, newScreen);

            switch (newScreen)
            {
                case Intro _:
                case MainMenu _:
                    if (versionManager != null)
                        versionManager.State = Visibility.Visible;
                    break;
                default:
                    if (versionManager != null)
                        versionManager.State = Visibility.Hidden;
                    break;
            }
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            if (host.Window is DesktopGameWindow desktopWindow)
            {
                desktopWindow.CursorState |= CursorState.Hidden;

                desktopWindow.SetIconFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico"));
                desktopWindow.Title = Name;

                desktopWindow.FileDrop += fileDrop;
            }
        }

        private void fileDrop(object sender, FileDropEventArgs e)
        {
            var filePaths = e.FileNames;

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

            public StableStorage(DesktopGameHost host)
                : base(string.Empty, host)
            {
            }
        }
    }
}
