// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data
{
    /// <summary>
    /// Encodes a list of <see cref="MonoStreak"/>s.
    /// <see cref="MonoStreak"/>s with the same <see cref="MonoStreak.RunLength"/> are grouped together.
    /// </summary>
    public class AlternatingMonoPattern
    {
        /// <summary>
        /// <see cref="MonoStreak"/>s that are grouped together within this <see cref="AlternatingMonoPattern"/>.
        /// </summary>
        public readonly List<MonoStreak> MonoStreaks = new List<MonoStreak>();

        /// <summary>
        /// The parent <see cref="RepeatingHitPatterns"/> that contains this <see cref="AlternatingMonoPattern"/>
        /// </summary>
        public RepeatingHitPatterns Parent = null!;

        /// <summary>
        /// Index of this <see cref="AlternatingMonoPattern"/> within it's parent <see cref="RepeatingHitPatterns"/>
        /// </summary>
        public int Index;

        /// <summary>
        /// The first <see cref="TaikoDifficultyHitObject"/> in this <see cref="AlternatingMonoPattern"/>.
        /// </summary>
        public TaikoDifficultyHitObject FirstHitObject => MonoStreaks[0].FirstHitObject;

        /// <summary>
        /// Determine if this <see cref="AlternatingMonoPattern"/> is a repetition of another <see cref="AlternatingMonoPattern"/>. This
        /// is a strict comparison and is true if and only if the colour sequence is exactly the same.
        /// </summary>
        public bool IsRepetitionOf(AlternatingMonoPattern other)
        {
            return HasIdenticalMonoLength(other) &&
                   other.MonoStreaks.Count == MonoStreaks.Count &&
                   other.MonoStreaks[0].HitType == MonoStreaks[0].HitType;
        }

        /// <summary>
        /// Determine if this <see cref="AlternatingMonoPattern"/> has the same mono length of another <see cref="AlternatingMonoPattern"/>.
        /// </summary>
        public bool HasIdenticalMonoLength(AlternatingMonoPattern other)
        {
            return other.MonoStreaks[0].RunLength == MonoStreaks[0].RunLength;
        }
    }
}
