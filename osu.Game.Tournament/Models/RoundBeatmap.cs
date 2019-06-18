// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Tournament.Models
{
    public class RoundBeatmap
    {
        public int ID;
        public string Mods;

        public BeatmapInfo BeatmapInfo;
    }
}
