// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// An interface providing properties of catch hit explosions.
    /// </summary>
    public interface ICatchHitExplosion
    {
        /// <summary>
        /// The color of the object the hit explosion is attached to.
        /// </summary>
        public Color4 ObjectColour
        {
            get;
            set;
        }

        /// <summary>
        /// The hit object the hit explosion is attached to.
        /// </summary>
        public CatchHitObject HitObject
        {
            get;
            set;
        }
    }
}
