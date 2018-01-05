// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap info retrieved for previewing locally without having the beatmap downloaded.
    /// </summary>
    public class BeatmapOnlineInfo
    {
        /// <summary>
        /// The length in milliseconds of this beatmap's song.
        /// </summary>
        public double Length { get; set; }

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
