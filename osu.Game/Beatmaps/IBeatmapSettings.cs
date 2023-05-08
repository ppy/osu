// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    public interface IBeatmapSettings
    {
        int[] Bookmarks { get; set; }

        CountdownType Countdown { get; set; }

        /// <summary>
        /// The number of beats to move the countdown backwards (compared to its default location).
        /// </summary>
        public int CountdownOffset { get; set; }
    }
}
