// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Win32;
using osu.Desktop.Overlays;
using osu.Framework.Platform;
using osu.Game;
using osu.Desktop.Updater;
using osu.Framework;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Updater;
using osu.Desktop.Windows;
using osu.Game.IO;

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

        public override StableStorage GetStorageForStableInstall()
        {
            try
            {
                if (Host is DesktopGameHost desktopHost)
                {
                    string stablePath = getStableInstallPath();
                    if (!string.IsNullOrEmpty(stablePath))
                        return new StableStorage(stablePath, desktopHost);
                }
            }
            catch (Exception)
            {
                Logger.Log("Could not find a stable install", LoggingTarget.Runtime, LogLevel.Important);
            }

            return null;
        }

        private string getStableInstallPath()
        {
            static bool checkExists(string p) => Directory.Exists(Path.Combine(p, "Songs"));

            string stableInstallPath;

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    stableInstallPath = getStableInstallPathFromRegistry();

                    if (!string.IsNullOrEmpty(stableInstallPath) && checkExists(stableInstallPath))
                        return stableInstallPath;
                }
                catch { }
            }

            stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");
            if (checkExists(stableInstallPath))
                return stableInstallPath;

            stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");
            if (checkExists(stableInstallPath))
                return stableInstallPath;

            return null;
        }

        [SupportedOSPlatform("windows")]
        private string getStableInstallPathFromRegistry()
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                return key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty)?.ToString()?.Split('"')[1].Replace("osu!.exe", "");
        }

        protected override UpdateManager CreateUpdateManager()
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    return new SquirrelUpdateManager();

                default:
                    return new SimpleUpdateManager();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!noVersionOverlay)
                LoadComponentAsync(versionManager = new VersionManager { Depth = int.MinValue }, Add);

            LoadComponentAsync(new DiscordRichPresence(), Add);

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                LoadComponentAsync(new GameplayWinKeyBlocker(), Add);
        }

        protected override void ScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            base.ScreenChanged(lastScreen, newScreen);

            switch (newScreen)
            {
                case IntroScreen _:
                case MainMenu _:
                    versionManager?.Show();
                    break;

                default:
                    versionManager?.Hide();
                    break;
            }
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico");

            var desktopWindow = (SDL2DesktopWindow)host.Window;

            desktopWindow.CursorState |= CursorState.Hidden;
            desktopWindow.SetIconFromStream(iconStream);
            desktopWindow.Title = Name;
            desktopWindow.DragDrop += f => fileDrop(new[] { f });
        }

        private void fileDrop(string[] filePaths)
        {
            var firstExtension = Path.GetExtension(filePaths.First());

            if (filePaths.Any(f => Path.GetExtension(f) != firstExtension)) return;

            Task.Factory.StartNew(() => Import(filePaths), TaskCreationOptions.LongRunning);
        }
    }
}
