﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IPC;
using osu.Game.Tournament;

namespace osu.Desktop
{
    public static class Program
    {
        private const string base_game_name = @"osu";

        [STAThread]
        public static int Main(string[] args)
        {
            // Back up the cwd before DesktopGameHost changes it
            var cwd = Environment.CurrentDirectory;

            string gameName = base_game_name;
            bool tournamentClient = false;

            foreach (var arg in args)
            {
                var split = arg.Split('=');

                var key = split[0];
                var val = split.Length > 1 ? split[1] : string.Empty;

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

            using (DesktopGameHost host = Host.GetSuitableHost(gameName, true))
            {
                host.ExceptionThrown += handleException;

                if (!host.IsPrimaryInstance)
                {
                    if (args.Length > 0 && args[0].Contains('.')) // easy way to check for a file import in args
                    {
                        var importer = new ArchiveImportIPCChannel(host);

                        foreach (var file in args)
                        {
                            Console.WriteLine(@"Importing {0}", file);
                            if (!importer.ImportAsync(Path.GetFullPath(file, cwd)).Wait(3000))
                                throw new TimeoutException(@"IPC took too long to send");
                        }

                        return 0;
                    }

                    // we want to allow multiple instances to be started when in debug.
                    if (!DebugUtils.IsDebugBuild)
                        return 0;
                }

                if (tournamentClient)
                    host.Run(new TournamentGame());
                else
                    host.Run(new OsuGameDesktop(args));

                return 0;
            }
        }

        private static int allowableExceptions = DebugUtils.IsDebugBuild ? 0 : 1;

        /// <summary>
        /// Allow a maximum of one unhandled exception, per second of execution.
        /// </summary>
        /// <param name="arg"></param>
        private static bool handleException(Exception arg)
        {
            bool continueExecution = Interlocked.Decrement(ref allowableExceptions) >= 0;

            Logger.Log($"Unhandled exception has been {(continueExecution ? $"allowed with {allowableExceptions} more allowable exceptions" : "denied")} .");

            // restore the stock of allowable exceptions after a short delay.
            Task.Delay(1000).ContinueWith(_ => Interlocked.Increment(ref allowableExceptions));

            return continueExecution;
        }
    }
}
