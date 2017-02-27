// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using System;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Used as an accuracy counter. Represented visually as a percentage.
    /// </summary>
    public class PercentageCounter : RollingCounter<float>
    {
        protected override Type TransformType => typeof(TransformAccuracy);

        protected override double RollingDuration => 750;

        private float epsilon => 1e-10f;

        public void SetFraction(float numerator, float denominator)
        {
            Count = Math.Abs(denominator) < epsilon ? 1.0f : numerator / denominator;
        }

        public PercentageCounter()
        {
            DisplayedCountSpriteText.FixedWidth = true;
            Count = 1.0f;
        }

        protected override string FormatCount(float count)
        {
            return $@"{count:P2}";
        }

        protected override double GetProportionalDuration(float currentValue, float newValue)
        {
            return Math.Abs(currentValue - newValue) * RollingDuration * 100.0f;
        }

        public override void Increment(float amount)
        {
            Count = Count + amount;
        }

        protected class TransformAccuracy : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as PercentageCounter).DisplayedCount = CurrentValue;
            }
        }
    }
}
