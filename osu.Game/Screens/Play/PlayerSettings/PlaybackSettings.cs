// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class PlaybackSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        protected override string Title => @"playback";

        public IAdjustableClock AdjustableClock { set; get; }

        private readonly PlayerSliderBar<double> sliderbar;

        public PlaybackSettings()
        {
            OsuSpriteText multiplierText;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = padding },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = "Playback speed",
                        },
                        multiplierText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Font = @"Exo2.0-Bold",
                        }
                    },
                },
                sliderbar = new PlayerSliderBar<double>
                {
                    Bindable = new BindableDouble(1)
                    {
                        Default = 1,
                        MinValue = 0.5,
                        MaxValue = 2,
                        Precision = 0.1,
                    },
                }
            };

            sliderbar.Bindable.ValueChanged += rateMultiplier => multiplierText.Text = $"{sliderbar.Bar.TooltipText}x";
            sliderbar.Bindable.TriggerChange();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (AdjustableClock == null)
                return;

            var clockRate = AdjustableClock.Rate;
            sliderbar.Bindable.ValueChanged += rateMultiplier => AdjustableClock.Rate = clockRate * rateMultiplier;
        }
    }
}
