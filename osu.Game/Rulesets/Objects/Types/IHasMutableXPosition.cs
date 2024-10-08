// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting X-position that can be mutated.
    /// </summary>
    public interface IHasMutableXPosition : IHasXPosition
    {
        /// <summary>
        /// The starting X-position of this HitObject.
        /// </summary>
        new float X { get; set; }
    }
}
