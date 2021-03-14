// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Interface for hit explosions shown on the playfield's hit target in taiko.
    /// </summary>
    public interface IHitExplosion
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
