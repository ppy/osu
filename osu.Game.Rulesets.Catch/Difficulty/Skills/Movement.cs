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

        protected override double SkillMultiplier => 135;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected override int SectionLength => 750;

        protected readonly float HalfCatcherWidth;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private bool previousIsDoubleHdash;

        public int DirectionChangeCount;

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

            double movementValue;

            // Counting direction changes for length scaling in the performance calculation
            if (Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved) && Math.Sign(lastDistanceMoved) != 0 && Math.Abs(distanceMoved) > 0.1)
            {
                DirectionChangeCount += 1;
            }

            // Separating HyperDashes and basic fruits calculations

            // Basic fruit calculation
            if (!catchCurrent.LastObject.HyperDash)
            {
                // The base value is a ratio between distance moved and strain time
                // For basic fruits, the distance is usually slightly more important for the calculation than the strain time
                movementValue = 0.06 * Math.Pow(Math.Abs(distanceMoved) / (weightedStrainTime * catcherSpeedMultiplier), 2);
                movementValue /= Math.Max(1, HalfCatcherWidth / Math.Abs(distanceMoved)); // Nerfes streams (shortest movements)

                // Gives a slight buff to direction changes
                if (Math.Abs(distanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved) && Math.Sign(lastDistanceMoved) != 0)
                {
                    movementValue *= 1 + (1 / (0.008 * Math.Abs(distanceMoved) / Math.Pow(weightedStrainTime, 0.2)));
                }
            }
            else
            {
                // For Hyperdashes, the distance is usually way more important for the calculation than the strain time 
                movementValue = 0.91 * Math.Log(Math.Abs(distanceMoved) + 1, 2) / Math.Pow(weightedStrainTime, 0.85);

                // Handling complex hyperdash chains
                if (previousIsDoubleHdash)
                {
                    if (isDoubleHdash) // HDash chains, nerf if same direction, buff otherwise
                        movementValue *= Math.Sign(distanceMoved) == Math.Sign(lastDistanceMoved) ? 0.2 : 2;
                }
                else if (isDoubleHdash) // Nerf simple double HDashes
                    movementValue *= 0.2;
            }

            // Edge dashes buff
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 5.1;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                movementValue *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20) * Math.Pow((Math.Min(catchCurrent.StrainTime, 265) / 265), 1.5) / catcherSpeedMultiplier; // Edge Dashes are easier at lower ms values
            }

            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;
            previousIsDoubleHdash = isDoubleHdash;

            return movementValue;
        }
    }
}
