// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
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
    ///  - Optionally applies (and tracks changes of) beatmap, user, and platform offsets (see ctor argument applyOffsets).
    ///  - Adjusts <see cref="Seek"/> operations to account for any applied offsets, seeking in raw "beatmap" time values.
    ///  - Exposes track length.
    ///  - Allows changing the source to a new track (for cases like editor track updating).
    /// </summary>
    public partial class FramedBeatmapClock : Component, IFrameBasedClock, IAdjustableClock, ISourceChangeableClock
    {
        private readonly bool applyOffsets;

        /// <summary>
        /// The length of the underlying beatmap track. Will default to 60 seconds if unavailable.
        /// </summary>
        public double TrackLength => Track.Length;

        /// <summary>
        /// The underlying beatmap track, if available.
        /// </summary>
        public Track Track { get; private set; } = new TrackVirtual(60000);

        /// <summary>
        /// The total frequency adjustment from pause transforms. Should eventually be handled in a better way.
        /// </summary>
        public readonly BindableDouble ExternalPauseFrequencyAdjust = new BindableDouble(1);

        private readonly OffsetCorrectionClock? userGlobalOffsetClock;
        private readonly OffsetCorrectionClock? platformOffsetClock;
        private readonly OffsetCorrectionClock? userBeatmapOffsetClock;

        private readonly IFrameBasedClock finalClockSource;

        private Bindable<double>? userAudioOffset;

        private IDisposable? beatmapOffsetSubscription;

        private readonly DecoupleableInterpolatingFramedClock decoupledClock;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public bool IsRewinding { get; private set; }

        public bool IsCoupled
        {
            get => decoupledClock.IsCoupled;
            set => decoupledClock.IsCoupled = value;
        }

        public FramedBeatmapClock(bool applyOffsets = false)
        {
            this.applyOffsets = applyOffsets;

            // A decoupled clock is used to ensure precise time values even when the host audio subsystem is not reporting
            // high precision times (on windows there's generally only 5-10ms reporting intervals, as an example).
            decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true };

            if (applyOffsets)
            {
                // Audio timings in general with newer BASS versions don't match stable.
                // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
                platformOffsetClock = new OffsetCorrectionClock(decoupledClock, ExternalPauseFrequencyAdjust) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

                // User global offset (set in settings) should also be applied.
                userGlobalOffsetClock = new OffsetCorrectionClock(platformOffsetClock, ExternalPauseFrequencyAdjust);

                // User per-beatmap offset will be applied to this final clock.
                finalClockSource = userBeatmapOffsetClock = new OffsetCorrectionClock(userGlobalOffsetClock, ExternalPauseFrequencyAdjust);
            }
            else
            {
                finalClockSource = decoupledClock;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (applyOffsets)
            {
                Debug.Assert(userBeatmapOffsetClock != null);
                Debug.Assert(userGlobalOffsetClock != null);

                userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
                userAudioOffset.BindValueChanged(offset => userGlobalOffsetClock.Offset = offset.NewValue, true);

                beatmapOffsetSubscription = realm.SubscribeToPropertyChanged(
                    r => r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings,
                    settings => settings.Offset,
                    val =>
                    {
                        userBeatmapOffsetClock.Offset = val;
                    });
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Source != null && Source is not IAdjustableClock && Source.CurrentTime < decoupledClock.CurrentTime - 100)
            {
                // InterpolatingFramedClock won't interpolate backwards unless its source has an ElapsedFrameTime.
                // See https://github.com/ppy/osu-framework/blob/ba1385330cc501f34937e08257e586c84e35d772/osu.Framework/Timing/InterpolatingFramedClock.cs#L91-L93
                // This is not always the case here when doing large seeks.
                // (Of note, this is not an issue if the source is adjustable, as the source is seeked to be in time by DecoupleableInterpolatingFramedClock).
                // Rather than trying to get around this by fixing the framework clock stack, let's work around it for now.
                Seek(Source.CurrentTime);
            }
            else
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
                Debug.Assert(platformOffsetClock != null);

                return userGlobalOffsetClock.RateAdjustedOffset + userBeatmapOffsetClock.RateAdjustedOffset + platformOffsetClock.RateAdjustedOffset;
            }
        }

        #region Delegation of IAdjustableClock / ISourceChangeableClock to decoupled clock.

        public void ChangeSource(IClock? source)
        {
            Track = source as Track ?? new TrackVirtual(60000);
            decoupledClock.ChangeSource(source);
        }

        public IClock? Source => decoupledClock.Source;

        public void Reset()
        {
            decoupledClock.Reset();
            finalClockSource.ProcessFrame();
        }

        public void Start()
        {
            decoupledClock.Start();
            finalClockSource.ProcessFrame();
        }

        public void Stop()
        {
            decoupledClock.Stop();
            finalClockSource.ProcessFrame();
        }

        public bool Seek(double position)
        {
            bool success = decoupledClock.Seek(position - TotalAppliedOffset);
            finalClockSource.ProcessFrame();

            return success;
        }

        public void ResetSpeedAdjustments() => decoupledClock.ResetSpeedAdjustments();

        public double Rate
        {
            get => decoupledClock.Rate;
            set => decoupledClock.Rate = value;
        }

        #endregion

        #region Delegation of IFrameBasedClock to clock with all offsets applied

        public double CurrentTime => finalClockSource.CurrentTime;

        public bool IsRunning => finalClockSource.IsRunning;

        public void ProcessFrame()
        {
            // Noop to ensure an external consumer doesn't process the internal clock an extra time.
        }

        public double ElapsedFrameTime => finalClockSource.ElapsedFrameTime;

        public double FramesPerSecond => finalClockSource.FramesPerSecond;

        public FrameTimeInfo TimeInfo => finalClockSource.TimeInfo;

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
        }
    }
}
