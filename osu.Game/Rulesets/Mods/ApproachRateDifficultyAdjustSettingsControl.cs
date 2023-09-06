// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Rulesets.Mods
{
    public partial class ApproachRateDifficultyAdjustSettingsControl : DifficultyAdjustSettingsControl
    {
        protected override Drawable CreateControl() => new SliderControl(sliderDisplayCurrent,
            new ApproachRateSlider
            {
                RelativeSizeAxes = Axes.X,
                Current = sliderDisplayCurrent,
                KeyboardStep = 0.1f,
            }
        );

        /// <summary>
        /// A slider bar with more detailed approach rate info for its given value
        /// </summary>
        public partial class ApproachRateSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText =>
                $"{base.TooltipText} ({millisecondsFromApproachRate(Current.Value, 1.0f)} ms)";

            private double millisecondsFromApproachRate(float value, float clockRate)
            {
                return Math.Round(1800 - Math.Min(value, 5) * 120 - (value >= 5 ? (value - 5) * 150 : 0) / clockRate);
            }
        }
    }
}
