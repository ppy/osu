// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Mods;
using System;
using System.Collections.Generic;
using System.IO;

namespace osu.Game.Beatmaps
{
    public abstract class WorkingBeatmap : IDisposable
    {
        public readonly BeatmapInfo BeatmapInfo;

        public readonly BeatmapSetInfo BeatmapSetInfo;

        /// <summary>
        /// A play mode that is preferred for this beatmap. PlayMode will become this mode where conversion is feasible,
        /// or otherwise to the beatmap's default.
        /// </summary>
        public PlayMode? PreferredPlayMode;

        public PlayMode PlayMode => beatmap?.BeatmapInfo?.Mode > PlayMode.Osu ? beatmap.BeatmapInfo.Mode : PreferredPlayMode ?? PlayMode.Osu;

        public readonly Bindable<IEnumerable<Mod>> Mods = new Bindable<IEnumerable<Mod>>();

        public readonly bool WithStoryboard;

        protected abstract ArchiveReader GetReader();

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, bool withStoryboard = false)
        {
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapSetInfo;
            WithStoryboard = withStoryboard;
        }

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
                        using (var reader = GetReader())
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
                    catch { }

                    return beatmap;
                }
            }
            set { lock (beatmapLock) beatmap = value; }
        }

        private ArchiveReader trackReader;
        private Track track;
        private object trackLock = new object();
        public Track Track
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
                    catch { }

                    return track;
                }
            }
            set { lock (trackLock) track = value; }
        }

        public bool TrackLoaded => track != null;

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
