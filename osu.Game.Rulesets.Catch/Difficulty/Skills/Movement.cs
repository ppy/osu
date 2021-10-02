// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : StrainDecaySkill
    {
        private const float absolute_player_positioning_error = 16f;
        private const float normalized_hitobject_radius = 41.0f;

        protected override double SkillMultiplier => 900;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected override int SectionLength => 750;

        protected readonly float HalfCatcherWidth;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private bool previousIsDoubleHdash;

        public double DirectionChangeCount;

        /// <summary>
        /// The speed multiplier applied to the player's catcher.
        /// </summary>
        private readonly double catcherSpeedMultiplier;

        public Movement(Mod[] mods, float halfCatcherWidth, double clockRate)
            : base(mods)
        {
            HalfCatcherWidth = halfCatcherWidth;

            // In catch, clockrate adjustments do not only affect the timings of hitobjects,
            // but also the speed of the player's catcher, which has an impact on difficulty
            // TODO: Support variable clockrates caused by mods such as ModTimeRamp
            //  (perhaps by using IApplicableToRate within the CatchDifficultyHitObject constructor to set a catcher speed for each object before processing)
            catcherSpeedMultiplier = clockRate;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;

            lastPlayerPosition ??= catchCurrent.LastNormalizedPosition;

            float playerPosition = Math.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            bool isDoubleHdash = catchCurrent.LastObject.HyperDash && catchCurrent.BaseObject.HyperDash;

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime / catcherSpeedMultiplier;
            double edgeDashBonus = 0;


            double distanceAddition;

            // Counting direction changes for length scaling in the performance calculation
            if (Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved) && Math.Sign(lastDistanceMoved) != 0 && Math.Abs(distanceMoved) > 4)
            {
                DirectionChangeCount += 1;
            }

            // Separating HyperDashes and basic fruits calculations

            // Basic fruit calculation
            if (!catchCurrent.LastObject.HyperDash)
            {
                // The base value is a ratio between distance moved and strain time
                // The exponents are used to give emphasis on the distance or the strain time
                // For basic fruits, the distance is usually slightly more important for the calculation than the strain time
                distanceAddition = 0.026 * Math.Pow(Math.Abs(distanceMoved), 1.25) / Math.Pow(weightedStrainTime, 0.9);
                distanceAddition /= Math.Max(1, 70 / Math.Abs(distanceMoved)); // Nerfes streams (shortest movements)

                // Gives a slight buff to direction changes
                if (Math.Abs(distanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    distanceAddition *= 1 + 1 / (4 * Math.Abs(distanceMoved / weightedStrainTime));
                }
            }
            else
            {
                // For Hyperdashes, the distance is usually way more important for the calculation than the strain time 
                distanceAddition = 0.87 * Math.Log(Math.Abs(distanceMoved + 1), 2.13) / Math.Pow(weightedStrainTime, 0.85);

                // Handling complex hyperdash chains
                if (previousIsDoubleHdash)
                {
                    if (isDoubleHdash) // HDash chains, nerf is same direction, buff otherwise
                        distanceAddition *= Math.Sign(distanceMoved) == Math.Sign(lastDistanceMoved) ? 0.8 : 1.8;
                }
                else if (isDoubleHdash) // Nerf same direction HDash
                    distanceAddition *= 0.8;

            }

            // Edge dashes buff
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 4.2;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20) * Math.Pow((Math.Min(catchCurrent.StrainTime, 265) / 265), 1.5) / Math.Max(1, catcherSpeedMultiplier); // Edge Dashes are easier at lower ms values
            }


            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;
            previousIsDoubleHdash = isDoubleHdash;

            return distanceAddition * 0.15; // Difficulty scaling (could be handled somewhere else)
        }
    }
}
