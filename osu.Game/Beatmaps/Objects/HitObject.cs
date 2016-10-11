﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Samples;
using osu.Game.GameModes.Play;

namespace osu.Game.Beatmaps.Objects
{
    /// <summary>
    /// A hitobject describes a point in a beatmap 
    /// </summary>
    public abstract class HitObject
    {
        public double StartTime;
        public double? EndTime;

        public double Duration => (EndTime ?? StartTime) - StartTime;

        public HitSampleInfo Sample;

        public static HitObject Parse(PlayMode mode, string val)
        {
            switch (mode)
            {
                case PlayMode.Osu:
                    return OsuBaseHit.Parse(val);
                default:
                    return null;
            }
        }
    }
}
