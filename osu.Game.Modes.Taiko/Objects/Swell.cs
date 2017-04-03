﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Taiko.Objects
{
    public class Swell : TaikoHitObject, IHasEndTime
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits = 10;
    }
}