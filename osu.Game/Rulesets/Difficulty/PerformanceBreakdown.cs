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
        /// Performance if the play was a perfect full combo (0 miss, max combo)
        /// </summary>
        public PerformanceAttributes FCPerformance { get; set; }

        /// <summary>
        /// Performance of a perfect play for comparison.
        /// </summary>
        public PerformanceAttributes PerfectPerformance { get; set; }

        /// <summary>
        /// Create a new performance breakdown.
        /// </summary>
        /// <param name="performance">Actual gameplay performance.</param>
        /// <param name="fcPerformance">Performance rewarded if the play was a full combo</param>
        /// <param name="perfectPerformance">Performance of a perfect play for comparison.</param>
        public PerformanceBreakdown(PerformanceAttributes performance, PerformanceAttributes fcPerformance, PerformanceAttributes perfectPerformance)
        {
            Performance = performance;
            FCPerformance = fcPerformance;
            PerfectPerformance = perfectPerformance;
        }
    }
}
