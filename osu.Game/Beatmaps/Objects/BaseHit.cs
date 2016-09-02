//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;

namespace osu.Game.Beatmaps.Objects
{
    /// <summary>
    /// A hitobject describes a point in a beatmap 
    /// </summary>
    public abstract class BaseHit
    {
        public double StartTime;
        public double? EndTime;

        public HitSampleInfo Sample;
    }
}
