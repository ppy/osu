// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps.Formats;
using osu.Game.Graphics.Textures;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    public partial class BeatmapManager
    {
        protected class BeatmapManagerWorkingBeatmap : WorkingBeatmap
        {
            private readonly IResourceStore<byte[]> store;

            public BeatmapManagerWorkingBeatmap(IResourceStore<byte[]> store, BeatmapInfo beatmapInfo)
                : base(beatmapInfo)
            {
                this.store = store;
            }

            protected override Beatmap GetBeatmap()
            {
                try
                {
                    using (var stream = new StreamReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                    {
                        Decoder decoder = Decoder.GetDecoder(stream);
                        return decoder.DecodeBeatmap(stream);
                    }
                }
                catch
                {
                    return null;
                }
            }

            private string getPathForFile(string filename) => BeatmapSetInfo.Files.First(f => string.Equals(f.Filename, filename, StringComparison.InvariantCultureIgnoreCase)).FileInfo.StoragePath;

            protected override Texture GetBackground()
            {
                if (Metadata?.BackgroundFile == null)
                    return null;

                try
                {
                    return new LargeTextureStore(new RawTextureLoaderStore(store)).Get(getPathForFile(Metadata.BackgroundFile));
                }
                catch
                {
                    return null;
                }
            }

            protected override Track GetTrack()
            {
                try
                {
                    var trackData = store.GetStream(getPathForFile(Metadata.AudioFile));
                    return trackData == null ? null : new TrackBass(trackData);
                }
                catch
                {
                    return new TrackVirtual();
                }
            }

            protected override Waveform GetWaveform() => new Waveform(store.GetStream(getPathForFile(Metadata.AudioFile)));

            protected override Storyboard GetStoryboard()
            {
                Storyboard storyboard;
                try
                {
                    using (var beatmap = new StreamReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                    {
                        Decoder decoder = Decoder.GetDecoder(beatmap);

                        // todo: support loading from both set-wide storyboard *and* beatmap specific.

                        if (BeatmapSetInfo?.StoryboardFile == null)
                            storyboard = decoder.GetStoryboardDecoder().DecodeStoryboard(beatmap);
                        else
                        {
                            using (var reader = new StreamReader(store.GetStream(getPathForFile(BeatmapSetInfo.StoryboardFile))))
                                storyboard = decoder.GetStoryboardDecoder().DecodeStoryboard(beatmap, reader);
                        }
                    }
                }
                catch
                {
                    storyboard = new Storyboard();
                }

                storyboard.BeatmapInfo = BeatmapInfo;

                return storyboard;
            }
        }
    }
}
