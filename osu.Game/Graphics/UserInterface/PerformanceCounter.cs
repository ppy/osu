// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;

namespace osu.Game.Graphics.UserInterface
{
    public class PerformanceCounter : RollingCounter<double>
    {
        protected override double RollingDuration => 250;

        public PerformanceCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours) => AccentColour = colours.BlueLighter;

        protected override string FormatCount(double count)
        {
            return $@"{Math.Round(count, 2)}pp";
        }

        protected override double GetProportionalDuration(double currentValue, double newValue)
        {
            return Math.Abs(currentValue - newValue) * RollingDuration * 100.0f;
        }

        public override void Increment(double amount)
        {
            Current.Value = Current + amount;
        }
    }
}
