// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
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
            foreach (HitResult result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Skip(1))
                AddStep("Show " + result.GetDescription(), () => SetContents(() =>
                    new DrawableOsuJudgement(new JudgementResult(new HitObject(), new Judgement()) { Type = result }, null)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }));
        }
    }
}
