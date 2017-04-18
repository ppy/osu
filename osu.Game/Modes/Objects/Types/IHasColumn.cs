// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that lies in a column space.
    /// </summary>
    public interface IHasColumn
    {
        /// <summary>
        /// The column which this HitObject lies in.
        /// </summary>
        int Column { get; }
    }
}
