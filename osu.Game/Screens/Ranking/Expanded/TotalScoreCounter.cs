// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// A counter for the player's total score to be displayed in the <see cref="ExpandedPanelMiddleContent"/>.
    /// </summary>
    public class TotalScoreCounter : RollingCounter<long>
    {
        protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

        protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

        private readonly bool playSound;
        private bool isTicking;
        private readonly Bindable<double> tickPlaybackRate = new Bindable<double>();
        private double lastSampleTime;
        private DrawableSample sampleTick;

        public TotalScoreCounter(bool playSound = false)
        {
            // Todo: AutoSize X removed here due to https://github.com/ppy/osu-framework/issues/3369
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            this.playSound = playSound;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            AddInternal(sampleTick = new DrawableSample(audio.Samples.Get(@"Results/score-tick-lesser")));
            lastSampleTime = Time.Current;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(startTickingPlayback);
        }

        protected override LocalisableString FormatCount(long count) => count.ToString("N0");

        protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
        {
            s.Anchor = Anchor.TopCentre;
            s.Origin = Anchor.TopCentre;

            s.Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light, fixedWidth: true);
            s.Spacing = new Vector2(-5, 0);
        });

        protected override void Update()
        {
            base.Update();

            if (playSound && isTicking) playTickSample();
        }

        private void startTickingPlayback(ValueChangedEvent<long> _)
        {
            const double tick_debounce_rate_start = 10f;
            const double tick_debounce_rate_end = 100f;
            const double tick_volume_start = 0.5f;
            const double tick_volume_end = 1.0f;
            double tickDuration = RollingDuration - AccuracyCircle.ACCURACY_TRANSFORM_DELAY - AccuracyCircle.RANK_CIRCLE_TRANSFORM_DELAY;

            this.TransformBindableTo(tickPlaybackRate, tick_debounce_rate_start);
            this.TransformBindableTo(tickPlaybackRate, tick_debounce_rate_end, tickDuration, Easing.OutSine);
            sampleTick.VolumeTo(tick_volume_start).Then().VolumeTo(tick_volume_end, tickDuration, Easing.OutSine);

            Scheduler.AddDelayed(stopTickingPlayback, tickDuration);

            isTicking = true;
        }

        private void stopTickingPlayback() => isTicking = false;

        private void playTickSample()
        {
            if (Time.Current > lastSampleTime + tickPlaybackRate.Value)
            {
                sampleTick?.Play();
                lastSampleTime = Time.Current;
            }
        }
    }
}
