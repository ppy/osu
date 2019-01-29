// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseHyperDash : Game.Tests.Visual.TestCasePlayer
    {
        public TestCaseHyperDash()
            : base(new CatchRuleset())
        {
        }

        protected override IBeatmap CreateBeatmap(Ruleset ruleset)
        {
            var beatmap = new Beatmap { BeatmapInfo = { Ruleset = ruleset.RulesetInfo } };

            for (int i = 0; i < 512; i++)
                if (i % 5 < 3)
                    beatmap.HitObjects.Add(new Fruit { X = i % 10 < 5 ? 0.02f : 0.98f, StartTime = i * 100, NewCombo = i % 8 == 0 });

            return beatmap;
        }
    }
}
