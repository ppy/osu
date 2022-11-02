// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Osu.Objects
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

        private double ratio = 1;

        /// <summary>
        /// The distance ratio of this segment.
        /// Greater than 1 for accelerating, less than 1 for decelerating.
        /// </summary>
        [JsonProperty]
        public double Ratio
        {
            get => ratio;
            set
            {
                if (value == ratio)
                    return;

                ratio = value;
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
        /// <param name="ratio">The initial ratio.</param>
        public StreamControlPoint(double time, int count, double ratio = 1)
            : this()
        {
            Time = time;
            Count = count;
            Ratio = ratio;
        }

        public bool Equals(StreamControlPoint other) => Time == other.Time && Count == other.Count && Ratio == other.Ratio;
    }
}
