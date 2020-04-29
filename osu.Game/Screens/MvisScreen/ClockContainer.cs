// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Mvis
{
    /// <summary>
    /// Encapsulates gameplay timing logic and provides a <see cref="Play.GameplayClock"/> for children.
    /// </summary>
    public class ClockContainer : Container
    {
        private readonly WorkingBeatmap beatmap;

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

        /// <summary>
        /// The final clock which is exposed to underlying components.
        /// </summary>
        [Cached]
        public readonly GameplayClock GameplayClock;

        private Bindable<double> userAudioOffset;

        private readonly FramedOffsetClock userOffsetClock;

        private readonly FramedOffsetClock platformOffsetClock;

        public ClockContainer(WorkingBeatmap beatmap, double gameplayStartTime)
        {
            this.beatmap = beatmap;
            this.gameplayStartTime = gameplayStartTime;
            firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;

            RelativeSizeAxes = Axes.Both;

            track = beatmap.Track;

            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new FramedOffsetClock(adjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            userOffsetClock = new FramedOffsetClock(platformOffsetClock);

            // the clock to be exposed via DI to children.
            GameplayClock = new GameplayClock(userOffsetClock);

            GameplayClock.IsPaused.BindTo(IsPaused);
        }

        private double totalOffset => userOffsetClock.Offset + platformOffsetClock.Offset;

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

        protected override void Update()
        {
            if (!IsPaused.Value)
                userOffsetClock.ProcessFrame();

            Seek(beatmap.Track.CurrentTime);

            base.Update();
        }

        private bool speedAdjustmentsApplied;

        private void updateRate()
        {
            if (track == null) return;

            speedAdjustmentsApplied = true;
            track.ResetSpeedAdjustments();

            track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
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
    }
}
