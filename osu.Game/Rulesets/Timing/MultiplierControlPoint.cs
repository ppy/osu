// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A control point which adds an aggregated multiplier based on the provided <see cref="TimingPoint"/>'s BeatLength and <see cref="DifficultyPoint"/>'s SpeedMultiplier.
    /// </summary>
    public class MultiplierControlPoint : IJsonSerializable, IComparable<MultiplierControlPoint>
    {
        /// <summary>
        /// The time in milliseconds at which this <see cref="MultiplierControlPoint"/> starts.
        /// </summary>
        public double StartTime;

        /// <summary>
        /// The aggregate multiplier which this <see cref="MultiplierControlPoint"/> provides.
        /// </summary>
        public double Multiplier => Velocity * DifficultyPoint.SpeedMultiplier * 1000 / TimingPoint.BeatLength;

        /// <summary>
        /// The velocity multiplier.
        /// </summary>
        public double Velocity = 1;

        /// <summary>
        /// The <see cref="TimingControlPoint"/> that provides the timing information for this <see cref="MultiplierControlPoint"/>.
        /// </summary>
        public TimingControlPoint TimingPoint = new TimingControlPoint();

        /// <summary>
        /// The <see cref="DifficultyControlPoint"/> that provides additional difficulty information for this <see cref="MultiplierControlPoint"/>.
        /// </summary>
        public DifficultyControlPoint DifficultyPoint = new DifficultyControlPoint();

        /// <summary>
        /// Creates a <see cref="MultiplierControlPoint"/>. This is required for JSON serialization
        /// </summary>
        public MultiplierControlPoint()
        {
        }

        /// <summary>
        /// Creates a <see cref="MultiplierControlPoint"/>.
        /// </summary>
        /// <param name="startTime">The start time of this <see cref="MultiplierControlPoint"/>.</param>
        public MultiplierControlPoint(double startTime)
        {
            StartTime = startTime;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        public int CompareTo(MultiplierControlPoint other) => StartTime.CompareTo(other?.StartTime);
    }
}
