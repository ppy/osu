// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Timing
{
    public class MultiplierControlPoint
    {
        /// <summary>
        /// The time in milliseconds at which this control point starts.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// The multiplier which this control point provides.
        /// </summary>
        public double Multiplier => 1000 / TimingPoint.BeatLength / DifficultyPoint.SpeedMultiplier;

        public TimingControlPoint TimingPoint = new TimingControlPoint();
        public DifficultyControlPoint DifficultyPoint = new DifficultyControlPoint();

        public MultiplierControlPoint(double startTime)
        {
            StartTime = startTime;
        }
    }
}