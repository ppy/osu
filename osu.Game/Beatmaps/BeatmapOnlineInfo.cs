// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap info retrieved for previewing locally without having the beatmap downloaded.
    /// </summary>
    public class BeatmapOnlineInfo
    {
        /// <summary>
        /// The amount of circles in this beatmap.
        /// </summary>
        public int CircleCount { get; set; }

        /// <summary>
        /// The amount of sliders in this beatmap.
        /// </summary>
        public int SliderCount { get; set; }

        /// <summary>
        /// The amount of plays this beatmap has.
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// The amount of passes this beatmap has.
        /// </summary>
        public int PassCount { get; set; }
    }
}
