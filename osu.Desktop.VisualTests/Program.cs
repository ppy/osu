// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Desktop;
using osu.Framework.Platform;
using osu.Game.Modes;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;

namespace osu.Desktop.VisualTests
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            bool benchmark = args.Length > 0 && args[0] == @"-benchmark";

            using (GameHost host = Host.GetSuitableHost(@"osu"))
            {
                var cwd = Environment.CurrentDirectory;
                if (Debugger.IsAttached)
                    cwd = Directory.GetParent(Directory.GetParent(cwd).FullName).FullName;
                loadRulesets(cwd);

                if (benchmark)
                    host.Run(new Benchmark());
                else
                    host.Run(new VisualTestGame());
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
