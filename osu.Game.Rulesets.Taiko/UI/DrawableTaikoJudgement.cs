// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public class DrawableTaikoJudgement : DrawableJudgement
    {
        protected override void ApplyHitAnimations()
        {
            this.MoveToY(-100, 500);
            base.ApplyHitAnimations();
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) => new TaikoJudgementPiece(result);

        private class TaikoJudgementPiece : DefaultJudgementPiece
        {
            public TaikoJudgementPiece(HitResult result)
                : base(result)
            {
            }

            public override void PlayAnimation()
            {
                if (Result != HitResult.Miss)
                {
                    JudgementText.ScaleTo(0.9f);
                    JudgementText.ScaleTo(1, 500, Easing.OutElastic);
                }

                base.PlayAnimation();
            }
        }
    }
}
