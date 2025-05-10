// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
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

        /// <summary>
        /// Calculates scale from a CS value, with an optional fudge that was historically applied to the osu! ruleset.
        /// </summary>
        public static float CalculateScaleFromCircleSize(float circleSize, bool applyFudge = false)
        {
            // The following comment is copied verbatim from osu-stable:
            //
            //   Builds of osu! up to 2013-05-04 had the gamefield being rounded down, which caused incorrect radius calculations
            //   in widescreen cases. This ratio adjusts to allow for old replays to work post-fix, which in turn increases the lenience
            //   for all plays, but by an amount so small it should only be effective in replays.
            //
            // To match expectations of gameplay we need to apply this multiplier to circle scale. It's weird but is what it is.
            // It works out to under 1 game pixel and is generally not meaningful to gameplay, but is to replay playback accuracy.
            const float broken_gamefield_rounding_allowance = 1.00041f;

            return (float)(1.0f - 0.7f * IBeatmapDifficultyInfo.DifficultyRange(circleSize)) / 2 * (applyFudge ? broken_gamefield_rounding_allowance : 1);
        }

        public static int CalculateDifficultyPeppyStars(BeatmapDifficulty difficulty, int objectCount, int drainLength)
        {
            /*
             * WARNING: DO NOT TOUCH IF YOU DO NOT KNOW WHAT YOU ARE DOING
             *
             * It so happens that in stable, due to .NET Framework internals, float math would be performed
             * using x87 registers and opcodes.
             * .NET (Core) however uses SSE instructions on 32- and 64-bit words.
             * x87 registers are _80 bits_ wide. Which is notably wider than _both_ float and double.
             * Therefore, on a significant number of beatmaps, the rounding would not produce correct values.
             *
             * Thus, to crudely - but, seemingly *mostly* accurately, after checking across all ranked maps - emulate this,
             * use `decimal`, which is slow, but has bigger precision than `double`.
             * At the time of writing, there is _one_ ranked exception to this - namely https://osu.ppy.sh/beatmapsets/1156087#osu/2625853 -
             * but it is considered an "acceptable casualty", since in that case scores aren't inflated by _that_ much compared to others.
             */

            decimal objectToDrainRatio = drainLength != 0
                ? Math.Clamp((decimal)objectCount / drainLength * 8, 0, 16)
                : 16;

            /*
             * Notably, THE `double` CASTS BELOW ARE IMPORTANT AND MUST REMAIN.
             * Their goal is to trick the compiler / runtime into NOT promoting from single-precision float, as doing so would prompt it
             * to attempt to "silently" fix the single-precision values when converting to decimal,
             * which is NOT what the x87 FPU does.
             */

            decimal drainRate = (decimal)(double)difficulty.DrainRate;
            decimal overallDifficulty = (decimal)(double)difficulty.OverallDifficulty;
            decimal circleSize = (decimal)(double)difficulty.CircleSize;

            return (int)Math.Round((drainRate + overallDifficulty + circleSize + objectToDrainRatio) / 38 * 5);
        }
    }
}
