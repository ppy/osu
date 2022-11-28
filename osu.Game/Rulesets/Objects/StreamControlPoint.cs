// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Objects
{
    public class StreamControlPoint : IEquatable<StreamControlPoint>
    {
        private double time;

        /// <summary>
        /// The time of the start of this segment.
        /// </summary>
        [JsonProperty]
        public double Time
        {
            get => time;
            set
            {
                if (value == time)
                    return;

                time = value;
                Changed?.Invoke();
            }
        }

        private int count;

        /// <summary>
        /// The number of circles in this segment.
        /// </summary>
        [JsonProperty]
        public int Count
        {
            get => count;
            set
            {
                if (value == count)
                    return;

                count = value;
                Changed?.Invoke();
            }
        }

        private double acceleration;

        /// <summary>
        /// The acceleration factor of this segment.
        /// Greater than 0 for accelerating, less than 0 for decelerating.
        /// </summary>
        [JsonProperty]
        public double Acceleration
        {
            get => acceleration;
            set
            {
                if (value == acceleration)
                    return;

                acceleration = value;
                Changed?.Invoke();
            }
        }

        private bool exponential;

        /// <summary>
        /// The acceleration type of this segment.
        /// False for quadratic, true for exponential.
        /// </summary>
        [JsonProperty]
        public bool Exponential
        {
            get => exponential;
            set
            {
                if (value == exponential)
                    return;

                exponential = value;
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Invoked when any property of this <see cref="StreamControlPoint"/> is changed.
        /// </summary>
        public event Action? Changed;

        /// <summary>
        /// Creates a new <see cref="StreamControlPoint"/>.
        /// </summary>
        public StreamControlPoint()
        {
        }

        /// <summary>
        /// Creates a new <see cref="StreamControlPoint"/> with a provided position and type.
        /// </summary>
        /// <param name="time">The initial time.</param>
        /// <param name="count">The initial count.</param>
        /// <param name="acceleration">The initial acceleration.</param>
        /// <param name="exponential">The initial acceleration type.</param>
        public StreamControlPoint(double time, int count, double acceleration = 0, bool exponential = false)
            : this()
        {
            Time = time;
            Count = count;
            Acceleration = acceleration;
            Exponential = exponential;
        }

        public bool Equals(StreamControlPoint other) => Time == other.Time && Count == other.Count && Acceleration == other.Acceleration && Exponential == other.Exponential;
    }
}
