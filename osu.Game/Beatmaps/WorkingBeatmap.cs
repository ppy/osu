// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using osu.Game.Storyboards;
using osu.Framework.IO.File;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
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

            Mods.ValueChanged += _ => applyRateAdjustments();

            beatmap = new RecyclableLazy<IBeatmap>(() =>
            {
                var b = GetBeatmap() ?? new Beatmap();

                // The original beatmap version needs to be preserved as the database doesn't contain it
                BeatmapInfo.BeatmapVersion = b.BeatmapInfo.BeatmapVersion;

                // Use the database-backed info for more up-to-date values (beatmap id, ranked status, etc)
                b.BeatmapInfo = BeatmapInfo;

                return b;
            });

            track = new RecyclableLazy<Track>(() =>
            {
                // we want to ensure that we always have a track, even if it's a fake one.
                var t = GetTrack() ?? new VirtualBeatmapTrack(Beatmap);
                applyRateAdjustments(t);
                return t;
            });

            background = new RecyclableLazy<Texture>(GetBackground, BackgroundStillValid);
            waveform = new RecyclableLazy<Waveform>(GetWaveform);
            storyboard = new RecyclableLazy<Storyboard>(GetStoryboard);
            skin = new RecyclableLazy<Skin>(GetSkin);
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

        public override string ToString() => BeatmapInfo.ToString();

        public bool BeatmapLoaded => beatmap.IsResultAvailable;
        public IBeatmap Beatmap => beatmap.Value;
        protected abstract IBeatmap GetBeatmap();
        private readonly RecyclableLazy<IBeatmap> beatmap;

        public bool BackgroundLoaded => background.IsResultAvailable;
        public Texture Background => background.Value;
        protected virtual bool BackgroundStillValid(Texture b) => b == null || b.Available;
        protected abstract Texture GetBackground();
        private readonly RecyclableLazy<Texture> background;

        public bool TrackLoaded => track.IsResultAvailable;
        public Track Track => track.Value;
        protected abstract Track GetTrack();
        private RecyclableLazy<Track> track;

        public bool WaveformLoaded => waveform.IsResultAvailable;
        public Waveform Waveform => waveform.Value;
        protected virtual Waveform GetWaveform() => new Waveform(null);
        private readonly RecyclableLazy<Waveform> waveform;

        public bool StoryboardLoaded => storyboard.IsResultAvailable;
        public Storyboard Storyboard => storyboard.Value;
        protected virtual Storyboard GetStoryboard() => new Storyboard { BeatmapInfo = BeatmapInfo };
        private readonly RecyclableLazy<Storyboard> storyboard;

        public bool SkinLoaded => skin.IsResultAvailable;
        public Skin Skin => skin.Value;
        protected virtual Skin GetSkin() => new DefaultSkin();
        private readonly RecyclableLazy<Skin> skin;

        /// <summary>
        /// Transfer pieces of a beatmap to a new one, where possible, to save on loading.
        /// </summary>
        /// <param name="other">The new beatmap which is being switched to.</param>
        public virtual void TransferTo(WorkingBeatmap other)
        {
            if (track.IsResultAvailable && Track != null && BeatmapInfo.AudioEquals(other.BeatmapInfo))
                other.track = track;
        }

        public virtual void Dispose()
        {
            background.Recycle();
            waveform.Recycle();
            storyboard.Recycle();
            skin.Recycle();
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

        public class RecyclableLazy<T>
        {
            private Lazy<T> lazy;
            private readonly Func<T> valueFactory;
            private readonly Func<T, bool> stillValidFunction;

            private readonly object fetchLock = new object();

            public RecyclableLazy(Func<T> valueFactory, Func<T, bool> stillValidFunction = null)
            {
                this.valueFactory = valueFactory;
                this.stillValidFunction = stillValidFunction;

                recreate();
            }

            public void Recycle()
            {
                if (!IsResultAvailable) return;

                (lazy.Value as IDisposable)?.Dispose();
                recreate();
            }

            public bool IsResultAvailable => stillValid;

            public T Value
            {
                get
                {
                    lock (fetchLock)
                    {
                        if (!stillValid)
                            recreate();
                        return lazy.Value;
                    }
                }
            }

            private bool stillValid => lazy.IsValueCreated && (stillValidFunction?.Invoke(lazy.Value) ?? true);

            private void recreate() => lazy = new Lazy<T>(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
