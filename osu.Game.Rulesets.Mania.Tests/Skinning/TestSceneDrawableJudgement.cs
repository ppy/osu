// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public partial class TestSceneDrawableJudgement : ManiaSkinnableTestScene
    {
        public TestSceneDrawableJudgement()
        {
            var hitWindows = new ManiaHitWindows();

            foreach (HitResult result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Skip(1))
            {
                if (hitWindows.IsHitResultAllowed(result))
                {
                    AddStep("Show " + result.GetDescription(), () =>
                    {
                        SetContents(_ =>
                            new DrawableManiaJudgement(new Judgement(new HitObject { StartTime = Time.Current }, new JudgementInfo())
                            {
                                Type = result
                            }, null)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            });

                        // for test purposes, undo the Y adjustment related to the `ScorePosition` legacy positioning config value
                        // (see `LegacyManiaJudgementPiece.load()`).
                        // this prevents the judgements showing somewhere below or above the bounding box of the judgement.
                        foreach (var legacyPiece in this.ChildrenOfType<LegacyManiaJudgementPiece>())
                            legacyPiece.Y = 0;
                    });
                }
            }
        }
    }
}
