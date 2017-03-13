// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Mania.Objects
{
    public abstract class ManiaBaseHit : HitObject, IHasEndTime
    {
        public int Column;

        public double EndTime { get; set; }

        public double Duration { get; set; }
    }
}
