// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Desktop.Beatmaps.IO;
using osu.Framework.Desktop;
using osu.Framework.Desktop.Platform;
using osu.Game.IPC;
using osu.Game.Modes;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;

namespace osu.Desktop
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            LegacyFilesystemReader.Register();

            // Back up the cwd before DesktopGameHost changes it
            var cwd = Environment.CurrentDirectory;

            using (DesktopGameHost host = Host.GetSuitableHost(@"osu", true))
            {
                if (!host.IsPrimaryInstance)
                {
                    var importer = new BeatmapImporter(host);
                    // Restore the cwd so relative paths given at the command line work correctly
                    Directory.SetCurrentDirectory(cwd);
                    foreach (var file in args)
                    {
                        Console.WriteLine(@"Importing {0}", file);
                        if (!importer.Import(Path.GetFullPath(file)).Wait(3000))
                            throw new TimeoutException(@"IPC took too long to send");
                    }
                }
                else
                {
                    Ruleset.Register(new OsuRuleset());
                    Ruleset.Register(new TaikoRuleset());
                    Ruleset.Register(new ManiaRuleset());
                    Ruleset.Register(new CatchRuleset());

                    host.Add(new OsuGameDesktop(args));
                    host.Run();
                }
                return 0;
            }
        }
    }
}
