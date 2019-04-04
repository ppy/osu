// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseHyperDash : PlayerTestCase
    {
        public TestCaseHyperDash()
            : base(new CatchRuleset())
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddAssert("First note is hyperdash", () => Beatmap.Value.Beatmap.HitObjects[0] is Fruit f && f.HyperDash);
        }

        protected override IBeatmap CreateBeatmap(Ruleset ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo =
                {
                    Ruleset = ruleset.RulesetInfo,
                    BaseDifficulty = new BeatmapDifficulty { CircleSize = 3.6f }
                }
            };

            // Should produce a hyper-dash
            beatmap.HitObjects.Add(new Fruit { StartTime = 816, X = 308 / 512f, NewCombo = true });
            beatmap.HitObjects.Add(new Fruit { StartTime = 1008, X = 56 / 512f, });

            for (int i = 0; i < 512; i++)
                if (i % 5 < 3)
                    beatmap.HitObjects.Add(new Fruit { X = i % 10 < 5 ? 0.02f : 0.98f, StartTime = 2000 + i * 100, NewCombo = i % 8 == 0 });

            return beatmap;
        }
    }
}
