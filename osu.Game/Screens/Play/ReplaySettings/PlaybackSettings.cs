// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Timing;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class PlaybackSettings : ReplayGroup
    {
        protected override string Title => @"playback";

        public IAdjustableClock AdjustableClock { set; get; }

        private readonly ReplaySliderBar<double> sliderbar;

        public PlaybackSettings()
        {
            Child = sliderbar = new ReplaySliderBar<double>
            {
                LabelText = "Playback speed",
                Bindable = new BindableDouble(1)
                {
                    Default = 1,
                    MinValue = 0.5,
                    MaxValue = 2,
                    Precision = 0.01,
                },
            };
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
