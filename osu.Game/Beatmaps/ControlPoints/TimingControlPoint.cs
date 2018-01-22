// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint
    {
        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public TimeSignatures TimeSignature = TimeSignatures.SimpleQuadruple;

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public double BeatLength = 1000;
    }
}
