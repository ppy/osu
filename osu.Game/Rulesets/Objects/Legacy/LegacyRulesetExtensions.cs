// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy
{
    public static class LegacyRulesetExtensions
    {
        /// <summary>
        /// Introduces floating-point errors to post-multiplied beat length for legacy rulesets that depend on it.
        /// You should definitely not use this unless you know exactly what you're doing.
        /// </summary>
        public static double GetPrecisionAdjustedBeatLength(IHasSliderVelocity hasSliderVelocity, TimingControlPoint timingControlPoint, string rulesetShortName)
        {
            double sliderVelocityAsBeatLength = -100 / hasSliderVelocity.SliderVelocityMultiplier;

            // Note: In stable, the division occurs on floats, but with compiler optimisations turned on actually seems to occur on doubles via some .NET black magic (possibly inlining?).
            double bpmMultiplier;

            switch (rulesetShortName)
            {
                case "taiko":
                case "mania":
                    bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 10000) / 100.0 : 1;
                    break;

                case "osu":
                case "fruits":
                    bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;
                    break;

                default:
                    throw new ArgumentException("Must be a legacy ruleset", nameof(rulesetShortName));
            }

            return timingControlPoint.BeatLength * bpmMultiplier;
        }
    }
}
