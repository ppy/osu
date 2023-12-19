// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A skinnable judgement element which supports playing an animation from the current point in time.
    /// Also supports switching to a "strong" judgement type.
    /// </summary>
    public interface IAnimatableTaikoJudgement
    {
        /// <summary>
        /// Shows the hit explosion for the supplied <paramref name="drawableHitObject"/>.
        /// </summary>
        void Animate(DrawableHitObject drawableHitObject);

        /// <summary>
        /// Transforms the hit explosion to visualise a secondary hit.
        /// </summary>
        void AnimateSecondHit();
    }
}
