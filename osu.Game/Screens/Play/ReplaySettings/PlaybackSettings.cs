// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Timing;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class PlaybackSettings : ReplayGroup
    {
        protected override string Title => @"playback";

        private IAdjustableClock adjustableClock;
        public IAdjustableClock AdjustableClock
        {
            set { adjustableClock = value; }
            get { return adjustableClock; }
        }

        private readonly ReplaySliderBar<double> sliderbar;

        public PlaybackSettings()
        {
            Child = sliderbar = new ReplaySliderBar<double>
            {
                LabelText = "Playback speed",
                Bindable = new BindableDouble(1)
                {
                    MinValue = 0.5,
                    MaxValue = 2
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (adjustableClock != null)
            {
                var clockRate = adjustableClock.Rate;
                sliderbar.Bindable.ValueChanged += rateMultiplier => adjustableClock.Rate = clockRate * rateMultiplier;
            }
        }
    }
}
