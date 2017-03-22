// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Osu.Objects
{
    public class Spinner : OsuHitObject, IHasEndTime
    {
        public double EndTime { get; set; }
        public double Duration => EndTime - StartTime;

        public override bool NewCombo => true;
    }
}
