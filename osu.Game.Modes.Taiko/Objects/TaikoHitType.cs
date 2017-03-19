// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Modes.Taiko.Objects
{
    [Flags]
    public enum TaikoHitType
    {
        None = 0,
        CentreHit = 1 << 0,
        RimHit = 1 << 1,
        DrumRoll = 1 << 2,
        DrumRollTick = 1 << 3,
        Bash = 1 << 4,
        Finisher = 1 << 5,

        Hit = CentreHit | RimHit
    }
}