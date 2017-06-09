// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;

namespace osu.Game.Rulesets.Timing
{
    public class MultiplierControlPoint : IJsonSerializable, IComparable<MultiplierControlPoint>
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

        public MultiplierControlPoint()
        {
        }

        public MultiplierControlPoint(double startTime)
        {
            StartTime = startTime;
        }

        public MultiplierControlPoint(double startTime, MultiplierControlPoint other)
            : this(startTime)
        {
            TimingPoint = other.TimingPoint;
            DifficultyPoint = other.DifficultyPoint;
        }

        public int CompareTo(MultiplierControlPoint other) => StartTime.CompareTo(other?.StartTime);
    }
}