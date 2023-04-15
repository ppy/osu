// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using M.DBus;
using Microsoft.Win32;
using Mvis.Plugin.BottomBar;
using Mvis.Plugin.CloudMusicSupport;
using Mvis.Plugin.CollectionSupport;
using Mvis.Plugin.StoryboardSupport;
using Mvis.Plugin.Yasp;
using osu.Desktop.DBus;
using osu.Desktop.Security;
using osu.Framework.Platform;
using osu.Game;
using osu.Framework;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Updater;
using osu.Desktop.Windows;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform.MacOS;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.IO;
using osu.Game.Screens.Menu;
using osu.Game.IPC;
using osu.Game.Utils;
using SDL2;
using osu.Framework.Platform.Windows;

namespace osu.Desktop
{
    internal partial class OsuGameDesktop : OsuGame
    {
        private OsuSchemeLinkIPCChannel? osuSchemeLinkIPCChannel;
        private ArchiveImportIPCChannel? archiveImportIPCChannel;

        private DBusManagerContainer? dBusManagerContainer;

        public OsuGameDesktop(string[]? args = null, string? hashOverride = null)
            : base(args)
        {
            if (!string.IsNullOrEmpty(hashOverride))
            {
                HashOverriden = true;
                VersionHash = hashOverride;
            }

            //workaround: 不预载会让PluginStore在AppDomain里扫不到插件...
            preloadPluginProviders();
        }

        private void preloadPluginProviders()
        {
            new YaspProvider();
            new LyricPluginProvider();
            new BottomBarProvider();
            new CollectionHelperProvider();
            new StoryboardPluginProvider();
        }

        public override StableStorage? GetStorageForStableInstall()
        {
            try
            {
                if (Host is DesktopGameHost desktopHost)
                {
                    string? stablePath = getStableInstallPath();
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

        private string? getStableInstallPath()
        {
            static bool checkExists(string p) => Directory.Exists(Path.Combine(p, "Songs")) || File.Exists(Path.Combine(p, "osu!.cfg"));

            string? stableInstallPath;

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    stableInstallPath = getStableInstallPathFromRegistry();

                    if (!string.IsNullOrEmpty(stableInstallPath) && checkExists(stableInstallPath))
                        return stableInstallPath;
                }
                catch
                {
                }
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
        private string? getStableInstallPathFromRegistry()
        {
            using (RegistryKey? key = Registry.ClassesRoot.OpenSubKey("osu"))
                return key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty)?.ToString()?.Split('"')[1].Replace("osu!.exe", "");
        }

        protected override UpdateManager CreateUpdateManager()
        {
            string? packageManaged = Environment.GetEnvironmentVariable("OSU_EXTERNAL_UPDATE_PROVIDER");

            if (!string.IsNullOrEmpty(packageManaged))
                return new NoActionUpdateManager();

            switch (RuntimeInfo.OS)
            {
                //case RuntimeInfo.Platform.Windows:
                //    Debug.Assert(OperatingSystem.IsWindows());

                //    return new SquirrelUpdateManager();

                default:
                    return new SimpleUpdateManager();
            }
        }

        private DependencyContainer dependencies = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new DiscordRichPresence(), Add);

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
                LoadComponentAsync(new GameplayWinKeyBlocker(), Add);

            LoadComponentAsync(new DBusManagerContainer(
                true,
                MConfig.GetBindable<bool>(MSetting.DBusIntegration)), d =>
            {
                dBusManagerContainer = d;
                dependencies.CacheAs<IDBusManagerContainer<IMDBusObject>>(d);
                d.NotificationAction += n => Notifications.Post(n);
                Add(d);
            });

            LoadComponentAsync(new ElevatedPrivilegesChecker(), Add);

            osuSchemeLinkIPCChannel = new OsuSchemeLinkIPCChannel(Host, this);

            archiveImportIPCChannel = new ArchiveImportIPCChannel(Host, this);

            MConfig.BindWith(MSetting.AllowWindowFadeEffect, allowWindowFade);

            windowOpacity.Value = allowWindowFade.Value ? 0 : 1;

            windowOpacity.BindValueChanged(v => SetWindowOpacity(v.NewValue), true);
        }

        private readonly BindableBool allowWindowFade = new BindableBool();

        protected override void ScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            base.ScreenChanged(lastScreen, newScreen);

