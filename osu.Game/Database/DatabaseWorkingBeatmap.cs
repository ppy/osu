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

            private ArchiveReader GetReader() => database?.GetReader(BeatmapSetInfo);

            private Beatmap beatmap;
            private object beatmapLock = new object();
            public override Beatmap Beatmap
            {
                get
                {
                    lock (beatmapLock)
                    {
                        if (beatmap != null) return beatmap;
    
                        try
                        {
                            using (var reader = GetReader())
                            {
                                BeatmapDecoder decoder;
                                using (var stream = new StreamReader(reader.GetStream(BeatmapInfo.Path)))
                                {
                                    decoder = BeatmapDecoder.GetDecoder(stream);
                                    beatmap = decoder?.Decode(stream);
                                }
    
                                if (WithStoryboard && beatmap != null && BeatmapSetInfo.StoryboardFile != null)
                                    using (var stream = new StreamReader(reader.GetStream(BeatmapSetInfo.StoryboardFile)))
                                        decoder?.Decode(stream, beatmap);
                            }
                        }
                        catch { return null; }
    
                        return beatmap;
                    }
                }
            }

            private object backgroundLock = new object();
            private Texture background;
            public override Texture Background
            {
                get
                {
                    lock (backgroundLock)
                    {
                        if (background != null) return background;
    
                        if (BeatmapInfo?.Metadata?.BackgroundFile == null) return null;
    
                        try
                        {
                            using (var reader = GetReader())
                            {
                                background = new TextureStore(
                                    new RawTextureLoaderStore(reader),
                                    false).Get(BeatmapInfo.Metadata.BackgroundFile);
                            }
                        }
                        catch { return null; }
    
                        return background;
                    }
                }
            }

            private ArchiveReader trackReader;
            private Track track;
            private object trackLock = new object();
            public override Track Track
            {
                get
                {
                    lock (trackLock)
                    {
                        if (track != null) return track;
    
                        try
                        {
                            //store a reference to the reader as we may continue accessing the stream in the background.
                            trackReader = GetReader();
                            var trackData = trackReader?.GetStream(BeatmapInfo.Metadata.AudioFile);
                            if (trackData != null)
                                track = new TrackBass(trackData);
                        }
                        catch { return null; }
    
                        return track;
                    }
                }
            }
            
            public override bool TrackLoaded => track != null;

            public override void TransferTo(WorkingBeatmap other)
            {
                var _other = (DatabaseWorkingBeatmap)other;
                if (track != null && BeatmapInfo.AudioEquals(_other.BeatmapInfo))
                    _other.track = track;
            }
            
            public override void Dispose()
            {
                background?.Dispose();
                background = null;
            }
        }
    }
}