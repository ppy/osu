// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Desktop;
using osu.Framework.Desktop.Platform;
using osu.Framework.Platform;
using osu.Game.Modes;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;

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
                Ruleset.Register(new OsuRuleset());
                Ruleset.Register(new TaikoRuleset());
                Ruleset.Register(new ManiaRuleset());
                Ruleset.Register(new CatchRuleset());

                if (benchmark)
                    host.Add(new Benchmark());
                else
                    host.Add(new VisualTestGame());
                host.Run();
            }
        }
    }
}
