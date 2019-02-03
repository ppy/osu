// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting position.
    /// </summary>
    public interface IHasPosition : IHasXPosition, IHasYPosition
    {
        /// <summary>
        /// The starting position of the HitObject.
        /// </summary>
        Vector2 Position { get; }
    }
}
