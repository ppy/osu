// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public class WorkingBeatmap : IDisposable
    {
        public readonly BeatmapInfo BeatmapInfo;

        public readonly BeatmapSetInfo BeatmapSetInfo;
        private readonly BeatmapDatabase database;

        public readonly bool WithStoryboard;

        private ArchiveReader getReader() => database?.GetReader(BeatmapSetInfo);

        private Texture background;
        private object backgroundLock = new object();
        public Texture Background
        {
            get
            {
                lock (backgroundLock)
                {
                    if (background != null) return background;

                    if (BeatmapInfo?.Metadata?.BackgroundFile == null) return null;

                    try
                    {
                        using (var reader = getReader())
                            background = new TextureStore(new RawTextureLoaderStore(reader), false).Get(BeatmapInfo.Metadata.BackgroundFile);
                    }
                    catch { }

                    return background;
                }
            }
            set { lock (backgroundLock) background = value; }
        }

        private Beatmap beatmap;
        private object beatmapLock = new object();
        public Beatmap Beatmap
        {
            get
            {
                lock (beatmapLock)
                {
                    if (beatmap != null) return beatmap;

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
                                    decoder?.Decode(stream, beatmap);
                        }
                    }
                    catch { }

                    return beatmap;
                }
            }
            set { lock (beatmapLock) beatmap = value; }
        }

        private ArchiveReader trackReader;
        private AudioTrack track;
        private object trackLock = new object();
        public AudioTrack Track
        {
            get
            {
                lock (trackLock)
                {
                    if (track != null) return track;

                    try
                    {
                        //store a reference to the reader as we may continue accessing the stream in the background.
                        trackReader = getReader();
                        var trackData = trackReader?.GetStream(BeatmapInfo.Metadata.AudioFile);
                        if (trackData != null)
                            track = new AudioTrackBass(trackData);
                    }
                    catch { }

                    return track;
                }
            }
            set { lock (trackLock) track = value; }
        }

        public bool TrackLoaded => track != null;

        public WorkingBeatmap(Beatmap beatmap)
        {
            this.beatmap = beatmap;
        }

        public WorkingBeatmap(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, BeatmapDatabase database, bool withStoryboard = false)
        {
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapSetInfo;
            this.database = database;
            this.WithStoryboard = withStoryboard;
        }

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                track?.Dispose();
                background?.Dispose();
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void TransferTo(WorkingBeatmap working)
        {
            if (track != null && BeatmapInfo.AudioEquals(working.BeatmapInfo))
                working.track = track;
        }
    }
}
