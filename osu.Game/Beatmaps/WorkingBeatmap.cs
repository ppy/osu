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
using osu.Framework.Extensions;
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

        // TODO: remove once the fallback lookup is not required (and access via `working.BeatmapInfo.Metadata` directly).
        public BeatmapMetadata Metadata => BeatmapInfo.Metadata;

        public Waveform Waveform => waveform.Value;

        public Storyboard Storyboard => storyboard.Value;

        public Texture Background => GetBackground(); // Texture uses ref counting, so we want to return a new instance every usage.

        public ISkin Skin => skin.Value;

        private AudioManager audioManager { get; }

        private CancellationTokenSource loadCancellationSource = new CancellationTokenSource();

        private readonly object beatmapFetchLock = new object();

        private readonly Lazy<Waveform> waveform;
        private readonly Lazy<Storyboard> storyboard;
        private readonly Lazy<ISkin> skin;
        private Track track; // track is not Lazy as we allow transferring and loading multiple times.

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, AudioManager audioManager)
        {
            this.audioManager = audioManager;

            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapInfo.BeatmapSet ?? new BeatmapSetInfo();

            waveform = new Lazy<Waveform>(GetWaveform);
            storyboard = new Lazy<Storyboard>(GetStoryboard);
            skin = new Lazy<ISkin>(GetSkin);
        }

        #region Resource getters

        protected virtual Waveform GetWaveform() => new Waveform(null);
        protected virtual Storyboard GetStoryboard() => new Storyboard { BeatmapInfo = BeatmapInfo };

        protected abstract IBeatmap GetBeatmap();
        protected abstract Texture GetBackground();
        protected abstract Track GetBeatmapTrack();

        /// <summary>
        /// Creates a new skin instance for this beatmap.
        /// </summary>
        /// <remarks>
        /// This should only be called externally in scenarios where it is explicitly desired to get a new instance of a skin
        /// (e.g. for editing purposes, to avoid state pollution).
        /// For standard reading purposes, <see cref="Skin"/> should always be used directly.
        /// </remarks>
        protected internal abstract ISkin GetSkin();

        #endregion

        #region Async load control

        public void BeginAsyncLoad() => loadBeatmapAsync();

        public void CancelAsyncLoad()
        {
            lock (beatmapFetchLock)
            {
                loadCancellationSource?.Cancel();
                loadCancellationSource = new CancellationTokenSource();

                if (beatmapLoadTask?.IsCompleted != true)
                    beatmapLoadTask = null;
            }
        }

        #endregion

        #region Track

        public virtual bool TrackLoaded => track != null;

        public Track LoadTrack() => track = GetBeatmapTrack() ?? GetVirtualTrack(1000);

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
        public void TransferTrack([NotNull] Track track) => this.track = track ?? throw new ArgumentNullException(nameof(track));

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

                return track;
            }
        }

        protected Track GetVirtualTrack(double emptyLength = 0)
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

            return audioManager.Tracks.GetVirtual(length);
        }

        #endregion

        #region Beatmap

        public virtual bool BeatmapLoaded => beatmapLoadTask?.IsCompleted ?? false;

        public IBeatmap Beatmap
        {
            get
            {
                try
                {
                    return loadBeatmapAsync().GetResultSafely();
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

        private Task<IBeatmap> beatmapLoadTask;

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
                }, loadCancellationSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        #endregion

        #region Playable beatmap

        public IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods = null)
        {
            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource(10_000))
                {
                    // don't apply the default timeout when debugger is attached (may be breakpointing / debugging).
                    return GetPlayableBeatmap(ruleset, mods ?? Array.Empty<Mod>(), Debugger.IsAttached ? new CancellationToken() : cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                throw new BeatmapLoadTimeoutException(BeatmapInfo);
            }
        }

        public virtual IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods, CancellationToken token)
        {
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
                token.ThrowIfCancellationRequested();
                mod.ApplyToBeatmapConverter(converter);
            }

            // Convert
            IBeatmap converted = converter.Convert(token);

            // Apply conversion mods to the result
            foreach (var mod in mods.OfType<IApplicableAfterBeatmapConversion>())
            {
                token.ThrowIfCancellationRequested();
                mod.ApplyToBeatmap(converted);
            }

            // Apply difficulty mods
            if (mods.Any(m => m is IApplicableToDifficulty))
            {
                foreach (var mod in mods.OfType<IApplicableToDifficulty>())
                {
                    token.ThrowIfCancellationRequested();
                    mod.ApplyToDifficulty(converted.Difficulty);
                }
            }

            IBeatmapProcessor processor = rulesetInstance.CreateBeatmapProcessor(converted);

            foreach (var mod in mods.OfType<IApplicableToBeatmapProcessor>())
                mod.ApplyToBeatmapProcessor(processor);

            processor?.PreProcess();

            // Compute default values for hitobjects, including creating nested hitobjects in-case they're needed
            foreach (var obj in converted.HitObjects)
            {
                token.ThrowIfCancellationRequested();
                obj.ApplyDefaults(converted.ControlPointInfo, converted.Difficulty, token);
            }

            foreach (var mod in mods.OfType<IApplicableToHitObject>())
            {
                foreach (var obj in converted.HitObjects)
                {
                    token.ThrowIfCancellationRequested();
                    mod.ApplyToHitObject(obj);
                }
            }

            processor?.PostProcess();

            foreach (var mod in mods.OfType<IApplicableToBeatmap>())
            {
                token.ThrowIfCancellationRequested();
                mod.ApplyToBeatmap(converted);
            }

            return converted;
        }

        /// <summary>
        /// Creates a <see cref="IBeatmapConverter"/> to convert a <see cref="IBeatmap"/> for a specified <see cref="Ruleset"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to be converted.</param>
        /// <param name="ruleset">The <see cref="Ruleset"/> for which <paramref name="beatmap"/> should be converted.</param>
        /// <returns>The applicable <see cref="IBeatmapConverter"/>.</returns>
        protected virtual IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap, Ruleset ruleset) => ruleset.CreateBeatmapConverter(beatmap);

        #endregion

        public override string ToString() => BeatmapInfo.ToString();

        public abstract Stream GetStream(string storagePath);

        IBeatmapInfo IWorkingBeatmap.BeatmapInfo => BeatmapInfo;

        private class BeatmapLoadTimeoutException : TimeoutException
        {
            public BeatmapLoadTimeoutException(BeatmapInfo beatmapInfo)
                : base($"Timed out while loading beatmap ({beatmapInfo}).")
            {
            }
        }
    }
}
