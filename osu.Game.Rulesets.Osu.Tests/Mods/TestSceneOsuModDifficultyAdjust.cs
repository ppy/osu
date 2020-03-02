// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModDifficultyAdjust : ModSandboxTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Append(typeof(OsuModDifficultyAdjust)).ToList();

        public TestSceneOsuModDifficultyAdjust()
            : base(new OsuRuleset())
        {
        }

        protected override ModTestCaseData[] CreateTestCases() => new[]
        {
            new ModTestCaseData("no adjustment", new OsuModDifficultyAdjust())
            {
                Autoplay = true,
                PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
            },
            new ModTestCaseData("cs = 10", new OsuModDifficultyAdjust { CircleSize = { Value = 10 } })
            {
                Autoplay = true,
                PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
            },
            new ModTestCaseData("ar = 10", new OsuModDifficultyAdjust { ApproachRate = { Value = 10 } })
            {
                Autoplay = true,
                PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
            },
        };

        protected override TestPlayer CreateReplayPlayer(Score score) => new ScoreAccessibleTestPlayer(score);

        private class ScoreAccessibleTestPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreAccessibleTestPlayer(Score score)
                : base(score)
            {
            }
        }
    }
}
