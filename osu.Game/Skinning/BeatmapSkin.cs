// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Skinning
{
    public class BeatmapSkin : LegacySkin
    {
        public BeatmapSkin(BeatmapInfo beatmap, IResourceStore<byte[]> storage)
            : base(new SkinInfo { Name = beatmap.ToString(), Creator =  beatmap.Metadata.Author.ToString() })
        {
            storage = new LegacySkinResourceStore<BeatmapSetFileInfo>(beatmap.BeatmapSet, storage);

            // todo: sample support
            // samples = audioManager.GetSampleManager(storage);

            Textures = new TextureStore(new RawTextureLoaderStore(storage));

            var decoder = new LegacySkinDecoder();

            using (StreamReader reader = new StreamReader(storage.GetStream(beatmap.Path)))
            {
                Configuration = decoder.Decode(reader);
            }
        }
    }
}