            switch (newScreen)
            {
                case IntroScreen introScreen:
                    if (!(lastScreen is Disclaimer) && allowWindowFade.Value)
                        TransformWindowOpacity(0, introScreen.FadeOutTime - 1);

                    break;

                case Disclaimer _:
                    if (!(lastScreen is IntroScreen) && allowWindowFade.Value)
                        TransformWindowOpacity(1, 300);
                    break;
            }
        }

        private readonly BindableFloat windowOpacity = new BindableFloat();

        public override void ForceWindowFadeIn() => TransformWindowOpacity(1, 300);

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            var desktopWindow = (SDL2DesktopWindow)host.Window;

            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico");
            if (iconStream != null)
                desktopWindow.SetIconFromStream(iconStream);

            desktopWindow.CursorState |= CursorState.Hidden;
            desktopWindow.Title = Name;
            desktopWindow.DragDrop += f =>
            {
                // on macOS, URL associations are handled via SDL_DROPFILE events.
                if (f.StartsWith(OSU_PROTOCOL, StringComparison.Ordinal))
                {
                    HandleLink(f);
                    return;
                }

                fileDrop(new[] { f });
            };

            //mfosu: Find SDLWindowHandle
            if (windowHandle == null)
            {
                const BindingFlags binding_flag = BindingFlags.Instance | BindingFlags.NonPublic;

                try
                {
                    // WindowsWindow/MacOSWindow -> SDL2DesktopWindow -> SDL2Window
                    FieldInfo[] fields = desktopWindow switch
                    {
                        WindowsWindow windowsWindow => windowsWindow.GetType().BaseType!.BaseType!.GetFields(binding_flag),
                        MacOSWindow macOSWindow => macOSWindow.GetType().BaseType!.BaseType!.GetFields(binding_flag),
                        _ => desktopWindow.GetType().BaseType!.GetFields(binding_flag)
                    };

                    foreach (var fieldInfo in fields)
                    {
                        if (!fieldInfo.Name.Contains("SDLWindowHandle")) continue;

                        object? val = fieldInfo.GetValue(desktopWindow);

                        windowHandle = (val is IntPtr ptr) ? ptr : IntPtr.Zero;

                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "寻找WindowHandle时出现问题");
                }

                if (windowHandle == null || windowHandle == IntPtr.Zero)
                    Logger.Log("未能找到WindowHandle，某些功能将被限制", level: LogLevel.Important);
            }
        }

        protected override BatteryInfo CreateBatteryInfo() => new SDL2BatteryInfo();

        private readonly List<string> importableFiles = new List<string>();
        private ScheduledDelegate? importSchedule;

        private void fileDrop(string[] filePaths)
        {
            lock (importableFiles)
            {
                importableFiles.AddRange(filePaths);

                Logger.Log($"Adding {filePaths.Length} files for import");

                // File drag drop operations can potentially trigger hundreds or thousands of these calls on some platforms.
                // In order to avoid spawning multiple import tasks for a single drop operation, debounce a touch.
                importSchedule?.Cancel();
                importSchedule = Scheduler.AddDelayed(handlePendingImports, 100);
            }
        }

        private void handlePendingImports()
        {
            lock (importableFiles)
            {
                Logger.Log($"Handling batch import of {importableFiles.Count} files");

                string[] paths = importableFiles.ToArray();
                importableFiles.Clear();

                Task.Factory.StartNew(() => Import(paths), TaskCreationOptions.LongRunning);
            }
        }

        public void TransformWindowOpacity(float final, double duration = 0, Easing easing = Easing.None) =>
            this.TransformBindableTo(windowOpacity, final, duration, easing);

        private IntPtr? windowHandle;

        /// <summary>
        /// Sets window opacity
        /// mfosu interface
        /// </summary>
        /// <param name="value"></param>
        public void SetWindowOpacity(float value)
        {
            if (windowHandle != null && windowHandle != IntPtr.Zero)
                SDL.SDL_SetWindowOpacity((IntPtr)windowHandle, value);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            osuSchemeLinkIPCChannel?.Dispose();
            archiveImportIPCChannel?.Dispose();
        }

        private class SDL2BatteryInfo : BatteryInfo
        {
            public override double? ChargeLevel
            {
                get
                {
                    SDL.SDL_GetPowerInfo(out _, out int percentage);

                    if (percentage == -1)
                        return null;

                    return percentage / 100.0;
                }
            }

            public override bool OnBattery => SDL.SDL_GetPowerInfo(out _, out _) == SDL.SDL_PowerState.SDL_POWERSTATE_ON_BATTERY;
        }
    }
}
