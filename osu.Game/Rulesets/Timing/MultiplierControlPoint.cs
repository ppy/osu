// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A control point which adds an aggregated multiplier based on the provided <see cref="TimingPoint"/>'s BeatLength and <see cref="EffectPoint"/>'s SpeedMultiplier.
    /// </summary>
    public class MultiplierControlPoint : IComparable<MultiplierControlPoint>, IControlPoint
    {
        /// <summary>
        /// The time in milliseconds at which this <see cref="MultiplierControlPoint"/> starts.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The aggregate multiplier which this <see cref="MultiplierControlPoint"/> provides.
        /// </summary>
        public double Multiplier => Velocity * EffectPoint.ScrollSpeed * BaseBeatLength / TimingPoint.BeatLength;

        /// <summary>
        /// The base beat length to scale the <see cref="TimingPoint"/> provided multiplier relative to.
        /// </summary>
        /// <example>For a <see cref="BaseBeatLength"/> of 1000, a <see cref="TimingPoint"/> with a beat length of 500 will increase the multiplier by 2.</example>
        public double BaseBeatLength = TimingControlPoint.DEFAULT_BEAT_LENGTH;

        /// <summary>
        /// The velocity multiplier.
        /// </summary>
        public double Velocity = 1;

        /// <summary>
        /// The <see cref="TimingControlPoint"/> that provides the timing information for this <see cref="MultiplierControlPoint"/>.
        /// </summary>
        public TimingControlPoint TimingPoint = new TimingControlPoint();

        /// <summary>
        /// The <see cref="EffectControlPoint"/> that provides additional difficulty information for this <see cref="MultiplierControlPoint"/>.
        /// </summary>
        public EffectControlPoint EffectPoint = new EffectControlPoint();

        /// <summary>
        /// Creates a <see cref="MultiplierControlPoint"/>. This is required for JSON serialization
        /// </summary>
        public MultiplierControlPoint()
        {
        }

        /// <summary>
        /// Creates a <see cref="MultiplierControlPoint"/>.
        /// </summary>
        /// <param name="time">The start time of this <see cref="MultiplierControlPoint"/>.</param>
        public MultiplierControlPoint(double time)
        {
            Time = time;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        public int CompareTo(MultiplierControlPoint other) => Time.CompareTo(other?.Time);
    }
}
