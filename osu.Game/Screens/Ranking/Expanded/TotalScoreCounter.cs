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
    public partial class TotalScoreCounter : RollingCounter<long>
    {
        protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

        protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

        private readonly bool playSamples;

        private readonly Bindable<double> tickPlaybackRate = new Bindable<double>();

        private double lastSampleTime;

        private DrawableSample sampleTick;

        public TotalScoreCounter(bool playSamples = false)
        {
            // Todo: AutoSize X removed here due to https://github.com/ppy/osu-framework/issues/3369
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            this.playSamples = playSamples;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            AddInternal(sampleTick = new DrawableSample(audio.Samples.Get(@"Results/score-tick-lesser")));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (playSamples)
                Current.BindValueChanged(_ => startTicking());
        }

        protected override LocalisableString FormatCount(long count) => count.ToString("N0");

        protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
        {
            s.Anchor = Anchor.TopCentre;
            s.Origin = Anchor.TopCentre;

            s.Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light, fixedWidth: true);
            s.Spacing = new Vector2(-5, 0);
        });

        public override long DisplayedCount
        {
            get => base.DisplayedCount;
            set
            {
                if (base.DisplayedCount == value)
                    return;

                base.DisplayedCount = value;

                if (playSamples && Time.Current > lastSampleTime + tickPlaybackRate.Value)
                {
                    sampleTick?.Play();
                    lastSampleTime = Time.Current;
                }
            }
        }

        private void startTicking()
        {
            const double tick_debounce_rate_start = 10f;
            const double tick_debounce_rate_end = 100f;
            const double tick_volume_start = 0.5f;
            const double tick_volume_end = 1.0f;

            this.TransformBindableTo(tickPlaybackRate, tick_debounce_rate_start);
            this.TransformBindableTo(tickPlaybackRate, tick_debounce_rate_end, RollingDuration, Easing.OutSine);
            sampleTick.VolumeTo(tick_volume_start).Then().VolumeTo(tick_volume_end, RollingDuration, Easing.OutSine);
        }
    }
}
