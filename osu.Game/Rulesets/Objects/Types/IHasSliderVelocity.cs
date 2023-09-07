// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a slider velocity multiplier.
    /// </summary>
    public interface IHasSliderVelocity
    {
        /// <summary>
        /// The slider velocity multiplier.
        /// </summary>
        double SliderVelocityMultiplier { get; set; }

        BindableNumber<double> SliderVelocityMultiplierBindable { get; }

        /// <summary>
        /// Introduces floating-point errors for rulesets that depend on it.
        /// </summary>
        public double GetPrecisionAdjustedSliderVelocityMultiplier(string rulesetShortName)
        {
            double beatLength = -100 / SliderVelocityMultiplier;

            switch (rulesetShortName)
            {
                case "taiko":
                case "mania":
                    return 1 / (beatLength < 0 ? Math.Clamp((float)-beatLength, 10, 10000) / 100.0 : 1);

                default:
                    return 1 / (beatLength < 0 ? Math.Clamp((float)-beatLength, 10, 1000) / 100.0 : 1);
            }
        }
    }
}
