// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Objects.Types
{
    /// <summary>
    /// A type of hit object which lies in one of a number of predetermined columns.
    /// </summary>
    public interface IHasColumn
    {
        /// <summary>
        /// The column which the hit object lies in.
        /// </summary>
        int Column { get; }
    }
}
