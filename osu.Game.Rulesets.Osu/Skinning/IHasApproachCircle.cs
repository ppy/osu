// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    /// <summary>
    /// A common interface between implementations which provide an approach circle.
    /// </summary>
    public interface IHasApproachCircle
    {
        /// <summary>
        /// The approach circle drawable.
        /// </summary>
        Drawable? ApproachCircle { get; }
    }
}
