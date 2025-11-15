// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner
{
    /// <summary>
    /// Container for time-based sampling points used in difficulty calculation.
    /// Stores three distinct sets of time corners that define when difficulty metrics should be sampled.
    /// </summary>
    public class CornerData
    {
        /// <summary>
        /// Final set of time corners used for sampling all difficulty metrics.
        /// This is the union of <see cref="BaseTimeCorners"/> and <see cref="AccuracyTimeCorners"/>.
        /// </summary>
        public double[] TimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Base time corners derived directly from note positions with minor offsets.
        /// These capture fundamental rhythm and pattern changes in the beatmap.
        /// </summary>
        public double[] BaseTimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Accuracy-focused time corners with expanded windows around notes.
        /// These emphasize timing precision requirements by sampling wider intervals around hit objects.
        /// </summary>
        public double[] AccuracyTimeCorners { get; set; } = Array.Empty<double>();
    }
}
