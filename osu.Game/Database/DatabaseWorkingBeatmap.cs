// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;

namespace osu.Game.Database
{
    public partial class BeatmapDatabase
    {
        private class DatabaseWorkingBeatmap : WorkingBeatmap
        {
            private readonly BeatmapDatabase database;

            public DatabaseWorkingBeatmap(BeatmapDatabase database, BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, bool withStoryboard = false)
                : base(beatmapInfo, beatmapSetInfo, withStoryboard)
            {
                this.database = database;
            }

            private ArchiveReader getReader() => database?.GetReader(BeatmapSetInfo);
            
            protected override Beatmap GetBeatmap()
            {
                Beatmap beatmap;
                try
                {
                    using (var reader = getReader())
                    {
                        BeatmapDecoder decoder;
                        using (var stream = new StreamReader(reader.GetStream(BeatmapInfo.Path)))
                        {
                            decoder = BeatmapDecoder.GetDecoder(stream);
                            beatmap = decoder?.Decode(stream);
                        }

                        if (WithStoryboard && beatmap != null && BeatmapSetInfo.StoryboardFile != null)
                            using (var stream = new StreamReader(reader.GetStream(BeatmapSetInfo.StoryboardFile)))
                                decoder.Decode(stream, beatmap);
                    }
                }
                catch { return null; }
                return beatmap;
            }
            
            protected override Texture GetBackground()
            {
                Texture background;
                if (BeatmapInfo?.Metadata?.BackgroundFile == null)
                    return null;
                try
                {
                    using (var reader = getReader())
                    {
                        background = new TextureStore(
                            new RawTextureLoaderStore(reader),
                            false).Get(BeatmapInfo.Metadata.BackgroundFile);
                    }
                }
                catch { return null; }
                return background;
            }

            private ArchiveReader trackReader;
            protected override Track GetTrack()
            {
                Track track;
                try
                {
                    //store a reference to the reader as we may continue accessing the stream in the background.
                    trackReader = getReader();
                    var trackData = trackReader?.GetStream(BeatmapInfo.Metadata.AudioFile);
                    track = trackData == null ? null : new TrackBass(trackData);
                }
                catch { return null; }
                return track;
            }
        }
    }
}