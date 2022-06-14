// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A <see cref="GameplayClockContainer"/> which uses a <see cref="WorkingBeatmap"/> as a source.
    /// <para>
    /// This is the most complete <see cref="GameplayClockContainer"/> which takes into account all user and platform offsets,
    /// and provides implementations for user actions such as skipping or adjusting playback rates that may occur during gameplay.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This is intended to be used as a single controller for gameplay, or as a reference source for other <see cref="GameplayClockContainer"/>s.
    /// </remarks>
    public class MasterGameplayClockContainer : GameplayClockContainer, IBeatSyncProvider
    {
        /// <summary>
        /// Duration before gameplay start time required before skip button displays.
        /// </summary>
        public const double MINIMUM_SKIP_TIME = 1000;

        protected Track Track => (Track)SourceClock;

        public readonly BindableNumber<double> UserPlaybackRate = new BindableDouble(1)
        {
            Default = 1,
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };

        private double totalAppliedOffset => userBeatmapOffsetClock.RateAdjustedOffset + userGlobalOffsetClock.RateAdjustedOffset + platformOffsetClock.RateAdjustedOffset;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(); // Important that this starts at zero, matching the paused state of the clock.

        private readonly WorkingBeatmap beatmap;

        private HardwareCorrectionOffsetClock userGlobalOffsetClock;
        private HardwareCorrectionOffsetClock userBeatmapOffsetClock;
        private HardwareCorrectionOffsetClock platformOffsetClock;
        private MasterGameplayClock masterGameplayClock;
        private Bindable<double> userAudioOffset;

        private IDisposable beatmapOffsetSubscription;

        private readonly double skipTargetTime;

        [Resolved]
        private RealmAccess realm { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        /// <summary>
        /// Create a new master gameplay clock container.
        /// </summary>
        /// <param name="beatmap">The beatmap to be used for time and metadata references.</param>
        /// <param name="skipTargetTime">The latest time which should be used when introducing gameplay. Will be used when skipping forward.</param>
        public MasterGameplayClockContainer(WorkingBeatmap beatmap, double skipTargetTime)
            : base(beatmap.Track)
        {
            this.beatmap = beatmap;
            this.skipTargetTime = skipTargetTime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => userGlobalOffsetClock.Offset = offset.NewValue, true);

            beatmapOffsetSubscription = realm.SubscribeToPropertyChanged(
                r => r.Find<BeatmapInfo>(beatmap.BeatmapInfo.ID)?.UserSettings,
                settings => settings.Offset,
                val => userBeatmapOffsetClock.Offset = val);

            // Reset may have been called externally before LoadComplete.
            // If it was, and the clock is in a playing state, we want to ensure that it isn't stopped here.
            bool isStarted = !IsPaused.Value;

            // If a custom start time was not specified, calculate the best value to use.
            StartTime ??= findEarliestStartTime();

            Reset(startClock: isStarted);
        }

        private double findEarliestStartTime()
        {
            // here we are trying to find the time to start playback from the "zero" point.
            // generally this is either zero, or some point earlier than zero in the case of storyboards, lead-ins etc.

            // start with the originally provided latest time (if before zero).
            double time = Math.Min(0, skipTargetTime);

            // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
            // this is commonly used to display an intro before the audio track start.
            double? firstStoryboardEvent = beatmap.Storyboard.EarliestEventTime;
            if (firstStoryboardEvent != null)
                time = Math.Min(time, firstStoryboardEvent.Value);

            // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
            // this is not available as an option in the live editor but can still be applied via .osu editing.
            double firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;
            if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                time = Math.Min(time, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);

            return time;
        }

        protected override void OnIsPausedChanged(ValueChangedEvent<bool> isPaused)
        {
            if (IsLoaded)
            {
                // During normal operation, the source is stopped after performing a frequency ramp.
                if (isPaused.NewValue)
                {
                    this.TransformBindableTo(pauseFreqAdjust, 0, 200, Easing.Out).OnComplete(_ =>
                    {
                        if (IsPaused.Value == isPaused.NewValue)
                            AdjustableSource.Stop();
                    });
                }
                else
                    this.TransformBindableTo(pauseFreqAdjust, 1, 200, Easing.In);
            }
            else
            {
                if (isPaused.NewValue)
                    AdjustableSource.Stop();

                // If not yet loaded, we still want to ensure relevant state is correct, as it is used for offset calculations.
                pauseFreqAdjust.Value = isPaused.NewValue ? 0 : 1;

                // We must also process underlying gameplay clocks to update rate-adjusted offsets with the new frequency adjustment.
                // Without doing this, an initial seek may be performed with the wrong offset.
                GameplayClock.UnderlyingClock.ProcessFrame();
            }
        }

        public override void Start()
        {
            addSourceClockAdjustments();
            base.Start();
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// </summary>
        /// <remarks>
        /// Adjusts for any offsets which have been applied (so the seek may not be the expected point in time on the underlying audio track).
        /// </remarks>
        /// <param name="time">The destination time to seek to.</param>
        public override void Seek(double time)
        {
            // remove the offset component here because most of the time we want the seek to be aligned to gameplay, not the audio track.
            // we may want to consider reversing the application of offsets in the future as it may feel more correct.
            base.Seek(time - totalAppliedOffset);
        }

        /// <summary>
        /// Skip forward to the next valid skip point.
        /// </summary>
        public void Skip()
        {
            if (GameplayClock.CurrentTime > skipTargetTime - MINIMUM_SKIP_TIME)
                return;

            double skipTarget = skipTargetTime - MINIMUM_SKIP_TIME;

            if (GameplayClock.CurrentTime < 0 && skipTarget > 6000)
                // double skip exception for storyboards with very long intros
                skipTarget = 0;

            Seek(skipTarget);
        }

        protected override GameplayClock CreateGameplayClock(IFrameBasedClock source)
        {
            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new HardwareCorrectionOffsetClock(source, pauseFreqAdjust) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            userGlobalOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock, pauseFreqAdjust);
            userBeatmapOffsetClock = new HardwareCorrectionOffsetClock(userGlobalOffsetClock, pauseFreqAdjust);

            return masterGameplayClock = new MasterGameplayClock(userBeatmapOffsetClock);
        }

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            removeSourceClockAdjustments();
            ChangeSource(new TrackVirtual(beatmap.Track.Length));
            addSourceClockAdjustments();
        }

        private bool speedAdjustmentsApplied;

        private void addSourceClockAdjustments()
        {
            if (speedAdjustmentsApplied)
                return;

            Track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            Track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            masterGameplayClock.MutableNonGameplayAdjustments.Add(pauseFreqAdjust);
            masterGameplayClock.MutableNonGameplayAdjustments.Add(UserPlaybackRate);

            speedAdjustmentsApplied = true;
        }

        private void removeSourceClockAdjustments()
        {
            if (!speedAdjustmentsApplied)
                return;

            Track.RemoveAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            Track.RemoveAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            masterGameplayClock.MutableNonGameplayAdjustments.Remove(pauseFreqAdjust);
            masterGameplayClock.MutableNonGameplayAdjustments.Remove(UserPlaybackRate);

            speedAdjustmentsApplied = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
            removeSourceClockAdjustments();
        }

        ControlPointInfo IBeatSyncProvider.ControlPoints => beatmap.Beatmap.ControlPointInfo;
        IClock IBeatSyncProvider.Clock => GameplayClock;
        ChannelAmplitudes? IBeatSyncProvider.Amplitudes => beatmap.TrackLoaded ? beatmap.Track.CurrentAmplitudes : (ChannelAmplitudes?)null;

        private class HardwareCorrectionOffsetClock : FramedOffsetClock
        {
            private readonly BindableDouble pauseRateAdjust;

            private double offset;

            public new double Offset
            {
                get => offset;
                set
                {
                    if (value == offset)
                        return;

                    offset = value;

                    updateOffset();
                }
            }

            public double RateAdjustedOffset => base.Offset;

            public HardwareCorrectionOffsetClock(IClock source, BindableDouble pauseRateAdjust)
                : base(source)
            {
                this.pauseRateAdjust = pauseRateAdjust;
            }

            public override void ProcessFrame()
            {
                base.ProcessFrame();
                updateOffset();
            }

            private void updateOffset()
            {
                // changing this during the pause transform effect will cause a potentially large offset to be suddenly applied as we approach zero rate.
                if (pauseRateAdjust.Value == 1)
                {
                    // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
                    base.Offset = Offset * Rate;
                }
            }
        }

        private class MasterGameplayClock : GameplayClock
        {
            public readonly List<Bindable<double>> MutableNonGameplayAdjustments = new List<Bindable<double>>();
            public override IEnumerable<Bindable<double>> NonGameplayAdjustments => MutableNonGameplayAdjustments;

            public MasterGameplayClock(FramedOffsetClock underlyingClock)
                : base(underlyingClock)
            {
            }
        }
    }
}
