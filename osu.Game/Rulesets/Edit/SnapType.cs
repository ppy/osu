// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Edit
{
    [Flags]
    public enum SnapType
    {
        None = 0,

        /// <summary>
        /// Snapping to visible nearby objects.
        /// </summary>
        NearbyObjects = 1 << 0,

        /// <summary>
        /// Grids which are global to the playfield.
        /// </summary>
        GlobalGrids = 1 << 1,

        /// <summary>
        /// Grids which are relative to other nearby hit objects.
        /// </summary>
        RelativeGrids = 1 << 2,

        AllGrids = RelativeGrids | GlobalGrids,

        All = NearbyObjects | GlobalGrids | RelativeGrids,
    }
}
