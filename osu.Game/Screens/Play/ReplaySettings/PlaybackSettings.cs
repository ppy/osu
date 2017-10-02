// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class PlaybackSettings : ReplayGroup
    {
        protected override string Title => @"playback";

        private ReplaySliderBar<double> sliderbar;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Child = sliderbar = new ReplaySliderBar<double>
            {
                LabelText = "Playback speed",
                Bindable = config.GetBindable<double>(OsuSetting.PlaybackSpeed),
            };
        }

        public void BindClock(IAdjustableClock clock)
        {
            var clockRate = clock.Rate;
            sliderbar.Bindable.ValueChanged += (rateMultiplier) => clock.Rate = clockRate * rateMultiplier;

            sliderbar.Bindable.Value = 1;
        }
    }
}
