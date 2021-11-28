// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Desktop.LegacyIpc;
using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IPC;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tournament;

namespace osu.Desktop
{
    public static class Program
    {
        private const string base_game_name = @"osu";

        private static LegacyTcpIpcProvider legacyIpcProvider;

        [STAThread]
        public static void Main(string[] args)
        {
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

            using (DesktopGameHost host = Host.GetSuitableHost(gameName, true))
            {
                host.ExceptionThrown += handleException;

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
                    var legacyIpc = new LegacyTcpIpcProvider();
                    legacyIpc.MessageReceived += onLegacyIpcMessageReceived;
                    legacyIpc.Bind();
                    legacyIpc.StartAsync();
                }

                if (tournamentClient)
                    host.Run(new TournamentGame());
                else
                    host.Run(new OsuGameDesktop(args));
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

        private static object onLegacyIpcMessageReceived(object message)
        {
            switch (message)
            {
                case LegacyIpcDifficultyCalculationRequest req:
                    try
                    {
                        Ruleset ruleset = req.RulesetId switch
                        {
                            0 => new OsuRuleset(),
                            1 => new TaikoRuleset(),
                            2 => new CatchRuleset(),
                            3 => new ManiaRuleset(),
                            _ => throw new ArgumentException("Invalid ruleset id")
                        };

                        Mod[] mods = ruleset.ConvertFromLegacyMods((LegacyMods)req.Mods).ToArray();
                        WorkingBeatmap beatmap = new FlatFileWorkingBeatmap(req.BeatmapFile, _ => ruleset);

                        return new LegacyIpcDifficultyCalculationResponse
                        {
                            StarRating = ruleset.CreateDifficultyCalculator(beatmap).Calculate(mods).StarRating
                        };
                    }
                    catch
                    {
                        return new LegacyIpcDifficultyCalculationResponse();
                    }
            }

            Console.WriteLine("Type not matched.");
            return null;
        }
    }
}
