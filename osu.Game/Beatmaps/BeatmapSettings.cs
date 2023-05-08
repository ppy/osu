// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps
{
    public class BeatmapSettings : IBeatmapSettings
    {
        public int[] Bookmarks { get; set; } = Array.Empty<int>();

        public CountdownType Countdown { get; set; }

        public int CountdownOffset { get; set; }
    }
}
