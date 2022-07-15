// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data
{
    /// <summary>
    /// Encodes a list of <see cref="ColourEncoding"/>s, grouped together by back and forth repetition of the same
    /// <see cref="ColourEncoding"/>. Also stores the repetition interval between this and the previous <see cref="CoupledColourEncoding"/>.
    /// </summary>
    public class CoupledColourEncoding
    {
        /// <summary>
        /// Maximum amount of <see cref="CoupledColourEncoding"/>s to look back to find a repetition.
        /// </summary>
        private const int max_repetition_interval = 16;

        /// <summary>
        /// The <see cref="ColourEncoding"/>s that are grouped together within this <see cref="CoupledColourEncoding"/>.
        /// </summary>
        public List<ColourEncoding> Payload = new List<ColourEncoding>();

        /// <summary>
        /// The previous <see cref="CoupledColourEncoding"/>. This is used to determine the repetition interval.
        /// </summary>
        public CoupledColourEncoding? Previous = null;

        /// <summary>
        /// How many <see cref="CoupledColourEncoding"/> between the current and previous identical <see cref="CoupledColourEncoding"/>.
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; } = max_repetition_interval + 1;

        /// <summary>
        /// Returns true if other is considered a repetition of this encoding. This is true if other's first two payload
        /// identical mono lengths.
        /// </summary>
        private bool isRepetitionOf(CoupledColourEncoding other)
        {
            if (Payload.Count != other.Payload.Count) return false;

            for (int i = 0; i < Math.Min(Payload.Count, 2); i++)
            {
                if (!Payload[i].HasIdenticalMonoLength(other.Payload[i])) return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the closest previous <see cref="CoupledColourEncoding"/> that has the identical <see cref="Payload"/>.
        /// Interval is defined as the amount of <see cref="CoupledColourEncoding"/> chunks between the current and repeated encoding.
        /// </summary>
        public void FindRepetitionInterval()
        {
            if (Previous?.Previous == null)
            {
                RepetitionInterval = max_repetition_interval + 1;
                return;
            }

            CoupledColourEncoding? other = Previous.Previous;
            int interval = 2;

            while (interval < max_repetition_interval)
            {
                if (isRepetitionOf(other))
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    return;
                }

                other = other.Previous;
                if (other == null) break;

                ++interval;
            }

            RepetitionInterval = max_repetition_interval + 1;
        }
    }
}
