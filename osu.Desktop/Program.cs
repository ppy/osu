// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Beatmaps;
using osu.Game.IPC;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            // Back up the cwd before DesktopGameHost changes it
            var cwd = Environment.CurrentDirectory;

            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                if (!host.IsPrimaryInstance)
                {
                    var importer = new BeatmapIPCChannel(host);
                    // Restore the cwd so relative paths given at the command line work correctly
                    Directory.SetCurrentDirectory(cwd);
                    foreach (var file in args)
                    {
                        Console.WriteLine(@"Importing {0}", file);
                        if (!importer.ImportAsync(Path.GetFullPath(file)).Wait(3000))
                            throw new TimeoutException(@"IPC took too long to send");
                    }
                }
                else
                {
                    parsingCommends(host, args);
                }
                return 0;
            }
        }

        private static void parsingCommends(DesktopGameHost host, string[] args)
        {

            switch (args.FirstOrDefault() ?? string.Empty)
            {
                case "--tests":
                host.Run(new OsuTestBrowser());
                break;
                case "--import-beatmap":
                case "-ib":
                Console.WriteLine();
                if(args.Length == 1)
                {
                    Console.WriteLine(args[0] + " Option require .osz files.");
                    break;
                }
                string[] files = Array.FindAll(args, f => Path.GetExtension(f) == @".osz");
                DatabaseContextFactory contextFactory = new DatabaseContextFactory(host);
                RulesetStore rulesetStore = new RulesetStore(contextFactory.GetContext);
                BeatmapManager beatmapManager = new BeatmapManager(host.Storage, contextFactory.GetContext, rulesetStore, null, host){ IsConsoleLogger = true};
                beatmapManager.Import(files);

                break;
                default:
                host.Run(new OsuGameDesktop(args));
                break;
            }
        }
    }
}
