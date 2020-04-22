// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneSwellJudgements : PlayerTestScene
    {
        public TestSceneSwellJudgements()
            : base(new TaikoRuleset())
        {
        }

        [Test]
        public void TestZeroTickTimeOffsets()
        {
            AddUntilStep("gameplay finished", () => Player.ScoreProcessor.HasCompleted.Value);
            AddAssert("all tick offsets are 0", () => Player.Results.Where(r => r.HitObject is SwellTick).All(r => r.TimeOffset == 0));
        }

        protected override bool Autoplay => true;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap<TaikoHitObject>
            {
                BeatmapInfo = { Ruleset = new TaikoRuleset().RulesetInfo },
                HitObjects =
                {
                    new Swell
                    {
                        StartTime = 1000,
                        Duration = 1000,
                    }
                }
            };

            return beatmap;
        }
    }
}
