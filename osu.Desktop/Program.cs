// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
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
        [STAThread]
        public static int Main(string[] args)
        {
            // Back up the cwd before DesktopGameHost changes it
            var cwd = Environment.CurrentDirectory;
            bool useOsuTK = args.Contains("--tk");

            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true, useOsuTK: useOsuTK))
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

                switch (args.FirstOrDefault() ?? string.Empty)
                {
                    default:
                        host.Run(new OsuGameDesktop(args));
                        break;

                    case "--tournament":
                        host.Run(new TournamentGame());
                        break;
                }

                return 0;
            }
        }

        private static int allowableExceptions = DebugUtils.IsDebugBuild ? 0 : 1;

        /// <summary>
        /// Allow a maximum of one unhandled exception, per second of execution.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
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
