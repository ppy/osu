// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        /// <summary>
        /// An <see cref="ITrack"/> whose applied adjustments are considered as part of the gameplay itself, unrelated to the playback.
        /// </summary>
        public ITrack GameplayTrack { get; }

        /// <summary>
        /// The rate of gameplay when playback is at 100%.
        /// This excludes any seeking / user adjustments.
        /// </summary>
        public double TrueGameplayRate => GameplayTrack.AggregateFrequency.Value * GameplayTrack.AggregateTempo.Value;

        /// <summary>
        /// The true gameplay rate combined with the <see cref="UserPlaybackRate"/> value.
        /// </summary>
        public double PlaybackRate => TrueGameplayRate * UserPlaybackRate.Value;

        private double totalOffset => userOffsetClock.Offset + platformOffsetClock.Offset;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(1);

        private readonly WorkingBeatmap beatmap;
        private readonly double gameplayStartTime;
        private readonly bool startAtGameplayStart;
        private readonly double firstHitObjectTime;

        protected virtual bool ApplyPlatformOffset => true;

        private FramedOffsetClock userOffsetClock;
        private FramedOffsetClock platformOffsetClock;
        private Bindable<double> userAudioOffset;
        private double startOffset;

        public MasterGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime, bool startAtGameplayStart = false)
            : base(beatmap.Track)
        {
            this.beatmap = beatmap;
            this.gameplayStartTime = gameplayStartTime;
            this.startAtGameplayStart = startAtGameplayStart;

            GameplayTrack = new MasterGameplayTrack(Track);

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
            base.Seek(time - totalOffset * PlaybackRate);
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
            if (ApplyPlatformOffset)
            {
                // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
                // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
                platformOffsetClock = new HardwareCorrectionOffsetClock(source, this) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };
            }

            // the final usable gameplay clock with user-set offsets applied.
            userOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock ?? source, this);

            return new MasterGameplayClock(userOffsetClock, this);
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

            Track.BindAdjustments(GameplayTrack);
            Track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            Track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            speedAdjustmentsApplied = true;
        }

        private void removeSourceClockAdjustments()
        {
            if (!speedAdjustmentsApplied)
                return;

            Track.RemoveAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            Track.RemoveAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);
            Track.UnbindAdjustments(GameplayTrack);

            speedAdjustmentsApplied = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            removeSourceClockAdjustments();
        }

        private class HardwareCorrectionOffsetClock : FramedOffsetClock
        {
            private readonly MasterGameplayClockContainer gameplayClockContainer;

            // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
            // base implementation already adds offset at 1.0 rate, so we only add the difference from that here.
            public override double CurrentTime => base.CurrentTime + Offset * (gameplayClockContainer.PlaybackRate - 1);

            public HardwareCorrectionOffsetClock(IClock source, MasterGameplayClockContainer gameplayClockContainer)
                : base(source)
            {
                this.gameplayClockContainer = gameplayClockContainer;
            }
        }

        private class MasterGameplayClock : GameplayClock
        {
            private readonly MasterGameplayClockContainer gameplayClockContainer;

            public override double TrueGameplayRate => gameplayClockContainer.TrueGameplayRate;

            public MasterGameplayClock(FramedOffsetClock underlyingClock, MasterGameplayClockContainer gameplayClockContainer)
                : base(underlyingClock)
            {
                this.gameplayClockContainer = gameplayClockContainer;
            }
        }

        /// <summary>
        /// An <see cref="ITrack"/> whose applied adjustments are considered as part of the gameplay, unrelated to the playback.
        /// </summary>
        private class MasterGameplayTrack : ITrack
        {
            private readonly ITrack track;

            private readonly AudioAdjustments adjustments = new AudioAdjustments();

            public MasterGameplayTrack(ITrack track)
            {
                this.track = track;
            }

            #region Delegated IAdjustableAudioComponent implementation (gameplay adjustments)

            public IBindable<double> AggregateVolume => adjustments.AggregateVolume;
            public IBindable<double> AggregateBalance => adjustments.AggregateBalance;
            public IBindable<double> AggregateFrequency => adjustments.AggregateFrequency;
            public IBindable<double> AggregateTempo => adjustments.AggregateTempo;

            public void BindAdjustments(IAggregateAudioAdjustment component) => adjustments.BindAdjustments(component);
            public void UnbindAdjustments(IAggregateAudioAdjustment component) => adjustments.UnbindAdjustments(component);

            public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => adjustments.AddAdjustment(type, adjustBindable);
            public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => adjustments.RemoveAdjustment(type, adjustBindable);
            public void RemoveAllAdjustments(AdjustableProperty type) => adjustments.RemoveAllAdjustments(type);

            public BindableNumber<double> Volume => adjustments.Volume;
            public BindableNumber<double> Balance => adjustments.Balance;
            public BindableNumber<double> Frequency => adjustments.Frequency;
            public BindableNumber<double> Tempo => adjustments.Tempo;

            #endregion

            #region Delegated ITrack implementation

            ChannelAmplitudes IHasAmplitudes.CurrentAmplitudes => track.CurrentAmplitudes;

            double IClock.Rate => track.Rate;

            public double Rate
            {
                get => track.Rate;
                set => track.Rate = value;
            }

            public bool IsRunning => track.IsRunning;

            public double CurrentTime => track.CurrentTime;

            public void Reset() => track.Reset();

            public void Start() => track.Start();

            public void Stop() => track.Stop();

            public bool Seek(double position) => track.Seek(position);

            public void Restart() => track.Restart();

            public bool Looping
            {
                get => track.Looping;
                set => track.Looping = value;
            }

            public bool IsDummyDevice => track.IsDummyDevice;

            public double RestartPoint
            {
                get => track.RestartPoint;
                set => track.RestartPoint = value;
            }

            public double Length
            {
                get => track.Length;
                set => track.Length = value;
            }

            public int? Bitrate => track.Bitrate;

            public bool IsReversed => track.IsReversed;

            public bool HasCompleted => track.HasCompleted;

            public event Action Completed
            {
                add => track.Completed += value;
                remove => track.Completed -= value;
            }

            public event Action Failed
            {
                add => track.Failed += value;
                remove => track.Failed -= value;
            }

            public void ResetSpeedAdjustments() => track.ResetSpeedAdjustments();

            #endregion
        }
    }
}
