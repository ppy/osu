// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
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
