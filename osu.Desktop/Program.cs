// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Runtime.Versioning;
using osu.Desktop.LegacyIpc;
using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IPC;
using osu.Game.Tournament;
using Squirrel;

namespace osu.Desktop
{
    public static class Program
    {
        private const string base_game_name = @"osu";

        private static LegacyTcpIpcProvider legacyIpc;

        [STAThread]
        public static void Main(string[] args)
        {
            // run Squirrel first, as the app may exit after these run
            if (OperatingSystem.IsWindows())
                setupSquirrel();

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

            using (DesktopGameHost host = Host.GetSuitableDesktopHost(gameName, new HostOptions { BindIPC = true }))
            {
                if (!host.IsPrimaryInstance)
                {
                    if (args.Length > 0 && args[0].Contains('.')) // easy way to check for a file import in args
                    {
                        var importer = new ArchiveImportIPCChannel(host);

                        foreach (string file in args)
                        {
                            Console.WriteLine(@"Importing {0}", file);
                            if (!importer.ImportAsync(Path.GetFullPath(file, cwd)).Wait(3000))
                                throw new TimeoutException(@"IPC took too long to send");
                        }

                        return;
                    }

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

        [SupportedOSPlatform("windows")]
        private static void setupSquirrel()
        {
            SquirrelAwareApp.HandleEvents(onInitialInstall: (version, tools) =>
            {
                tools.CreateShortcutForThisExe();
                tools.CreateUninstallerRegistryEntry();
            }, onAppUninstall: (version, tools) =>
            {
                tools.RemoveShortcutForThisExe();
                tools.RemoveUninstallerRegistryEntry();
            }, onEveryRun: (version, tools, firstRun) =>
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
