// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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
        /// The amount of circles in this beatmap.
        /// </summary>
        public int CircleCount { get; }

        /// <summary>
        /// The amount of sliders in this beatmap.
        /// </summary>
        public int SliderCount { get; }

        /// <summary>
        /// The amount of plays this beatmap has.
        /// </summary>
        public int PlayCount { get; }

        /// <summary>
        /// The amount of passes this beatmap has.
        /// </summary>
        public int PassCount { get; }

        APIFailTimes? FailTimes { get; }
    }
}
