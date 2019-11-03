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
        /// The original source (usually a <see cref="WorkingBeatmap"/>'s track).
        /// </summary>
        private IAdjustableClock sourceClock;

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        private readonly DecoupleableInterpolatingFramedClock adjustableClock;

        private readonly double gameplayStartTime;

        public readonly Bindable<double> UserPlaybackRate = new BindableDouble(1)
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

            RelativeSizeAxes = Axes.Both;

            sourceClock = (IAdjustableClock)beatmap.Track ?? new StopwatchClock();
            (sourceClock as IAdjustableAudioComponent)?.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);

            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new FramedOffsetClock(adjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 22 : 0 };

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

            UserPlaybackRate.ValueChanged += _ => updateRate();

            Seek(Math.Min(-beatmap.BeatmapInfo.AudioLeadIn, gameplayStartTime));
        }

        public void Restart()
        {
            Task.Run(() =>
            {
                sourceClock.Reset();

                Schedule(() =>
                {
                    adjustableClock.ChangeSource(sourceClock);
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

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided beatmap's track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            if (sourceClock != beatmap.Track)
                return;

            sourceClock = new TrackVirtual(beatmap.Track.Length);
            adjustableClock.ChangeSource(sourceClock);
        }

        public void ResetLocalAdjustments()
        {
            // In the case of replays, we may have changed the playback rate.
            UserPlaybackRate.Value = 1;
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                userOffsetClock.ProcessFrame();

            base.Update();
        }

        private void updateRate()
        {
            if (sourceClock == null) return;

            sourceClock.ResetSpeedAdjustments();

            if (sourceClock is IHasTempoAdjust tempo)
                tempo.TempoAdjust = UserPlaybackRate.Value;
            else
                sourceClock.Rate = UserPlaybackRate.Value;

            foreach (var mod in mods.OfType<IApplicableToClock>())
                mod.ApplyToClock(sourceClock);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            (sourceClock as IAdjustableAudioComponent)?.RemoveAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
        }
    }
}
