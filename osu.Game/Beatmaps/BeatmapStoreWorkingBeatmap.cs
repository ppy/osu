// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Beatmaps
{
    internal class BeatmapStoreWorkingBeatmap : WorkingBeatmap
    {
        private readonly IResourceStore<byte[]> store;

        public BeatmapStoreWorkingBeatmap(IResourceStore<byte[]> store, BeatmapInfo beatmapInfo)
            : base(beatmapInfo)
        {
            this.store = store;
        }

        protected override Beatmap GetBeatmap()
        {
            try
            {
                Beatmap beatmap;

                BeatmapDecoder decoder;
                using (var stream = new StreamReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                {
                    decoder = BeatmapDecoder.GetDecoder(stream);
                    beatmap = decoder.Decode(stream);
                }

                if (beatmap == null || BeatmapSetInfo.StoryboardFile == null)
                    return beatmap;

                using (var stream = new StreamReader(store.GetStream(getPathForFile(BeatmapSetInfo.StoryboardFile))))
                    decoder.Decode(stream, beatmap);


                return beatmap;
            }
            catch { return null; }
        }

        private string getPathForFile(string filename) => BeatmapSetInfo.Files.First(f => f.Filename == filename).StoragePath;

        protected override Texture GetBackground()
        {
            if (Metadata?.BackgroundFile == null)
                return null;

            try
            {
                return new TextureStore(new RawTextureLoaderStore(store), false).Get(getPathForFile(Metadata.BackgroundFile));
            }
            catch { return null; }
        }

        protected override Track GetTrack()
        {
            try
            {
                var trackData = store.GetStream(getPathForFile(Metadata.AudioFile));
                return trackData == null ? null : new TrackBass(trackData);
            }
            catch { return new TrackVirtual(); }
        }
    }
}
