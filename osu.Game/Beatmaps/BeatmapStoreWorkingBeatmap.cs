// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Beatmaps
{
    internal class BeatmapStoreWorkingBeatmap : WorkingBeatmap
    {
        private readonly Func<IResourceStore<byte[]>> getStore;

        public BeatmapStoreWorkingBeatmap(Func<IResourceStore<byte[]>> getStore, BeatmapInfo beatmapInfo)
            : base(beatmapInfo)
        {
            this.getStore = getStore;
        }

        protected override Beatmap GetBeatmap()
        {
            try
            {
                Beatmap beatmap;

                BeatmapDecoder decoder;
                using (var stream = new StreamReader(getStore().GetStream(BeatmapInfo.Path)))
                {
                    decoder = BeatmapDecoder.GetDecoder(stream);
                    beatmap = decoder.Decode(stream);
                }

                if (beatmap == null || BeatmapSetInfo.StoryboardFile == null)
                    return beatmap;

                using (var stream = new StreamReader(getStore().GetStream(BeatmapSetInfo.StoryboardFile)))
                    decoder.Decode(stream, beatmap);


                return beatmap;
            }
            catch { return null; }
        }

        protected override Texture GetBackground()
        {
            if (Metadata?.BackgroundFile == null)
                return null;

            try
            {
                return new TextureStore(new RawTextureLoaderStore(getStore()), false).Get(Metadata.BackgroundFile);
            }
            catch { return null; }
        }

        protected override Track GetTrack()
        {
            try
            {
                var trackData = getStore().GetStream(Metadata.AudioFile);
                return trackData == null ? null : new TrackBass(trackData);
            }
            catch { return new TrackVirtual(); }
        }
    }
}
