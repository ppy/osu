// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting position.
    /// </summary>
    public interface IHasPosition
    {
        /// <summary>
        /// The starting position of the HitObject.
        /// </summary>
        Vector2 Position { get; }
    }
}
