//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            using (BasicGameHost host = Host.GetSuitableHost(@"osu-visual-tests"))
            {
                Ruleset.Register(new OsuRuleset());
                Ruleset.Register(new TaikoRuleset());
                Ruleset.Register(new ManiaRuleset());
                Ruleset.Register(new CatchRuleset());

                host.Add(new VisualTestGame());
                host.Run();
            }
        }
    }
}
