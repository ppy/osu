// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Timing;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class PlaybackSettings : ReplayGroup
    {
        protected override string Title => @"playback";

        private readonly ReplaySliderBar<double> sliderbar;

        private readonly BindableNumber<double> current;

        public PlaybackSettings()
        {
            current = new BindableDouble(1) as BindableNumber<double>;
            current.MinValue = 0.5;
            current.MaxValue = 2;

            Child = sliderbar = new ReplaySliderBar<double>
            {
                LabelText = "Playback speed",
                Bindable = current,
            };
        }

        public void BindClock(IAdjustableClock clock)
        {
            var clockRate = clock.Rate;
            sliderbar.Bindable.ValueChanged += (rateMultiplier) => clock.Rate = clockRate * rateMultiplier;
        }
    }
}
