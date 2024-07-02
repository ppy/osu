// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.ObjectExtensions;
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
        private const double direction_change_bonus = 21.0;

        protected override double SkillMultiplier => 900;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected override int SectionLength => 750;

        protected readonly float HalfCatcherWidth;

        private float? lastPlayerPosition;
        private float lastDistanceMoved;
        private double lastStrainTime;

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

            lastPlayerPosition ??= catchCurrent.NormalizedPositionLast0;

            float playerPosition = Math.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float groupness = getGroupness(catchCurrent);

            // If 3 objects are in group - player will try to cheese them
            if (groupness > 0)
            {
                // Coordinates of the group
                float leftCoords = Math.Min(Math.Min(catchCurrent.NormalizedPosition, catchCurrent.NormalizedPositionLast0), (float)catchCurrent.NormalizedPositionLast1!);
                float rightCoords = Math.Max(Math.Max(catchCurrent.NormalizedPosition, catchCurrent.NormalizedPositionLast0), (float)catchCurrent.NormalizedPositionLast1!);
                float centerCoords = (leftCoords + rightCoords) / 2;

                // Distance from player to the boundaries of group
                float leftDelta = Math.Abs(lastPlayerPosition.Value - leftCoords);
                float rightDelta = Math.Abs(lastPlayerPosition.Value - rightCoords);

                // Normalized deltas:
                leftDelta = normalizeDelta(leftDelta, normalized_hitobject_radius);
                rightDelta = normalizeDelta(rightDelta, normalized_hitobject_radius);

                // Determines how close player is to position where they can hit it
                float closetness = 1;
                closetness *= 1 - leftDelta;
                closetness *= 1 - rightDelta;

                // Player will be more likely to move if it's far away from center of group
                float resultPosition = float.Lerp(centerCoords, lastPlayerPosition.Value, closetness);

                // Assume that player will take the easier route
                if (Math.Abs(playerPosition - lastPlayerPosition.Value) < Math.Abs(resultPosition - lastPlayerPosition.Value))
                    resultPosition = playerPosition;

                // Player will be more likely to move to group center if objects are close enough
                playerPosition = float.Lerp(playerPosition, resultPosition, groupness);
            }

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime + 13 + (3 / catcherSpeedMultiplier);

            double distanceAddition = (Math.Pow(Math.Abs(distanceMoved), 1.3) / 510);
            double sqrtStrain = Math.Sqrt(weightedStrainTime);

            double edgeDashBonus = 0;

            // Direction change bonus.
            if (Math.Abs(distanceMoved) > 0.1)
            {
                if (Math.Abs(lastDistanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved))
                {
                    double bonusFactor = Math.Min(50, Math.Abs(distanceMoved)) / 50;
                    double antiflowFactor = Math.Max(Math.Min(70, Math.Abs(lastDistanceMoved)) / 70, 0.38);

                    distanceAddition += direction_change_bonus / Math.Sqrt(lastStrainTime + 16) * bonusFactor * antiflowFactor * Math.Max(1 - Math.Pow(weightedStrainTime / 1000, 3), 0);
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 12.5 * Math.Min(Math.Abs(distanceMoved), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Bonus for edge dashes.
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 5.7;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20) * Math.Pow((Math.Min(catchCurrent.StrainTime * catcherSpeedMultiplier, 265) / 265), 1.5); // Edge Dashes are easier at lower ms values
            }

            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;
            lastStrainTime = catchCurrent.StrainTime;

            return distanceAddition / weightedStrainTime;
        }

        private float getGroupness(CatchDifficultyHitObject obj)
        {
            if (obj.NormalizedPositionLast1.IsNull())
                return 0;

            // Raw deltas between objects
            float delta01 = Math.Abs(obj.NormalizedPosition - obj.NormalizedPositionLast0);
            float delta02 = Math.Abs((float)(obj.NormalizedPosition - obj.NormalizedPositionLast1));
            float delta12 = Math.Abs((float)(obj.NormalizedPositionLast0 - obj.NormalizedPositionLast1));

            // Normalized deltas:
            // 0 - very close, inbetween - moderately close, 1 - too far
            delta01 = normalizeDelta(delta01, 2 * normalized_hitobject_radius);
            delta02 = normalizeDelta(delta02, 2 * normalized_hitobject_radius);
            delta12 = normalizeDelta(delta12, 2 * normalized_hitobject_radius);

            // If at least one is far apart - groupness is 0
            float groupness = 1;
            groupness *= 1 - delta01;
            groupness *= 1 - delta02;
            groupness *= 1 - delta12;

            return groupness;
        }

        /// <summary>
        /// Normalizing delta:
        /// 0 - very close, inbetween - moderately close, 1 - too far
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="shift">Deltas lower than shift will be 0</param>
        /// <returns></returns>
        private float normalizeDelta(float delta, float shift) => Math.Clamp(delta - shift + absolute_player_positioning_error, 0, absolute_player_positioning_error) / absolute_player_positioning_error;
    }
}
