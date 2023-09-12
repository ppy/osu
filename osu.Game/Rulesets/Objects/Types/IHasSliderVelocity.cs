// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

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
        /// Introduces floating-point errors to post-multiplied beat length for rulesets that depend on it.
        /// </summary>
        public double GetPrecisionAdjustedBeatLength(TimingControlPoint timingControlPoint, string rulesetShortName)
        {
            double sliderVelocityAsBeatLength = -100 / SliderVelocityMultiplier;

            // Note: In stable, the division occurs on floats, but with compiler optimisations turned on actually seems to occur on doubles via some .NET black magic (possibly inlining?).
            double bpmMultiplier;

            switch (rulesetShortName)
            {
                case "taiko":
                case "mania":
                    bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 10000) / 100.0 : 1;
                    break;

                default:
                    bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;
                    break;
            }

            return timingControlPoint.BeatLength * bpmMultiplier;
        }
    }
}
