// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneDrawableJudgement : ManiaSkinnableTestScene
    {
        public TestSceneDrawableJudgement()
        {
            var hitWindows = new ManiaHitWindows();

            foreach (HitResult result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Skip(1))
            {
                if (hitWindows.IsHitResultAllowed(result))
                {
                    AddStep("Show " + result.GetDescription(), () => SetContents(() =>
                        new DrawableManiaJudgement(new JudgementResult(new HitObject { StartTime = Time.Current }, new Judgement())
                        {
                            Type = result
                        }, null)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }));
                }
            }
        }
    }
}
