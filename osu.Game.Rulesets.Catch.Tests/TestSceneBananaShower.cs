// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneBananaShower : PlayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BananaShower),
            typeof(DrawableBananaShower),

            typeof(CatchRuleset),
            typeof(DrawableCatchRuleset),
        };

        public TestSceneBananaShower()
            : base(new CatchRuleset())
        {
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty { CircleSize = 6 },
                    Ruleset = ruleset
                }
            };

            beatmap.HitObjects.Add(new BananaShower { StartTime = 200, Duration = 5000, NewCombo = true });

            return beatmap;
        }
    }
}
