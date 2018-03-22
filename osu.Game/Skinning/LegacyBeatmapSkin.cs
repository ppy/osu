// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkin : LegacySkin
    {
        public LegacyBeatmapSkin(BeatmapInfo beatmap, IResourceStore<byte[]> storage, AudioManager audioManager)
            : base(createSkinInfo(beatmap), new LegacySkinResourceStore<BeatmapSetFileInfo>(beatmap.BeatmapSet, storage), audioManager, beatmap.Path)
        {
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmap) =>
            new SkinInfo { Name = beatmap.ToString(), Creator = beatmap.Metadata.Author.ToString() };
    }
}
