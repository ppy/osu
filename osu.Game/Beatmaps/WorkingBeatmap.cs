﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using osu.Game.Storyboards;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Audio;
using osu.Framework.Statistics;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Framework.Graphics.Video;

namespace osu.Game.Beatmaps
{
    public abstract class WorkingBeatmap : IWorkingBeatmap, IDisposable
    {
        public readonly BeatmapInfo BeatmapInfo;

        public readonly BeatmapSetInfo BeatmapSetInfo;

        public readonly BeatmapMetadata Metadata;

        protected AudioManager AudioManager { get; }

        private static readonly GlobalStatistic<int> total_count = GlobalStatistics.Get<int>(nameof(Beatmaps), $"Total {nameof(WorkingBeatmap)}s");

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, AudioManager audioManager)
        {
            AudioManager = audioManager;
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapInfo.BeatmapSet;
            Metadata = beatmapInfo.Metadata ?? BeatmapSetInfo?.Metadata ?? new BeatmapMetadata();

            track = new RecyclableLazy<Track>(() => GetTrack() ?? GetVirtualTrack());
            background = new RecyclableLazy<Texture>(GetBackground, BackgroundStillValid);
            waveform = new RecyclableLazy<Waveform>(GetWaveform);
            storyboard = new RecyclableLazy<Storyboard>(GetStoryboard);
            skin = new RecyclableLazy<ISkin>(GetSkin);

            total_count.Value++;
        }

        protected virtual Track GetVirtualTrack()
        {
            const double excess_length = 1000;

            var lastObject = Beatmap.HitObjects.LastOrDefault();

            double length;

            switch (lastObject)
            {
                case null:
                    length = excess_length;
                    break;

                case IHasEndTime endTime:
                    length = endTime.EndTime + excess_length;
                    break;

                default:
                    length = lastObject.StartTime + excess_length;
                    break;
            }

            return AudioManager.Tracks.GetVirtual(length);
        }

