// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkin : LegacySkin
    {
        // Null should be returned in the case of no colours provided to fallback into current skin's colours.
        protected override bool AllowDefaultColoursFallback => false;

        public LegacyBeatmapSkin(BeatmapInfo beatmap, IResourceStore<byte[]> storage, AudioManager audioManager)
            : base(createSkinInfo(beatmap), new LegacySkinResourceStore<BeatmapSetFileInfo>(beatmap.BeatmapSet, storage), audioManager, beatmap.Path)
        {
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmap) =>
            new SkinInfo { Name = beatmap.ToString(), Creator = beatmap.Metadata.Author.ToString() };
    }
}
