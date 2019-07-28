// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Replays.Legacy
{
    [Flags]
    public enum ReplayButtonState
    {
        None = 0,
        Left1 = 1,
        Right1 = 2,
        Left2 = 4,
        Right2 = 8,
        Smoke = 16
    }
}
