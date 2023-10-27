// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public partial class DrawableTaikoJudgement : DrawableJudgement, IAnimatableHitExplosion
    {
        public DrawableTaikoJudgement()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) => new DefaultJudgementPiece(result);

        // Not actually used in execution. We're implementing the interface for AnimateSecondHit().
        public void Animate(DrawableHitObject drawableHitObject)
        {
            (JudgementBody.Drawable as IAnimatableHitExplosion)?.Animate(drawableHitObject);
        }

        public void AnimateSecondHit()
        {
            (JudgementBody.Drawable as IAnimatableHitExplosion)?.AnimateSecondHit();
        }
    }
}
