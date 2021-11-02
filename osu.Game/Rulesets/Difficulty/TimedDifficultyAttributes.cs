// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// Wraps a <see cref="DifficultyAttributes"/> object and adds a time value for which the attribute is valid.
    /// Output by <see cref="DifficultyCalculator.CalculateTimed"/>.
    /// </summary>
    public class TimedDifficultyAttributes : IComparable<TimedDifficultyAttributes>
    {
        /// <summary>
        /// The non-clock adjusted time value at which the attributes take effect.
        /// </summary>
        public readonly double Time;

        /// <summary>
        /// The attributes.
        /// </summary>
        public readonly DifficultyAttributes Attributes;

        public TimedDifficultyAttributes(double time, DifficultyAttributes attributes)
        {
            Time = time;
            Attributes = attributes;
        }

        public int CompareTo(TimedDifficultyAttributes other) => Time.CompareTo(other.Time);
    }
}
