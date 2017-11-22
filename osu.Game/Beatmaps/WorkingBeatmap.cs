﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Metadata = beatmapInfo.Metadata ?? BeatmapSetInfo?.Metadata ?? new BeatmapMetadata();

            Mods.ValueChanged += mods => applyRateAdjustments();

            beatmap = new AsyncLazy<Beatmap>(populateBeatmap);
            background = new AsyncLazy<Texture>(populateBackground);
            track = new AsyncLazy<Track>(populateTrack);
            waveform = new AsyncLazy<Waveform>(populateWaveform);
        }

        protected abstract Beatmap GetBeatmap();
        protected abstract Texture GetBackground();
        protected abstract Track GetTrack();
        protected virtual Waveform GetWaveform() => new Waveform();

        public bool BeatmapLoaded => beatmap.IsValueCreated;
        public Beatmap Beatmap => beatmap.Value.Result;
        public async Task<Beatmap> GetBeatmapAsync() => await beatmap.Value;

        private readonly AsyncLazy<Beatmap> beatmap;

        private Beatmap populateBeatmap()
        {
            var b = GetBeatmap() ?? new Beatmap();

            // use the database-backed info.
            b.BeatmapInfo = BeatmapInfo;

            return b;
        }

        public bool BackgroundLoaded => background.IsValueCreated;
        public Texture Background => background.Value.Result;
        public async Task<Texture> GetBackgroundAsync() => await background.Value;
        private AsyncLazy<Texture> background;

        private Texture populateBackground() => GetBackground();

        public bool TrackLoaded => track.IsValueCreated;
        public Track Track => track.Value.Result;
        public async Task<Track> GetTrackAsync() => await track.Value;
        private AsyncLazy<Track> track;

        private Track populateTrack()
        {
            // we want to ensure that we always have a track, even if it's a fake one.
            var t = GetTrack() ?? new TrackVirtual();
            applyRateAdjustments(t);
            return t;
        }

        public bool WaveformLoaded => waveform.IsValueCreated;
        public Waveform Waveform => waveform.Value.Result;
        public async Task<Waveform> GetWaveformAsync() => await waveform.Value;
        private readonly AsyncLazy<Waveform> waveform;

        private Waveform populateWaveform() => GetWaveform();

        public void TransferTo(WorkingBeatmap other)
        {
            if (track.IsValueCreated && Track != null && BeatmapInfo.AudioEquals(other.BeatmapInfo))
                other.track = track;

            if (background.IsValueCreated && Background != null && BeatmapInfo.BackgroundEquals(other.BeatmapInfo))
                other.background = background;
        }

        public virtual void Dispose()
        {
            if (BackgroundLoaded) Background?.Dispose();
            if (WaveformLoaded) Waveform?.Dispose();
        }

        public void DisposeTrack()
        {
            if (TrackLoaded) Track?.Dispose();
        }

        private void applyRateAdjustments(Track t = null)
        {
            if (t == null && track.IsValueCreated) t = Track;
            if (t == null) return;

            t.ResetSpeedAdjustments();
            foreach (var mod in Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(t);
        }

        public class AsyncLazy<T> : Lazy<Task<T>>
        {
            public AsyncLazy(Func<T> valueFactory)
                : base(() => Task.Run(valueFactory))
            {
            }
        }
    }
}
