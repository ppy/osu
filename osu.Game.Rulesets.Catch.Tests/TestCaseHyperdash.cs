// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseHyperdash : Game.Tests.Visual.TestCasePlayer
    {
        public TestCaseHyperdash()
            : base(new CatchRuleset())
        {
        }

        protected override Beatmap CreateBeatmap(Ruleset ruleset)
        {
            var beatmap = new Beatmap { BeatmapInfo = { Ruleset = ruleset.RulesetInfo } };


            for (int i = 0; i < 512; i++)
                if (i % 5 < 3)
                    beatmap.HitObjects.Add(new Fruit { X = i % 10 < 5 ? 0.02f : 0.98f, StartTime = i * 100, NewCombo = i % 8 == 0 });

            return beatmap;
        }
    }
}
