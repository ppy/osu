// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// Common interface for all hit explosion bodies.
    /// </summary>
    public interface IHitExplosion
    {
        /// <summary>
        /// Begins animating this <see cref="IHitExplosion"/>.
        /// </summary>
        void Animate();
    }
}
