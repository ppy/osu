// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseBananaShower : Game.Tests.Visual.TestCasePlayer
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BananaShower),
            typeof(DrawableBananaShower),

            typeof(CatchRuleset),
            typeof(CatchRulesetContainer),
        };

        public TestCaseBananaShower()
            : base(new CatchRuleset())
        {
        }

        protected override IBeatmap CreateBeatmap(Ruleset ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty { CircleSize = 6 },
                    Ruleset = ruleset.RulesetInfo
                }
            };

            beatmap.HitObjects.Add(new BananaShower { StartTime = 200, Duration = 5000, NewCombo = true });

            return beatmap;
        }
    }
}
