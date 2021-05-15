// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Skinning
{
    public static class BeatmapSkinExtensions
    {
        public static SkinInfo CreateSkinInfo(BeatmapInfo beatmap) => new SkinInfo
        {
            Name = beatmap.ToString(),
            Creator = beatmap.Metadata?.AuthorString,
        };
    }
}
