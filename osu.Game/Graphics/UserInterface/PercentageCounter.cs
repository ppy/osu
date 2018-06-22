// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Used as an accuracy counter. Represented visually as a percentage.
    /// </summary>
    public class PercentageCounter : RollingCounter<double>
    {
        protected override double RollingDuration => 750;

        private float epsilon => 1e-10f;

        public void SetFraction(float numerator, float denominator)
        {
            Current.Value = Math.Abs(denominator) < epsilon ? 1.0f : numerator / denominator;
        }

        public PercentageCounter()
        {
            DisplayedCountSpriteText.FixedWidth = true;
            Current.Value = DisplayedCount = 1.0f;
        }

        protected override string FormatCount(double count)
        {
            return $@"{count:P2}";
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
