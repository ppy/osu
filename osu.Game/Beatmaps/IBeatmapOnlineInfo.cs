// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap info retrieved for previewing locally.
    /// </summary>
    public interface IBeatmapOnlineInfo
    {
        /// <summary>
        /// The max combo of this beatmap.
        /// </summary>
        int? MaxCombo { get; }

        /// <summary>
        /// The approach rate.
        /// </summary>
        float ApproachRate { get; }

        /// <summary>
        /// The circle size.
        /// </summary>
        float CircleSize { get; }

        /// <summary>
        /// The drain rate.
        /// </summary>
        float DrainRate { get; }

        /// <summary>
        /// The overall difficulty.
        /// </summary>
        float OverallDifficulty { get; }

        /// <summary>
        /// The amount of circles in this beatmap.
        /// </summary>
        int CircleCount { get; }

        /// <summary>
        /// The amount of sliders in this beatmap.
        /// </summary>
        int SliderCount { get; }

        /// <summary>
        /// The amount of spinners in tihs beatmap.
        /// </summary>
        int SpinnerCount { get; }

        /// <summary>
        /// The amount of plays this beatmap has.
        /// </summary>
        int PlayCount { get; }

        /// <summary>
        /// The amount of passes this beatmap has.
        /// </summary>
        int PassCount { get; }

        APIFailTimes? FailTimes { get; }

        /// <summary>
        /// The playable length in milliseconds of this beatmap.
        /// </summary>
        double HitLength { get; }
    }
}
