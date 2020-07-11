// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Encapsulates gameplay timing logic and provides a <see cref="Play.GameplayClock"/> for children.
    /// </summary>
    public class GameplayClockContainer : Container
    {
        private readonly WorkingBeatmap beatmap;
        private readonly IReadOnlyList<Mod> mods;

        /// <summary>
        /// The <see cref="WorkingBeatmap"/>'s track.
        /// </summary>
        private Track track;

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        private readonly DecoupleableInterpolatingFramedClock adjustableClock;

        private readonly double gameplayStartTime;

        private readonly double firstHitObjectTime;

        public readonly BindableNumber<double> UserPlaybackRate = new BindableDouble(1)
        {
            Default = 1,
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };

        /// <summary>
        /// The final clock which is exposed to underlying components.
        /// </summary>
        [Cached]
        public readonly GameplayClock GameplayClock;

        private Bindable<double> userAudioOffset;

        private readonly FramedOffsetClock userOffsetClock;

        private readonly FramedOffsetClock platformOffsetClock;

        public GameplayClockContainer(WorkingBeatmap beatmap, IReadOnlyList<Mod> mods, double gameplayStartTime)
        {
            this.beatmap = beatmap;
            this.mods = mods;
            this.gameplayStartTime = gameplayStartTime;
            firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;

            RelativeSizeAxes = Axes.Both;

            track = beatmap.Track;

            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new HardwareCorrectionOffsetClock(adjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            userOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock);

            // the clock to be exposed via DI to children.
            GameplayClock = new GameplayClock(userOffsetClock);

            GameplayClock.IsPaused.BindTo(IsPaused);
        }

        private double totalOffset => userOffsetClock.Offset + platformOffsetClock.Offset;

        /// <summary>
        /// Duration before gameplay start time required before skip button displays.
        /// </summary>
        public const double MINIMUM_SKIP_TIME = 1000;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(1);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => userOffsetClock.Offset = offset.NewValue, true);

            // sane default provided by ruleset.
            double startTime = Math.Min(0, gameplayStartTime);

            // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
            // this is commonly used to display an intro before the audio track start.
            startTime = Math.Min(startTime, beatmap.Storyboard.FirstEventTime);

            // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
            // this is not available as an option in the live editor but can still be applied via .osu editing.
            if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                startTime = Math.Min(startTime, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);

            Seek(startTime);

            adjustableClock.ProcessFrame();
        }

        public void Restart()
        {
            // The Reset() call below causes speed adjustments to be reset in an async context, leading to deadlocks.
            // The deadlock can be prevented by resetting the track synchronously before entering the async context.
            track.ResetSpeedAdjustments();

            Task.Run(() =>
            {
                track.Reset();

                Schedule(() =>
                {
                    adjustableClock.ChangeSource(track);
                    updateRate();

                    if (!IsPaused.Value)
                        Start();
                });
            });
        }

        public void Start()
        {
            // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
            // This accounts for the audio clock source potentially taking time to enter a completely stopped state
            Seek(GameplayClock.CurrentTime);
            adjustableClock.Start();
            IsPaused.Value = false;

            this.TransformBindableTo(pauseFreqAdjust, 1, 200, Easing.In);
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

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// <remarks>
        /// Adjusts for any offsets which have been applied (so the seek may not be the expected point in time on the underlying audio track).
        /// </remarks>
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public void Seek(double time)
        {
            // remove the offset component here because most of the time we want the seek to be aligned to gameplay, not the audio track.
            // we may want to consider reversing the application of offsets in the future as it may feel more correct.
            adjustableClock.Seek(time - totalOffset);

            // manually process frame to ensure GameplayClock is correctly updated after a seek.
            userOffsetClock.ProcessFrame();
        }

        public void Stop()
        {
            this.TransformBindableTo(pauseFreqAdjust, 0, 200, Easing.Out).OnComplete(_ => adjustableClock.Stop());

            IsPaused.Value = true;
        }

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided beatmap's track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            if (track != beatmap.Track)
                return;

            removeSourceClockAdjustments();

            track = new TrackVirtual(beatmap.Track.Length);
            adjustableClock.ChangeSource(track);
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                userOffsetClock.ProcessFrame();

            base.Update();
        }

        private bool speedAdjustmentsApplied;

        private void updateRate()
        {
            if (track == null) return;

            speedAdjustmentsApplied = true;
            track.ResetSpeedAdjustments();

            track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            foreach (var mod in mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(track);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            removeSourceClockAdjustments();
            track = null;
        }

        private void removeSourceClockAdjustments()
        {
            if (speedAdjustmentsApplied)
            {
                track.ResetSpeedAdjustments();
                speedAdjustmentsApplied = false;
            }
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
    }
}
