//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Beatmaps.Timing
{
    [Flags]
    public enum KiaiType
    {
        None = 0,
        Kiai = 1,
        OmitFirstBarLine = 8
    };
}