        /// <summary>
        /// Saves the <see cref="Beatmaps.Beatmap"/>.
        /// </summary>
        /// <returns>The absolute path of the output file.</returns>
        public string Save()
        {
            string directory = Path.Combine(Path.GetTempPath(), @"osu!");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, Guid.NewGuid().ToString().Replace("-", string.Empty) + ".json");
            using (var sw = new StreamWriter(path))
                sw.WriteLine(Beatmap.Serialize());
            return path;
        }

        /// <summary>
        /// Creates a <see cref="IBeatmapConverter"/> to convert a <see cref="IBeatmap"/> for a specified <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be converted.</param>
        /// <param name="ruleset">The <see cref="Ruleset"/> for which <paramref name="beatmap"/> should be converted.</param>
        /// <returns>The applicable <see cref="IBeatmapConverter"/>.</returns>
        protected virtual IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap, Ruleset ruleset) => ruleset.CreateBeatmapConverter(beatmap);

        public IBeatmap GetPlayableBeatmap(RulesetInfo ruleset, IReadOnlyList<Mod> mods = null)
        {
            mods ??= Array.Empty<Mod>();

            var rulesetInstance = ruleset.CreateInstance();

            IBeatmapConverter converter = CreateBeatmapConverter(Beatmap, rulesetInstance);

            // Check if the beatmap can be converted
            if (Beatmap.HitObjects.Count > 0 && !converter.CanConvert())
                throw new BeatmapInvalidForRulesetException($"{nameof(Beatmaps.Beatmap)} can not be converted for the ruleset (ruleset: {ruleset.InstantiationInfo}, converter: {converter}).");

            // Apply conversion mods
            foreach (var mod in mods.OfType<IApplicableToBeatmapConverter>())
                mod.ApplyToBeatmapConverter(converter);

            // Convert
            IBeatmap converted = converter.Convert();

            // Apply difficulty mods
            if (mods.Any(m => m is IApplicableToDifficulty))
            {
                converted.BeatmapInfo = converted.BeatmapInfo.Clone();
                converted.BeatmapInfo.BaseDifficulty = converted.BeatmapInfo.BaseDifficulty.Clone();

                foreach (var mod in mods.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(converted.BeatmapInfo.BaseDifficulty);
            }

            IBeatmapProcessor processor = rulesetInstance.CreateBeatmapProcessor(converted);

            processor?.PreProcess();

            // Compute default values for hitobjects, including creating nested hitobjects in-case they're needed
            foreach (var obj in converted.HitObjects)
                obj.ApplyDefaults(converted.ControlPointInfo, converted.BeatmapInfo.BaseDifficulty);

            foreach (var mod in mods.OfType<IApplicableToHitObject>())
            {
                foreach (var obj in converted.HitObjects)
                    mod.ApplyToHitObject(obj);
            }

            processor?.PostProcess();

            foreach (var mod in mods.OfType<IApplicableToBeatmap>())
                mod.ApplyToBeatmap(converted);

            return converted;
        }

        public override string ToString() => BeatmapInfo.ToString();

        public bool BeatmapLoaded => beatmapLoadTask?.IsCompleted ?? false;

        public Task<IBeatmap> LoadBeatmapAsync() => beatmapLoadTask ??= Task.Factory.StartNew(() =>
        {
            // Todo: Handle cancellation during beatmap parsing
            var b = GetBeatmap() ?? new Beatmap();

            // The original beatmap version needs to be preserved as the database doesn't contain it
            BeatmapInfo.BeatmapVersion = b.BeatmapInfo.BeatmapVersion;

            // Use the database-backed info for more up-to-date values (beatmap id, ranked status, etc)
            b.BeatmapInfo = BeatmapInfo;

            return b;
        }, beatmapCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        public IBeatmap Beatmap
        {
            get
            {
                try
                {
                    return LoadBeatmapAsync().Result;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
            }
        }

        private readonly CancellationTokenSource beatmapCancellation = new CancellationTokenSource();
        protected abstract IBeatmap GetBeatmap();
        private Task<IBeatmap> beatmapLoadTask;

        public bool BackgroundLoaded => background.IsResultAvailable;
        public Texture Background => background.Value;
        protected virtual bool BackgroundStillValid(Texture b) => b == null || b.Available;
        protected abstract Texture GetBackground();
        private readonly RecyclableLazy<Texture> background;

        public VideoSprite Video => GetVideo();

        protected abstract VideoSprite GetVideo();

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
        public ISkin Skin => skin.Value;

        protected virtual ISkin GetSkin() => new DefaultSkin();
        private readonly RecyclableLazy<ISkin> skin;

        /// <summary>
        /// Transfer pieces of a beatmap to a new one, where possible, to save on loading.
        /// </summary>
        /// <param name="other">The new beatmap which is being switched to.</param>
        public virtual void TransferTo(WorkingBeatmap other)
        {
            if (track.IsResultAvailable && Track != null && BeatmapInfo.AudioEquals(other.BeatmapInfo))
                other.track = track;
        }

        /// <summary>
        /// Eagerly dispose of the audio track associated with this <see cref="WorkingBeatmap"/> (if any).
        /// Accessing track again will load a fresh instance.
        /// </summary>
        public virtual void RecycleTrack() => track.Recycle();

        #region Disposal

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            // recycling logic is not here for the time being, as components which use
            // retrieved objects from WorkingBeatmap may not hold a reference to the WorkingBeatmap itself.
            // this should be fine as each retrieved component do have their own finalizers.

            // cancelling the beatmap load is safe for now since the retrieval is a synchronous
            // operation. if we add an async retrieval method this may need to be reconsidered.
            beatmapCancellation?.Cancel();
            total_count.Value--;
        }

        ~WorkingBeatmap()
        {
            Dispose(false);
        }

        #endregion

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
