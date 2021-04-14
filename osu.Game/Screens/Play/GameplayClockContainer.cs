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
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play
{
    public abstract class GameplayClockContainer : Container
    {
        /// <summary>
        /// The final clock which is exposed to underlying components.
        /// </summary>
        public GameplayClock GameplayClock { get; private set; }

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        protected readonly DecoupleableInterpolatingFramedClock AdjustableClock;

        protected readonly IClock SourceClock;

        protected GameplayClockContainer(IClock sourceClock)
        {
            SourceClock = sourceClock;

            RelativeSizeAxes = Axes.Both;

            AdjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            AdjustableClock.ChangeSource(SourceClock);

            IsPaused.BindValueChanged(OnPauseChanged);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(GameplayClock = CreateGameplayClock(AdjustableClock));

            return dependencies;
        }

        public virtual void Start()
        {
            if (!AdjustableClock.IsRunning)
            {
                // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
                // This accounts for the clock source potentially taking time to enter a completely stopped state
                Seek(GameplayClock.CurrentTime);

                AdjustableClock.Start();
            }

            IsPaused.Value = false;
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// <remarks>
        /// Adjusts for any offsets which have been applied (so the seek may not be the expected point in time on the underlying audio track).
        /// </remarks>
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public virtual void Seek(double time) => AdjustableClock.Seek(time);

        public virtual void Stop() => IsPaused.Value = true;

        public virtual void Restart()
        {
            AdjustableClock.Seek(0);
            AdjustableClock.Stop();

            if (!IsPaused.Value)
                Start();
        }

        protected abstract void OnPauseChanged(ValueChangedEvent<bool> isPaused);

        protected abstract GameplayClock CreateGameplayClock(IClock source);
    }

    public class MasterGameplayClockContainer : GameplayClockContainer
    {
        /// <summary>
        /// Duration before gameplay start time required before skip button displays.
        /// </summary>
        public const double MINIMUM_SKIP_TIME = 1000;

        protected new DecoupleableInterpolatingFramedClock SourceClock => (DecoupleableInterpolatingFramedClock)base.SourceClock;

        public readonly BindableNumber<double> UserPlaybackRate = new BindableDouble(1)
        {
            Default = 1,
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };

        private double totalOffset => userOffsetClock.Offset + platformOffsetClock.Offset;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(1);

        private readonly WorkingBeatmap beatmap;
        private readonly double gameplayStartTime;
        private readonly bool startAtGameplayStart;
        private readonly double firstHitObjectTime;

        private FramedOffsetClock userOffsetClock;
        private FramedOffsetClock platformOffsetClock;
        private LocalGameplayClock localGameplayClock;
        private Bindable<double> userAudioOffset;

        public MasterGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime, bool startAtGameplayStart = false)
            : base(new DecoupleableInterpolatingFramedClock())
        {
            this.beatmap = beatmap;
            this.gameplayStartTime = gameplayStartTime;
            this.startAtGameplayStart = startAtGameplayStart;

            firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;

            SourceClock.ChangeSource(beatmap.Track);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => userOffsetClock.Offset = offset.NewValue, true);

            // sane default provided by ruleset.
            double startTime = gameplayStartTime;

            if (!startAtGameplayStart)
            {
                startTime = Math.Min(0, startTime);

                // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
                // this is commonly used to display an intro before the audio track start.
                double? firstStoryboardEvent = beatmap.Storyboard.EarliestEventTime;
                if (firstStoryboardEvent != null)
                    startTime = Math.Min(startTime, firstStoryboardEvent.Value);

                // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
                // this is not available as an option in the live editor but can still be applied via .osu editing.
                if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                    startTime = Math.Min(startTime, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);
            }

            Seek(startTime);

            AdjustableClock.ProcessFrame();
        }

        protected override void OnPauseChanged(ValueChangedEvent<bool> isPaused)
        {
            if (isPaused.NewValue)
                this.TransformBindableTo(pauseFreqAdjust, 0, 200, Easing.Out).OnComplete(_ => AdjustableClock.Stop());
            else
                this.TransformBindableTo(pauseFreqAdjust, 1, 200, Easing.In);
        }

        public override void Seek(double time)
        {
            // remove the offset component here because most of the time we want the seek to be aligned to gameplay, not the audio track.
            // we may want to consider reversing the application of offsets in the future as it may feel more correct.
            base.Seek(time - totalOffset);

            // manually process frame to ensure GameplayClock is correctly updated after a seek.
            userOffsetClock.ProcessFrame();
        }

        public override void Restart()
        {
            updateRate();
            base.Restart();
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

        protected override GameplayClock CreateGameplayClock(IClock source)
        {
            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new HardwareCorrectionOffsetClock(source) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            userOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock);

            return localGameplayClock = new LocalGameplayClock(userOffsetClock);
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                userOffsetClock.ProcessFrame();

            base.Update();
        }

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            removeSourceClockAdjustments();
            SourceClock.ChangeSource(new TrackVirtual(beatmap.Track.Length));
        }

        private bool speedAdjustmentsApplied;

        private void updateRate()
        {
            if (speedAdjustmentsApplied)
                return;

            var track = (Track)SourceClock.Source;

            track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            localGameplayClock.MutableNonGameplayAdjustments.Add(pauseFreqAdjust);
            localGameplayClock.MutableNonGameplayAdjustments.Add(UserPlaybackRate);

            speedAdjustmentsApplied = true;
        }

        private void removeSourceClockAdjustments()
        {
            if (!speedAdjustmentsApplied)
                return;

            var track = (Track)SourceClock.Source;

            track.RemoveAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            track.RemoveAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            localGameplayClock.MutableNonGameplayAdjustments.Remove(pauseFreqAdjust);
            localGameplayClock.MutableNonGameplayAdjustments.Remove(UserPlaybackRate);

            speedAdjustmentsApplied = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            removeSourceClockAdjustments();
        }

        private class HardwareCorrectionOffsetClock : FramedOffsetClock
        {
            // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
            // base implementation already adds offset at 1.0 rate, so we only add the difference from that here.
            public override double CurrentTime => base.CurrentTime + Offset * (Rate - 1);

            public HardwareCorrectionOffsetClock(IClock source, bool processSource = true)
                : base(source, processSource)
            {
            }
        }

        private class LocalGameplayClock : GameplayClock
        {
            public readonly List<Bindable<double>> MutableNonGameplayAdjustments = new List<Bindable<double>>();

            public override IEnumerable<Bindable<double>> NonGameplayAdjustments => MutableNonGameplayAdjustments;

            public LocalGameplayClock(FramedOffsetClock underlyingClock)
                : base(underlyingClock)
            {
            }
        }
    }
}
