// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Desktop;
using osu.Framework.Platform;
using osu.Game.Modes;
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
                Ruleset.LoadRulesetsFrom(Environment.CurrentDirectory);

                if (benchmark)
                    host.Run(new Benchmark());
                else
                    host.Run(new VisualTestGame());
            }
        }
    }
}
