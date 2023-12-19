// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public partial class DrawableTaikoJudgement : DrawableJudgement, IVisualiseSecondHit
    {
        public DrawableTaikoJudgement()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) => new TaikoDefaultJudgementPiece(result);

        // Not actually used in execution. We're implementing the interface for AnimateSecondHit().
        public void Animate(DrawableHitObject drawableHitObject)
        {
            if (JudgementBody.Drawable is IAnimatableTaikoJudgement taikoJudgement)
                taikoJudgement.Animate(drawableHitObject);
            else if (JudgementBody.Drawable is IAnimatableJudgement animatableJudgement)
                animatableJudgement.Animate();
        }

        public override void Apply(JudgementResult result, DrawableHitObject? judgedObject)
        {
            base.Apply(result, judgedObject);
            if (JudgementBody.Drawable is IVisualiseSecondHit visualiseSecondHit)
                visualiseSecondHit.VisualiseSecondHit(null);
        }

        public void VisualiseSecondHit(JudgementResult? judgementResult)
        {
            if (JudgementBody.Drawable is IVisualiseSecondHit visualiseSecondHit)
                visualiseSecondHit.VisualiseSecondHit(judgementResult);
        }
    }
}
