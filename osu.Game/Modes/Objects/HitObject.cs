﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;

namespace osu.Game.Modes.Objects
{
    /// <summary>
    /// A HitObject describes an object in a Beatmap.
    /// <para>
    /// HitObjects may contain more properties for which you should be checking through the IHas* types.
    /// </para>
    /// </summary>
    public class HitObject
    {
        /// <summary>
        /// The time at which the HitObject starts.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// The sample to be played when this HitObject is hit.
        /// </summary>
        public HitSampleInfo Sample { get; set; }
    }
}
