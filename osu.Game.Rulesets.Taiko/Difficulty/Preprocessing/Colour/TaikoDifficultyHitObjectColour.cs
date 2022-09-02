// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        /// <summary>
        /// The <see cref="MonoStreak"/> that encodes this note, only present if this is the first note within a <see cref="MonoStreak"/>
        /// </summary>
        public MonoStreak? MonoStreak;

        /// <summary>
        /// The <see cref="AlternatingMonoPattern"/> that encodes this note, only present if this is the first note within a <see cref="AlternatingMonoPattern"/>
        /// </summary>
        public AlternatingMonoPattern? AlternatingMonoPattern;

        /// <summary>
        /// The <see cref="RepeatingHitPattern"/> that encodes this note, only present if this is the first note within a <see cref="RepeatingHitPattern"/>
        /// </summary>
        public RepeatingHitPatterns? RepeatingHitPattern;
    }
}
