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
using osu.Game.Configuration;

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
    public class MasterGameplayClockContainer : GameplayClockContainer
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

        private double totalAppliedOffset => userOffsetClock.RateAdjustedOffset + platformOffsetClock.RateAdjustedOffset;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(1);

        private readonly WorkingBeatmap beatmap;
        private readonly double gameplayStartTime;
        private readonly bool startAtGameplayStart;
        private readonly double firstHitObjectTime;

        private HardwareCorrectionOffsetClock userOffsetClock;
        private HardwareCorrectionOffsetClock platformOffsetClock;
        private MasterGameplayClock masterGameplayClock;
        private Bindable<double> userAudioOffset;
        private double startOffset;

        public MasterGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime, bool startAtGameplayStart = false)
            : base(beatmap.Track)
        {
            this.beatmap = beatmap;
            this.gameplayStartTime = gameplayStartTime;
            this.startAtGameplayStart = startAtGameplayStart;

            firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => userOffsetClock.Offset = offset.NewValue, true);

            // sane default provided by ruleset.
            startOffset = gameplayStartTime;

            if (!startAtGameplayStart)
            {
                startOffset = Math.Min(0, startOffset);

                // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
                // this is commonly used to display an intro before the audio track start.
                double? firstStoryboardEvent = beatmap.Storyboard.EarliestEventTime;
                if (firstStoryboardEvent != null)
                    startOffset = Math.Min(startOffset, firstStoryboardEvent.Value);

                // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
                // this is not available as an option in the live editor but can still be applied via .osu editing.
                if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                    startOffset = Math.Min(startOffset, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);
            }

            Seek(startOffset);
        }

        protected override void OnIsPausedChanged(ValueChangedEvent<bool> isPaused)
        {
            // The source is stopped by a frequency fade first.
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
            if (GameplayClock.CurrentTime > gameplayStartTime - MINIMUM_SKIP_TIME)
                return;

            double skipTarget = gameplayStartTime - MINIMUM_SKIP_TIME;

            if (GameplayClock.CurrentTime < 0 && skipTarget > 6000)
                // double skip exception for storyboards with very long intros
                skipTarget = 0;

            Seek(skipTarget);
        }

        public override void Reset()
        {
            base.Reset();
            Seek(startOffset);
        }

        protected override GameplayClock CreateGameplayClock(IFrameBasedClock source)
        {
            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new HardwareCorrectionOffsetClock(source, pauseFreqAdjust) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            userOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock, pauseFreqAdjust);

            return masterGameplayClock = new MasterGameplayClock(userOffsetClock);
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
            removeSourceClockAdjustments();
        }

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
