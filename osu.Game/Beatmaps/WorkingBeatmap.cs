// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    public abstract class WorkingBeatmap : IWorkingBeatmap
    {
        public readonly BeatmapInfo BeatmapInfo;
        public readonly BeatmapSetInfo BeatmapSetInfo;
        public readonly BeatmapMetadata Metadata;

        protected AudioManager AudioManager { get; }

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, AudioManager audioManager)
        {
            AudioManager = audioManager;
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapInfo.BeatmapSet;
            Metadata = beatmapInfo.Metadata ?? BeatmapSetInfo?.Metadata ?? new BeatmapMetadata();

            background = new RecyclableLazy<Texture>(GetBackground, BackgroundStillValid);
            waveform = new RecyclableLazy<Waveform>(GetWaveform);
            storyboard = new RecyclableLazy<Storyboard>(GetStoryboard);
            skin = new RecyclableLazy<ISkin>(GetSkin);
        }

        protected virtual Track GetVirtualTrack(double emptyLength = 0)
        {
            const double excess_length = 1000;

            var lastObject = Beatmap?.HitObjects.LastOrDefault();

            double length;

            switch (lastObject)
            {
                case null:
                    length = emptyLength;
                    break;

                case IHasDuration endTime:
                    length = endTime.EndTime + excess_length;
                    break;

                default:
                    length = lastObject.StartTime + excess_length;
                    break;
            }

            return AudioManager.Tracks.GetVirtual(length);
        }

        /// <summary>
        /// Creates a <see cref="IBeatmapConverter"/> to convert a <see cref="IBeatmap"/> for a specified <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be converted.</param>
        /// <param name="ruleset">The <see cref="Ruleset"/> for which <paramref name="beatmap"/> should be converted.</param>
        /// <returns>The applicable <see cref="IBeatmapConverter"/>.</returns>
        protected virtual IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap, Ruleset ruleset) => ruleset.CreateBeatmapConverter(beatmap);

        public virtual IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods = null, TimeSpan? timeout = null)
        {
            using (var cancellationSource = createCancellationTokenSource(timeout))
            {
                mods ??= Array.Empty<Mod>();

                var rulesetInstance = ruleset.CreateInstance();

                if (rulesetInstance == null)
                    throw new RulesetLoadException("Creating ruleset instance failed when attempting to create playable beatmap.");

                IBeatmapConverter converter = CreateBeatmapConverter(Beatmap, rulesetInstance);

                // Check if the beatmap can be converted
                if (Beatmap.HitObjects.Count > 0 && !converter.CanConvert())
                    throw new BeatmapInvalidForRulesetException($"{nameof(Beatmaps.Beatmap)} can not be converted for the ruleset (ruleset: {ruleset.InstantiationInfo}, converter: {converter}).");

                // Apply conversion mods
                foreach (var mod in mods.OfType<IApplicableToBeatmapConverter>())
                {
                    if (cancellationSource.IsCancellationRequested)
                        throw new BeatmapLoadTimeoutException(BeatmapInfo);

                    mod.ApplyToBeatmapConverter(converter);
                }

                // Convert
                IBeatmap converted = converter.Convert(cancellationSource.Token);

                // Apply conversion mods to the result
                foreach (var mod in mods.OfType<IApplicableAfterBeatmapConversion>())
                {
                    if (cancellationSource.IsCancellationRequested)
                        throw new BeatmapLoadTimeoutException(BeatmapInfo);

                    mod.ApplyToBeatmap(converted);
                }

                // Apply difficulty mods
                if (mods.Any(m => m is IApplicableToDifficulty))
                {
                    foreach (var mod in mods.OfType<IApplicableToDifficulty>())
                    {
                        if (cancellationSource.IsCancellationRequested)
                            throw new BeatmapLoadTimeoutException(BeatmapInfo);

                        mod.ApplyToDifficulty(converted.Difficulty);
                    }
                }

                IBeatmapProcessor processor = rulesetInstance.CreateBeatmapProcessor(converted);

                foreach (var mod in mods.OfType<IApplicableToBeatmapProcessor>())
                    mod.ApplyToBeatmapProcessor(processor);

                processor?.PreProcess();

                // Compute default values for hitobjects, including creating nested hitobjects in-case they're needed
                try
                {
                    foreach (var obj in converted.HitObjects)
                    {
                        if (cancellationSource.IsCancellationRequested)
                            throw new BeatmapLoadTimeoutException(BeatmapInfo);

                        obj.ApplyDefaults(converted.ControlPointInfo, converted.Difficulty, cancellationSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw new BeatmapLoadTimeoutException(BeatmapInfo);
                }

                foreach (var mod in mods.OfType<IApplicableToHitObject>())
                {
                    foreach (var obj in converted.HitObjects)
                    {
                        if (cancellationSource.IsCancellationRequested)
                            throw new BeatmapLoadTimeoutException(BeatmapInfo);

                        mod.ApplyToHitObject(obj);
                    }
                }

                processor?.PostProcess();

                foreach (var mod in mods.OfType<IApplicableToBeatmap>())
                {
                    cancellationSource.Token.ThrowIfCancellationRequested();
                    mod.ApplyToBeatmap(converted);
                }

                return converted;
            }
        }

        private CancellationTokenSource loadCancellation = new CancellationTokenSource();

        public void BeginAsyncLoad() => loadBeatmapAsync();

        public void CancelAsyncLoad()
        {
            lock (beatmapFetchLock)
            {
                loadCancellation?.Cancel();
                loadCancellation = new CancellationTokenSource();

                if (beatmapLoadTask?.IsCompleted != true)
                    beatmapLoadTask = null;
            }
        }

        private CancellationTokenSource createCancellationTokenSource(TimeSpan? timeout)
        {
            if (Debugger.IsAttached)
                // ignore timeout when debugger is attached (may be breakpointing / debugging).
                return new CancellationTokenSource();

            return new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(10));
        }

        private readonly object beatmapFetchLock = new object();

        private Task<IBeatmap> loadBeatmapAsync()
        {
            lock (beatmapFetchLock)
            {
                return beatmapLoadTask ??= Task.Factory.StartNew(() =>
                {
                    // Todo: Handle cancellation during beatmap parsing
                    var b = GetBeatmap() ?? new Beatmap();

                    // The original beatmap version needs to be preserved as the database doesn't contain it
                    BeatmapInfo.BeatmapVersion = b.BeatmapInfo.BeatmapVersion;

                    // Use the database-backed info for more up-to-date values (beatmap id, ranked status, etc)
                    b.BeatmapInfo = BeatmapInfo;

                    return b;
                }, loadCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        public override string ToString() => BeatmapInfo.ToString();

        public virtual bool BeatmapLoaded => beatmapLoadTask?.IsCompleted ?? false;

        IBeatmapInfo IWorkingBeatmap.BeatmapInfo => BeatmapInfo;
        IBeatmapMetadataInfo IWorkingBeatmap.Metadata => Metadata;
        IBeatmapSetInfo IWorkingBeatmap.BeatmapSetInfo => BeatmapSetInfo;

        public IBeatmap Beatmap
        {
            get
            {
                try
                {
                    return loadBeatmapAsync().Result;
                }
                catch (AggregateException ae)
                {
                    // This is the exception that is generally expected here, which occurs via natural cancellation of the asynchronous load
                    if (ae.InnerExceptions.FirstOrDefault() is TaskCanceledException)
                        return null;

                    Logger.Error(ae, "Beatmap failed to load");
                    return null;
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Beatmap failed to load");
                    return null;
                }
            }
        }

        protected abstract IBeatmap GetBeatmap();
        private Task<IBeatmap> beatmapLoadTask;

        public bool BackgroundLoaded => background.IsResultAvailable;
        public Texture Background => background.Value;
        protected virtual bool BackgroundStillValid(Texture b) => b == null || b.Available;
        protected abstract Texture GetBackground();
        private readonly RecyclableLazy<Texture> background;

        private Track loadedTrack;

        [NotNull]
        public Track LoadTrack() => loadedTrack = GetBeatmapTrack() ?? GetVirtualTrack(1000);

        public void PrepareTrackForPreviewLooping()
        {
            Track.Looping = true;
            Track.RestartPoint = Metadata.PreviewTime;

            if (Track.RestartPoint == -1)
            {
                if (!Track.IsLoaded)
                {
                    // force length to be populated (https://github.com/ppy/osu-framework/issues/4202)
                    Track.Seek(Track.CurrentTime);
                }

                Track.RestartPoint = 0.4f * Track.Length;
            }
        }

        /// <summary>
        /// Transfer a valid audio track into this working beatmap. Used as an optimisation to avoid reload / track swap
        /// across difficulties in the same beatmap set.
        /// </summary>
        /// <param name="track">The track to transfer.</param>
        public void TransferTrack([NotNull] Track track) => loadedTrack = track ?? throw new ArgumentNullException(nameof(track));

        /// <summary>
        /// Whether this beatmap's track has been loaded via <see cref="LoadTrack"/>.
        /// </summary>
        public virtual bool TrackLoaded => loadedTrack != null;

        /// <summary>
        /// Get the loaded audio track instance. <see cref="LoadTrack"/> must have first been called.
        /// This generally happens via MusicController when changing the global beatmap.
        /// </summary>
        public Track Track
        {
            get
            {
                if (!TrackLoaded)
                    throw new InvalidOperationException($"Cannot access {nameof(Track)} without first calling {nameof(LoadTrack)}.");

                return loadedTrack;
            }
        }

        protected abstract Track GetBeatmapTrack();

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

        /// <summary>
        /// Creates a new skin instance for this beatmap.
        /// </summary>
        /// <remarks>
        /// This should only be called externally in scenarios where it is explicitly desired to get a new instance of a skin
        /// (e.g. for editing purposes, to avoid state pollution).
        /// For standard reading purposes, <see cref="Skin"/> should always be used directly.
        /// </remarks>
        protected internal abstract ISkin GetSkin();

        private readonly RecyclableLazy<ISkin> skin;

        public abstract Stream GetStream(string storagePath);

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

        private class BeatmapLoadTimeoutException : TimeoutException
        {
            public BeatmapLoadTimeoutException(BeatmapInfo beatmapInfo)
                : base($"Timed out while loading beatmap ({beatmapInfo}).")
            {
            }
        }
    }
}
