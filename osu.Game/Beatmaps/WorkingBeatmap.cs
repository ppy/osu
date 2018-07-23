// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Storyboards;
using osu.Framework.IO.File;
using System.IO;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Beatmaps
{
    public abstract partial class WorkingBeatmap : IDisposable
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

            beatmap = new AsyncLazy<IBeatmap>(populateBeatmap);
            background = new AsyncLazy<Texture>(populateBackground, b => b == null || !b.IsDisposed);
            track = new AsyncLazy<Track>(populateTrack);
            waveform = new AsyncLazy<Waveform>(populateWaveform);
            storyboard = new AsyncLazy<Storyboard>(populateStoryboard);
            skin = new AsyncLazy<Skin>(populateSkin);
        }

        /// <summary>
        /// Saves the <see cref="Beatmaps.Beatmap"/>.
        /// </summary>
        /// <returns>The absolute path of the output file.</returns>
        public string Save()
        {
            var path = FileSafety.GetTempPath(Guid.NewGuid().ToString().Replace("-", string.Empty) + ".json");
            using (var sw = new StreamWriter(path))
                sw.WriteLine(Beatmap.Serialize());
            return path;
        }

        protected abstract IBeatmap GetBeatmap();
        protected abstract Texture GetBackground();
        protected abstract Track GetTrack();
        protected virtual Skin GetSkin() => new DefaultSkin();
        protected virtual Waveform GetWaveform() => new Waveform();
        protected virtual Storyboard GetStoryboard() => new Storyboard { BeatmapInfo = BeatmapInfo };

        public bool BeatmapLoaded => beatmap.IsResultAvailable;
        public IBeatmap Beatmap => beatmap.Value.Result;
        public async Task<IBeatmap> GetBeatmapAsync() => await beatmap.Value;
        private readonly AsyncLazy<IBeatmap> beatmap;

        private IBeatmap populateBeatmap()
        {
            var b = GetBeatmap() ?? new Beatmap();

            // use the database-backed info.
            b.BeatmapInfo = BeatmapInfo;

            return b;
        }

        /// <summary>
        /// Constructs a playable <see cref="IBeatmap"/> from <see cref="Beatmap"/> using the applicable converters for a specific <see cref="RulesetInfo"/>.
        /// <para>
        /// The returned <see cref="IBeatmap"/> is in a playable state - all <see cref="HitObject"/> and <see cref="BeatmapDifficulty"/> <see cref="Mod"/>s
        /// have been applied, and <see cref="HitObject"/>s have been fully constructed.
        /// </para>
        /// </summary>
        /// <param name="ruleset">The <see cref="RulesetInfo"/> to create a playable <see cref="IBeatmap"/> for.</param>
        /// <returns>The converted <see cref="IBeatmap"/>.</returns>
        /// <exception cref="BeatmapInvalidForRulesetException">If <see cref="Beatmap"/> could not be converted to <paramref name="ruleset"/>.</exception>
        public IBeatmap GetPlayableBeatmap(RulesetInfo ruleset)
        {
            var rulesetInstance = ruleset.CreateInstance();

            IBeatmapConverter converter = rulesetInstance.CreateBeatmapConverter(Beatmap);

            // Check if the beatmap can be converted
            if (!converter.CanConvert)
                throw new BeatmapInvalidForRulesetException($"{nameof(Beatmaps.Beatmap)} can not be converted for the ruleset (ruleset: {ruleset.InstantiationInfo}, converter: {converter}).");

            // Apply conversion mods
            foreach (var mod in Mods.Value.OfType<IApplicableToBeatmapConverter>())
                mod.ApplyToBeatmapConverter(converter);

            // Convert
            IBeatmap converted = converter.Convert();

            // Apply difficulty mods
            if (Mods.Value.Any(m => m is IApplicableToDifficulty))
            {
                converted.BeatmapInfo = converted.BeatmapInfo.Clone();
                converted.BeatmapInfo.BaseDifficulty = converted.BeatmapInfo.BaseDifficulty.Clone();

                foreach (var mod in Mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(converted.BeatmapInfo.BaseDifficulty);
            }

            IBeatmapProcessor processor = rulesetInstance.CreateBeatmapProcessor(converted);

            processor?.PreProcess();

            // Compute default values for hitobjects, including creating nested hitobjects in-case they're needed
            foreach (var obj in converted.HitObjects)
                obj.ApplyDefaults(converted.ControlPointInfo, converted.BeatmapInfo.BaseDifficulty);

            foreach (var mod in Mods.Value.OfType<IApplicableToHitObject>())
            foreach (var obj in converted.HitObjects)
                mod.ApplyToHitObject(obj);

            processor?.PostProcess();

            return converted;
        }

        public bool BackgroundLoaded => background.IsResultAvailable;
        public Texture Background => background.Value.Result;
        public async Task<Texture> GetBackgroundAsync() => await background.Value;
        private AsyncLazy<Texture> background;

        private Texture populateBackground() => GetBackground();

        public bool TrackLoaded => track.IsResultAvailable;
        public Track Track => track.Value.Result;
        public async Task<Track> GetTrackAsync() => await track.Value;
        private AsyncLazy<Track> track;

        private Track populateTrack()
        {
            // we want to ensure that we always have a track, even if it's a fake one.
            var t = GetTrack() ?? new VirtualBeatmapTrack(Beatmap);
            applyRateAdjustments(t);
            return t;
        }

        public bool WaveformLoaded => waveform.IsResultAvailable;
        public Waveform Waveform => waveform.Value.Result;
        public async Task<Waveform> GetWaveformAsync() => await waveform.Value;
        private readonly AsyncLazy<Waveform> waveform;

        private Waveform populateWaveform() => GetWaveform();

        public bool StoryboardLoaded => storyboard.IsResultAvailable;
        public Storyboard Storyboard => storyboard.Value.Result;
        public async Task<Storyboard> GetStoryboardAsync() => await storyboard.Value;
        private readonly AsyncLazy<Storyboard> storyboard;

        private Storyboard populateStoryboard() => GetStoryboard();

        public bool SkinLoaded => skin.IsResultAvailable;
        public Skin Skin => skin.Value.Result;
        public async Task<Skin> GetSkinAsync() => await skin.Value;
        private readonly AsyncLazy<Skin> skin;

        private Skin populateSkin() => GetSkin();

        public void TransferTo(WorkingBeatmap other)
        {
            if (track.IsResultAvailable && Track != null && BeatmapInfo.AudioEquals(other.BeatmapInfo))
                other.track = track;

            if (background.IsResultAvailable && Background != null && BeatmapInfo.BackgroundEquals(other.BeatmapInfo))
                other.background = background;
        }

        public virtual void Dispose()
        {
            if (BackgroundLoaded) Background?.Dispose();
            if (WaveformLoaded) Waveform?.Dispose();
            if (StoryboardLoaded) Storyboard?.Dispose();
            if (SkinLoaded) Skin?.Dispose();
        }

        /// <summary>
        /// Eagerly dispose of the audio track associated with this <see cref="WorkingBeatmap"/> (if any).
        /// Accessing track again will load a fresh instance.
        /// </summary>
        public void RecycleTrack() => track.Recycle();

        private void applyRateAdjustments(Track t = null)
        {
            if (t == null && track.IsResultAvailable) t = Track;
            if (t == null) return;

            t.ResetSpeedAdjustments();
            foreach (var mod in Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(t);
        }

        public class AsyncLazy<T>
        {
            private Lazy<Task<T>> lazy;
            private readonly Func<T> valueFactory;
            private readonly Func<T, bool> stillValidFunction;

            private readonly object initLock = new object();

            public AsyncLazy(Func<T> valueFactory, Func<T, bool> stillValidFunction = null)
            {
                this.valueFactory = valueFactory;
                this.stillValidFunction = stillValidFunction;

                recreate();
            }

            public void Recycle()
            {
                if (!IsResultAvailable) return;

                (lazy.Value.Result as IDisposable)?.Dispose();
                recreate();
            }

            public bool IsResultAvailable
            {
                get
                {
                    recreateIfInvalid();
                    return lazy.Value.IsCompleted;
                }
            }

            public Task<T> Value
            {
                get
                {
                    recreateIfInvalid();
                    return lazy.Value;
                }
            }

            private void recreateIfInvalid()
            {
                lock (initLock)
                {
                    if (!lazy.IsValueCreated || !lazy.Value.IsCompleted)
                        // we have not yet been initialised or haven't run the task.
                        return;

                    if (stillValidFunction?.Invoke(lazy.Value.Result) ?? true)
                        // we are still in a valid state.
                        return;

                    recreate();
                }
            }

            private void recreate() => lazy = new Lazy<Task<T>>(() => Task.Run(valueFactory));
        }
    }
}
