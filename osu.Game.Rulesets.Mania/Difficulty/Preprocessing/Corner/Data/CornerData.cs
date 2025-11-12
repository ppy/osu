// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner.Data
{
    public class CornerData
    {
        /// <summary>Time corners used for final difficulty sampling (union of Base and Accuracy corners)</summary>
        public double[] TimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>Base time corners derived from note positions with small offsets</summary>
        public double[] BaseTimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>Accuracy-focused time corners with wider windows around notes</summary>
        public double[] AccuracyTimeCorners { get; set; } = Array.Empty<double>();
    }
}

