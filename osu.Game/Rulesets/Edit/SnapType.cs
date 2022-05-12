// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Edit
{
    [Flags]
    public enum SnapType
    {
        NearbyObjects = 0,
        Grids = 1,
        All = NearbyObjects | Grids,
    }
}
