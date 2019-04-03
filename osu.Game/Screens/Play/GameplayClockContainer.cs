// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
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

        /// <summary>
        /// The original source (usually a <see cref="WorkingBeatmap"/>'s track).
        /// </summary>
        private readonly IAdjustableClock sourceClock;

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        private readonly DecoupleableInterpolatingFramedClock adjustableClock;

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

        private readonly FramedOffsetClock offsetClock;

        public GameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime)
        {
            this.beatmap = beatmap;

            RelativeSizeAxes = Axes.Both;

            sourceClock = (IAdjustableClock)beatmap.Track ?? new StopwatchClock();

            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            adjustableClock.Seek(Math.Min(0, gameplayStartTime - beatmap.BeatmapInfo.AudioLeadIn));

            adjustableClock.ProcessFrame();

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            var platformOffsetClock = new FramedOffsetClock(adjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 22 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            offsetClock = new FramedOffsetClock(platformOffsetClock);

            // the clock to be exposed via DI to children.
            GameplayClock = new GameplayClock(offsetClock);

            GameplayClock.IsPaused.BindTo(IsPaused);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => offsetClock.Offset = offset.NewValue, true);

            UserPlaybackRate.ValueChanged += _ => updateRate();
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

                    this.Delay(750).Schedule(() =>
                    {
                        if (!IsPaused.Value)
                        {
                            adjustableClock.Start();
                        }
                    });
                });
            });
        }

        public void Start()
        {
            // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
            // This accounts for the audio clock source potentially taking time to enter a completely stopped state
            adjustableClock.Seek(adjustableClock.CurrentTime);
            adjustableClock.Start();
            IsPaused.Value = false;
        }

        public void Seek(double time) => adjustableClock.Seek(time);

        public void Stop()
        {
            adjustableClock.Stop();
            IsPaused.Value = true;
        }

        public void ResetLocalAdjustments()
        {
            // In the case of replays, we may have changed the playback rate.
            UserPlaybackRate.Value = 1;
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                offsetClock.ProcessFrame();

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

            foreach (var mod in beatmap.Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(sourceClock);
        }
    }
}
