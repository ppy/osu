// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// Data for generating a performance breakdown by comparing performance to a perfect play.
    /// </summary>
    public class PerformanceBreakdown
    {
        /// <summary>
        /// Actual gameplay performance.
        /// </summary>
        public PerformanceAttributes Performance { get; set; }

        /// <summary>
        /// Performance of a perfect play for comparison.
        /// </summary>
        public PerformanceAttributes PerfectPerformance { get; set; }
    }
}
