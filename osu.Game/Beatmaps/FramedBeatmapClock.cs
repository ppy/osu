// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Screens.Play;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A clock intended to be the single source-of-truth for beatmap timing.
    ///
    /// It provides some functionality:
    ///  - Optionally applies (and tracks changes of) online, beatmap, user, and platform offsets (see ctor argument applyOffsets).
    ///  - Adjusts <see cref="Seek"/> operations to account for any applied offsets, seeking in raw "beatmap" time values.
    ///  - Exposes track length.
    ///  - Allows changing the source to a new track (for cases like editor track updating).
    /// </summary>
    public partial class FramedBeatmapClock : Component, IFrameBasedClock, IAdjustableClock, ISourceChangeableClock
    {
        private readonly bool applyOffsets;

        private readonly OffsetCorrectionClock? platformOffsetClock;
        private readonly OffsetCorrectionClock? onlineOffsetClock;
        private readonly OffsetCorrectionClock? userGlobalOffsetClock;
        private readonly FramedOffsetClock? userBeatmapOffsetClock;

        private readonly IFrameBasedClock finalClockSource;

        private Bindable<double>? userAudioOffset;

        private IDisposable? beatmapOffsetSubscription;

        private readonly DecouplingFramedClock decoupledTrack;
        private readonly InterpolatingFramedClock interpolatedTrack;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        private Bindable<bool> experimentalAudio = null!;

        /// <summary>
        /// Store seek target for any <see cref="Seek"/> performed before <see cref="LoadState.Loaded"/>.
        /// </summary>
        /// <remarks>
        /// Initial seeks must be delayed until the <see cref="FramedBeatmapClock"/> instance is fully loaded.
        ///
        /// If not, <see cref="TotalAppliedOffset"/> will not be in a correct state and result in a potentially
        /// incorrect seek target.</remarks>
        private double? initialSeek;

        public bool IsRewinding { get; private set; }

        public FramedBeatmapClock(bool applyOffsets, bool requireDecoupling, IClock? source = null)
        {
            this.applyOffsets = applyOffsets;

            decoupledTrack = new DecouplingFramedClock(source) { AllowDecoupling = requireDecoupling };

            // An interpolating clock is used to ensure precise time values even when the host audio subsystem is not reporting
            // high precision times (on windows there's generally only 5-10ms reporting intervals, as an example).
            interpolatedTrack = new InterpolatingFramedClock(decoupledTrack)
            {
                DriftRecoveryHalfLife = 80,
            };

            if (applyOffsets)
            {
                platformOffsetClock = new OffsetCorrectionClock(interpolatedTrack);

                onlineOffsetClock = new OffsetCorrectionClock(platformOffsetClock);

                // User global offset (set in settings) should also be applied.
                userGlobalOffsetClock = new OffsetCorrectionClock(onlineOffsetClock);

                // User per-beatmap offset will be applied to this final clock.
                finalClockSource = userBeatmapOffsetClock = new FramedOffsetClock(userGlobalOffsetClock);
            }
            else
            {
                finalClockSource = interpolatedTrack;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (applyOffsets)
            {
                Debug.Assert(userBeatmapOffsetClock != null);
                Debug.Assert(onlineOffsetClock != null);
                Debug.Assert(userGlobalOffsetClock != null);

                userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
                userAudioOffset.BindValueChanged(offset => userGlobalOffsetClock.Offset = offset.NewValue, true);

                experimentalAudio = audioManager.UseExperimentalWasapi.GetBoundCopy();
                experimentalAudio.BindValueChanged(_ => updatePlatformOffset(), true);

                // TODO: this doesn't update when using ChangeSource() to change beatmap.
                beatmapOffsetSubscription = realm.RegisterForNotifications(
                    r => r.All<BeatmapInfo>().Where(b => b.ID == beatmap.Value.BeatmapInfo.ID),
                    (result, _) =>
                    {
                        var beatmapInfo = result.SingleOrDefault();

                        // for reasoning behind minus sign, see https://github.com/peppy/osu-stable-reference/blob/fa1197b286f491fbb80653332234675acd705b67/osu!/Audio/AudioEngine.cs#L825-L836
                        // the minus sign isn't applied to the user offset because in that case directionality is arbitrary,
                        // doesn't need to match across clients, and changing it would break existing user settings
                        onlineOffsetClock.Offset = -beatmapInfo?.BeatmapSet?.OnlineOffset ?? 0;

                        userBeatmapOffsetClock.Offset = beatmapInfo?.UserSettings.Offset ?? 0;
                    });
            }

            if (initialSeek != null)
            {
                Seek(initialSeek.Value);
                initialSeek = null;
            }
        }

        /// <summary>
        /// Audio timings in general with newer BASS versions don't match stable.
        /// This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
        /// </summary>
        public const double WINDOWS_BASE_AUDIO_OFFSET = 15;

        /// <summary>
        /// An additional offset applied to account for experimental mode being much better.
        /// </summary>
        public const double WINDOWS_EXPERIMENTAL_AUDIO_OFFSET = -25;

        private void updatePlatformOffset()
        {
            if (!applyOffsets)
                return;

            Debug.Assert(platformOffsetClock != null);

            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    platformOffsetClock.Offset = WINDOWS_BASE_AUDIO_OFFSET;

                    if (audioManager.UseExperimentalWasapi.Value)
                        platformOffsetClock.Offset += WINDOWS_EXPERIMENTAL_AUDIO_OFFSET;
                    return;

                default:
                    platformOffsetClock.Offset = 0;
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            finalClockSource.ProcessFrame();

            if (Clock.ElapsedFrameTime != 0)
                IsRewinding = Clock.ElapsedFrameTime < 0;
        }

        public double TotalAppliedOffset
        {
            get
            {
                if (!applyOffsets)
                    return 0;

                Debug.Assert(userGlobalOffsetClock != null);
                Debug.Assert(userBeatmapOffsetClock != null);
                Debug.Assert(onlineOffsetClock != null);
                Debug.Assert(platformOffsetClock != null);

                return userGlobalOffsetClock.RateAdjustedOffset + userBeatmapOffsetClock.Offset + onlineOffsetClock.RateAdjustedOffset + platformOffsetClock.RateAdjustedOffset;
            }
        }

        #region Delegation of IAdjustableClock / ISourceChangeableClock to decoupled clock.

        public void ChangeSource(IClock? source) => decoupledTrack.ChangeSource(source);

        public IClock Source => decoupledTrack.Source;

        public void Reset()
        {
            decoupledTrack.Reset();
            finalClockSource.ProcessFrame();
        }

        public void Start()
        {
            decoupledTrack.Start();
            finalClockSource.ProcessFrame();
        }

        public void Stop()
        {
            decoupledTrack.Stop();
            finalClockSource.ProcessFrame();
        }

        public bool Seek(double position)
        {
            // TotalAppliedOffset will not be correct until fully loaded.
            if (applyOffsets && !IsLoaded)
            {
                initialSeek = position;
                return true;
            }

            bool success = decoupledTrack.Seek(position - TotalAppliedOffset);
            finalClockSource.ProcessFrame();

            return success;
        }

        public void ResetSpeedAdjustments() => decoupledTrack.ResetSpeedAdjustments();

        public double Rate
        {
            get => decoupledTrack.Rate;
            set => decoupledTrack.Rate = value;
        }

        #endregion

        #region Delegation of IFrameBasedClock to clock with all offsets applied

        public double CurrentTime => initialSeek ?? finalClockSource.CurrentTime;

        public bool IsRunning => finalClockSource.IsRunning;

        public void ProcessFrame()
        {
            // Noop to ensure an external consumer doesn't process the internal clock an extra time.
        }

        public double ElapsedFrameTime => finalClockSource.ElapsedFrameTime;

        public double FramesPerSecond => finalClockSource.FramesPerSecond;

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
        }

        public string GetSnapshot()
        {
            return
                $"originalSource: {output(Source)}\n" +
                $"platformOffsetClock: {output(platformOffsetClock)}\n" +
                $"onlineOffsetClock: {output(onlineOffsetClock)}\n" +
                $"userGlobalOffsetClock: {output(userGlobalOffsetClock)}\n" +
                $"userBeatmapOffsetClock: {output(userBeatmapOffsetClock)}\n" +
                $"interpolatedTrack: {output(interpolatedTrack)}\n" +
                $"decoupledTrack: {output(decoupledTrack)}\n" +
                $"finalClockSource: {output(finalClockSource)}\n";

            string output(IClock? clock)
            {
                if (clock == null)
                    return "null";

                if (clock is IFrameBasedClock framed)
                    return $"current: {clock.CurrentTime:N2} running: {clock.IsRunning} rate: {clock.Rate} elapsed: {framed.ElapsedFrameTime:N2}";

                return $"current: {clock.CurrentTime:N2} running: {clock.IsRunning} rate: {clock.Rate}";
            }
        }
    }
}
