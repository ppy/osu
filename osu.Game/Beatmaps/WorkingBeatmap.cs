// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Mods;
using System;
using System.Collections.Generic;

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

        public PlayMode PlayMode => Beatmap?.BeatmapInfo?.Mode > PlayMode.Osu ? Beatmap.BeatmapInfo.Mode : PreferredPlayMode ?? PlayMode.Osu;

        public readonly Bindable<IEnumerable<Mod>> Mods = new Bindable<IEnumerable<Mod>>();

        public readonly bool WithStoryboard;

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, bool withStoryboard = false)
        {
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapSetInfo;
            WithStoryboard = withStoryboard;
        }

        protected abstract Beatmap GetBeatmap();
        protected abstract Texture GetBackground();
        protected abstract Track GetTrack();
        
        private Beatmap beatmap;
        private readonly object beatmapLock = new object();
        public Beatmap Beatmap
        {
            get
            {
                lock (beatmapLock)
                {
                    return beatmap ?? (beatmap = GetBeatmap());
                }
            }
        }
        
        private readonly object backgroundLock = new object();
        private Texture background;
        public Texture Background
        {
            get
            {
                lock (backgroundLock)
                {
                    return background ?? (background = GetBackground());
                }
            }
        }

        private Track track;
        private readonly object trackLock = new object();
        public Track Track
        {
            get
            {
                lock (trackLock)
                {
                    return track ?? (track = GetTrack());
                }
            }
        }

        public bool TrackLoaded => track != null;

        public void TransferTo(WorkingBeatmap other)
        {
            if (track != null && BeatmapInfo.AudioEquals(other.BeatmapInfo))
                other.track = track;
        }
        
        public virtual void Dispose()
        {
            track?.Dispose();
            track = null;
            background?.Dispose();
            background = null;
        }
    }
}
