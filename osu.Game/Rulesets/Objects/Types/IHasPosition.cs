// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

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
