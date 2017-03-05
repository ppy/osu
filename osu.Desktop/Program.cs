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
using osu.Game.Modes.Vitaru;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

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
                        if (!importer.ImportAsync(Path.GetFullPath(file)).Wait(3000))
                            throw new TimeoutException(@"IPC took too long to send");
                    }
                }
                else
                {
                    if (Debugger.IsAttached)
                        cwd = Directory.GetParent(Directory.GetParent(cwd).FullName).FullName;
                    loadRulesets(cwd);

                    host.Run(new OsuGameDesktop(args));
                }
                return 0;
            }
        }

        [DebuggerNonUserCode]
        private static void loadRulesets(string cwd)
        {
            foreach (string dir in Directory.EnumerateDirectories(cwd))
                loadRulesets(dir);
            
            foreach (string file in Directory.EnumerateFiles(cwd))
            {
                if (!file.EndsWith(".dll"))
                    continue;
                try
                {
                    var rulesets = Assembly.LoadFile(file).GetTypes().Where((Type t) => t.IsSubclassOf(typeof(Ruleset)));
                    foreach (Type rulesetType in rulesets)
                    {
                        Ruleset.Register(Activator.CreateInstance(rulesetType) as Ruleset);
                    }
                    
                }
                catch (Exception e) { }
            }
        }
    }
}
