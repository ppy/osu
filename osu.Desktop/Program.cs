// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Runtime.Versioning;
using osu.Desktop.LegacyIpc;
using osu.Desktop.Windows;
using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.IPC;
using osu.Game.Tournament;
using SDL2;
using Squirrel;

namespace osu.Desktop
{
    public static class Program
    {
#if DEBUG
        private const string base_game_name = @"osu-development";
#else
        private const string base_game_name = @"osu";
#endif

        private static LegacyTcpIpcProvider? legacyIpc;

        [STAThread]
        public static void Main(string[] args)
        {
            /*
             * WARNING: DO NOT PLACE **ANY** CODE ABOVE THE FOLLOWING BLOCK!
             *
             * Logic handling Squirrel MUST run before EVERYTHING if you do not want to break it.
             * To be more precise: Squirrel is internally using a rather... crude method to determine whether it is running under NUnit,
             * namely by checking loaded assemblies:
             * https://github.com/clowd/Clowd.Squirrel/blob/24427217482deeeb9f2cacac555525edfc7bd9ac/src/Squirrel/SimpleSplat/PlatformModeDetector.cs#L17-L32
             *
             * If it finds ANY assembly from the ones listed above - REGARDLESS of the reason why it is loaded -
             * the app will then do completely broken things like:
             * - not creating system shortcuts (as the logic is if'd out if "running tests")
             * - not exiting after the install / first-update / uninstall hooks are ran (as the `Environment.Exit()` calls are if'd out if "running tests")
             */
            if (OperatingSystem.IsWindows())
            {
                var windowsVersion = Environment.OSVersion.Version;

                // While .NET 6 still supports Windows 7 and above, we are limited by realm currently, as they choose to only support 8.1 and higher.
                // See https://www.mongodb.com/docs/realm/sdk/dotnet/#supported-platforms
                if (windowsVersion.Major < 6 || (windowsVersion.Major == 6 && windowsVersion.Minor <= 2))
                {
                    // If users running in compatibility mode becomes more of a common thing, we may want to provide better guidance or even consider
                    // disabling it ourselves.
                    // We could also better detect compatibility mode if required:
                    // https://stackoverflow.com/questions/10744651/how-i-can-detect-if-my-application-is-running-under-compatibility-mode#comment58183249_10744730
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
                        "Your operating system is too old to run osu!",
                        "This version of osu! requires at least Windows 8.1 to run.\n"
                        + "Please upgrade your operating system or consider using an older version of osu!.\n\n"
                        + "If you are running a newer version of windows, please check you don't have \"Compatibility mode\" turned on for osu!", IntPtr.Zero);
                    return;
                }

                setupSquirrel();
            }

            // NVIDIA profiles are based on the executable name of a process.
            // Lazer and stable share the same executable name.
            // Stable sets this setting to "Off", which may not be what we want, so let's force it back to the default "Auto" on startup.
            NVAPI.ThreadedOptimisations = NvThreadControlSetting.OGL_THREAD_CONTROL_DEFAULT;

            // Back up the cwd before DesktopGameHost changes it
            string cwd = Environment.CurrentDirectory;

            string gameName = base_game_name;
            bool tournamentClient = false;

            foreach (string arg in args)
            {
                string[] split = arg.Split('=');

                string key = split[0];
                string val = split.Length > 1 ? split[1] : string.Empty;

                switch (key)
                {
                    case "--tournament":
                        tournamentClient = true;
                        break;

                    case "--debug-client-id":
                        if (!DebugUtils.IsDebugBuild)
                            throw new InvalidOperationException("Cannot use this argument in a non-debug build.");

                        if (!int.TryParse(val, out int clientID))
                            throw new ArgumentException("Provided client ID must be an integer.");

                        gameName = $"{base_game_name}-{clientID}";
                        break;
                }
            }

            using (DesktopGameHost host = Host.GetSuitableDesktopHost(gameName, new HostOptions { IPCPort = !tournamentClient ? OsuGame.IPC_PORT : null }))
            {
                if (!host.IsPrimaryInstance)
                {
                    if (trySendIPCMessage(host, cwd, args))
                        return;

                    // we want to allow multiple instances to be started when in debug.
                    if (!DebugUtils.IsDebugBuild)
                    {
                        Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                        return;
                    }
                }

                if (host.IsPrimaryInstance)
                {
                    try
                    {
                        Logger.Log("Starting legacy IPC provider...");
                        legacyIpc = new LegacyTcpIpcProvider();
                        legacyIpc.Bind();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to start legacy IPC provider");
                    }
                }

                if (tournamentClient)
                    host.Run(new TournamentGame());
                else
                    host.Run(new OsuGameDesktop(args));
            }
        }

        private static bool trySendIPCMessage(IIpcHost host, string cwd, string[] args)
        {
            if (args.Length == 1 && args[0].StartsWith(OsuGameBase.OSU_PROTOCOL, StringComparison.Ordinal))
            {
                var osuSchemeLinkHandler = new OsuSchemeLinkIPCChannel(host);
                if (!osuSchemeLinkHandler.HandleLinkAsync(args[0]).Wait(3000))
                    throw new IPCTimeoutException(osuSchemeLinkHandler.GetType());

                return true;
            }

            if (args.Length > 0 && args[0].Contains('.')) // easy way to check for a file import in args
            {
                var importer = new ArchiveImportIPCChannel(host);

                foreach (string file in args)
                {
                    Console.WriteLine(@"Importing {0}", file);
                    if (!importer.ImportAsync(Path.GetFullPath(file, cwd)).Wait(3000))
                        throw new IPCTimeoutException(importer.GetType());
                }

                return true;
            }

            return false;
        }

        [SupportedOSPlatform("windows")]
        private static void setupSquirrel()
        {
            SquirrelAwareApp.HandleEvents(onInitialInstall: (_, tools) =>
            {
                tools.CreateShortcutForThisExe();
                tools.CreateUninstallerRegistryEntry();
            }, onAppUpdate: (_, tools) =>
            {
                tools.CreateUninstallerRegistryEntry();
            }, onAppUninstall: (_, tools) =>
            {
                tools.RemoveShortcutForThisExe();
                tools.RemoveUninstallerRegistryEntry();
                WindowsAssociationManager.UninstallAssociations();
            }, onEveryRun: (_, _, _) =>
            {
                // While setting the `ProcessAppUserModelId` fixes duplicate icons/shortcuts on the taskbar, it currently
                // causes the right-click context menu to function incorrectly.
                //
                // This may turn out to be non-required after an alternative solution is implemented.
                // see https://github.com/clowd/Clowd.Squirrel/issues/24
                // tools.SetProcessAppUserModelId();
            });
        }
    }
}
