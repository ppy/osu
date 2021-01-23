// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Structure holding the player's expected performance parameters on a given segment of a map.
    /// </summary>
    internal struct MapSectionPerformanceData
    {
        /// <summary>
        /// The expected amount of time until a full combo is attained for the section.
        /// </summary>
        public double ExpectedTimeUntilFullCombo { get; set; }

        /// <summary>
        /// The expected probability of achieving a full combo on a single play-through of the section.
        /// </summary>
        public double FullComboProbability { get; set; }

        /// <summary>
        /// The exponentiated difficulty value of this particular section.
        /// </summary>
        public double ExponentiatedDifficulty { get; set; }

        /// <summary>
        /// The start time of the section.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// The end time of the section.
        /// </summary>
        public double EndTime { get; set; }

        /// <summary>
        /// The length of a single playthrough of the section.
        /// </summary>
        public double Duration => EndTime - StartTime;

        /// <summary>
        /// Expected time taken to play all failed attempts before achieving a full combo.
        /// </summary>
        public double ExpectedDurationOfFailedFullComboAttempts => ExpectedTimeUntilFullCombo - Duration;
    }
}
