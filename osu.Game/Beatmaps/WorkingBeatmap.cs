// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Beatmaps
{
    public abstract class WorkingBeatmap : IDisposable
    {
        public readonly BeatmapInfo BeatmapInfo;

        public readonly BeatmapSetInfo BeatmapSetInfo;

        public readonly BeatmapMetadata Metadata;

        public readonly Bindable<IEnumerable<Mod>> Mods = new Bindable<IEnumerable<Mod>>(new Mod[] { });

        protected WorkingBeatmap(BeatmapInfo beatmapInfo)
        {
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapInfo.BeatmapSet;
            Metadata = beatmapInfo.Metadata ?? BeatmapSetInfo.Metadata;

            Mods.ValueChanged += mods => applyRateAdjustments();
        }

        private void applyRateAdjustments()
        {
            var t = track;
            if (t == null) return;

            t.ResetSpeedAdjustments();
            foreach (var mod in Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(t);
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
                    if (beatmap != null) return beatmap;

                    beatmap = GetBeatmap() ?? new Beatmap();

                    // use the database-backed info.
                    beatmap.BeatmapInfo = BeatmapInfo;

                    return beatmap;
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
                    if (track != null) return track;

                    // we want to ensure that we always have a track, even if it's a fake one.
                    track = GetTrack() ?? new TrackVirtual();

                    applyRateAdjustments();
                    return track;
                }
            }
        }

        public bool TrackLoaded => track != null;

        public void TransferTo(WorkingBeatmap other)
        {
            lock (trackLock)
            {
                if (track != null && BeatmapInfo.AudioEquals(other.BeatmapInfo))
                    other.track = track;
            }

            if (background != null && BeatmapInfo.BackgroundEquals(other.BeatmapInfo))
                other.background = background;
        }

        public virtual void Dispose()
        {
            background?.Dispose();
            background = null;
        }

        public void DisposeTrack()
        {
            lock (trackLock)
            {
                track?.Dispose();
                track = null;
            }
        }
    }
}
