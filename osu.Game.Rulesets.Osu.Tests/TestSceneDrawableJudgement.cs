// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneDrawableJudgement : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableJudgement),
            typeof(DrawableOsuJudgement)
        };

        public TestSceneDrawableJudgement()
        {
            foreach (HitResult result in Enum.GetValues(typeof(HitResult)))
            {
                JudgementResult judgement = new JudgementResult(null)
                {
                    Type = result,
                };

                AddStep("Show " + result.GetDescription(), () => SetContents(() => new DrawableOsuJudgement(judgement, null)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }));
            }
        }
    }
}
