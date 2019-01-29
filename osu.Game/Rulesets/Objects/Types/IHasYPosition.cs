// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting Y-position.
    /// </summary>
    public interface IHasYPosition
    {
        /// <summary>
        /// The starting Y-position of this HitObject.
        /// </summary>
        float Y { get; }
    }
}
