// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ManagedBass;
using ManagedBass.Fx;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    public abstract class WorkingBeatmap : IWorkingBeatmap
    {
        public readonly BeatmapInfo BeatmapInfo;
        public readonly BeatmapSetInfo BeatmapSetInfo;

        // TODO: remove once the fallback lookup is not required (and access via `working.BeatmapInfo.Metadata` directly).
        public BeatmapMetadata Metadata => BeatmapInfo.Metadata;

        public Storyboard Storyboard => storyboard.Value;

        public ISkin Skin => skin.Value;

        private AudioManager audioManager { get; }

        private CancellationTokenSource loadCancellationSource = new CancellationTokenSource();

        private readonly object beatmapFetchLock = new object();

        private readonly Lazy<Storyboard> storyboard;
        private readonly Lazy<ISkin> skin;

        private Track track; // track is not Lazy as we allow transferring and loading multiple times.
        private Waveform waveform; // waveform is also not Lazy as the track may change.

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, AudioManager audioManager)
        {
            this.audioManager = audioManager;

            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapInfo.BeatmapSet ?? new BeatmapSetInfo();

            storyboard = new Lazy<Storyboard>(GetStoryboard);
            skin = new Lazy<ISkin>(GetSkin);
        }

        #region Resource getters

        protected virtual Waveform GetWaveform() => new Waveform(null);
        protected virtual Storyboard GetStoryboard() => new Storyboard { BeatmapInfo = BeatmapInfo };

        protected abstract IBeatmap GetBeatmap();
        public abstract Texture GetBackground();
        public virtual Texture GetPanelBackground() => GetBackground();
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

        public Track LoadTrack()
        {
            track = GetBeatmapTrack() ?? GetVirtualTrack(1000);

            // the track may have changed, recycle the current waveform.
            waveform?.Dispose();
            waveform = null;

            addAudioNormalization();
            Logger.Log("Added normalization");

            return track;
        }

        public void PrepareTrackForPreview(bool looping, double offsetFromPreviewPoint = 0)
        {
            Track.Looping = looping;
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

            Track.RestartPoint += offsetFromPreviewPoint;
        }

        /// <summary>
        /// Attempts to transfer the audio track to a target working beatmap, if valid for transferring.
        /// Used as an optimisation to avoid reload / track swap across difficulties in the same beatmap set.
        /// </summary>
        /// <param name="target">The target working beatmap to transfer this track to.</param>
        /// <returns>Whether the track has been transferred to the <paramref name="target"/>.</returns>
        public virtual bool TryTransferTrack([NotNull] WorkingBeatmap target)
        {
            if (BeatmapInfo?.AudioEquals(target.BeatmapInfo) != true || Track.IsDummyDevice)
                return false;

            target.track = Track;
            return true;
        }

        /// <summary>
        /// Get the loaded audio track instance. <see cref="LoadTrack"/> must have first been called.
        /// This generally happens via MusicController when changing the global beatmap.
        /// </summary>
        [NotNull]
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

            double length = (BeatmapInfo?.Length + excess_length) ?? emptyLength;

            return audioManager.Tracks.GetVirtual(length);
        }

        #endregion

        #region Waveform

        public Waveform Waveform => waveform ??= GetWaveform();

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

            var processor = rulesetInstance.CreateBeatmapProcessor(converted);

            if (processor != null)
            {
                foreach (var mod in mods.OfType<IApplicableToBeatmapProcessor>())
                    mod.ApplyToBeatmapProcessor(processor);

                processor.PreProcess();
            }

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

        #region Audio Normalization

        private void addAudioNormalization()
        {
            AudioNormalization audioNormalizationModule = BeatmapInfo.AudioNormalization;

            Logger.Log("Audio Normalization Manager Null Status: " + audioNormalizationModule.IsNull());

            VolumeParameters volumeParameters = new VolumeParameters
            {
                fTarget = audioNormalizationModule?.VolumeOffset ?? 0.8f,
                fCurrent = 1.0f,
                fTime = 0,
                lCurve = 0,
                lChannel = FXChannelFlags.All
            };

            addFx(volumeParameters);
        }

        private void addFx(IEffectParameter effectParameter)
        {
            AudioMixer audioMixer = audioManager.TrackMixer;

            IEffectParameter effect = audioMixer.Effects.SingleOrDefault(e => e.FXType == effectParameter.FXType)!;

            if (effect != null)
            {
                int i = audioMixer.Effects.IndexOf(effect);
                audioMixer.Effects[i] = effectParameter;
            }
            else
            {
                audioMixer.Effects.Add(effectParameter);
            }
        }

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
