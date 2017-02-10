// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Desktop.VisualTests;
using osu.Framework.Desktop.Platform;
using osu.Game.Modes;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;

namespace osu.Desktop.Tests
{
    [TestFixture]
    public class BenchmarkTest
    {
        [Test]
        public void TestBenchmark()
        {
            using (var host = new HeadlessGameHost())
            {
                Ruleset.Register(new OsuRuleset());
                Ruleset.Register(new TaikoRuleset());
                Ruleset.Register(new ManiaRuleset());
                Ruleset.Register(new CatchRuleset());

                host.Add(new Benchmark());
                host.Run();
            }
        }
    }
}
