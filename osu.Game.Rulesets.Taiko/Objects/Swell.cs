// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Swell : TaikoHitObject, IHasEndTime
    {
        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits = 10;
    }
}
