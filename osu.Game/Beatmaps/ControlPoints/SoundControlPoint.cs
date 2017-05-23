// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.ControlPoints
{
    public class SoundControlPoint : ControlPoint
    {
        /// <summary>
        /// The default sample bank at this control point.
        /// </summary>
        public string SampleBank;

        /// <summary>
        /// The default sample volume at this control point.
        /// </summary>
        public int SampleVolume;
    }
}