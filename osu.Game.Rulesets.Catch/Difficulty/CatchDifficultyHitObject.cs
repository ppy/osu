using System;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    class CatchDifficultyHitObject
    {
        internal static readonly double DECAY_BASE = 0.20;
        private const float NORMALIZED_HITOBJECT_RADIUS = 41.0f;
        private const float ABSOLUTE_PLAYER_POSITIONING_ERROR = 16f;
        private float playerPositioningError;

        internal CatchHitObject BaseHitObject;

        /// <summary>
        /// Measures jump difficulty. CtB doesn't have something like button pressing speed or accuracy
        /// </summary>
        internal double Strain = 1;

        /// <summary>
        /// This is required to keep track of lazy player movement (always moving only as far as necessary)
        /// Without this quick repeat sliders / weirdly shaped streams might become ridiculously overrated
        /// </summary>
        internal float PlayerPositionOffset;
        internal float LastMovement;

        internal float NormalizedPosition;
        internal float ActualNormalizedPosition => NormalizedPosition + PlayerPositionOffset;

        internal CatchDifficultyHitObject(CatchHitObject baseHitObject, float catcherWidthHalf)
        {
            BaseHitObject = baseHitObject;

            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = NORMALIZED_HITOBJECT_RADIUS / catcherWidthHalf;

            playerPositioningError = ABSOLUTE_PLAYER_POSITIONING_ERROR; // * scalingFactor;
            NormalizedPosition = baseHitObject.X * CatchPlayfield.BASE_WIDTH * scalingFactor;
        }

        private const double DIRECTION_CHANGE_BONUS = 12.5;
        internal void CalculateStrains(CatchDifficultyHitObject previousHitObject, double timeRate)
        {
            // Rather simple, but more specialized things are inherently inaccurate due to the big difference playstyles and opinions make.
            // See Taiko feedback thread.
            double timeElapsed = (BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime) / timeRate;
            double decay = Math.Pow(DECAY_BASE, timeElapsed / 1000);

            // Update new position with lazy movement.
            PlayerPositionOffset =
                MathHelper.Clamp(
                    previousHitObject.ActualNormalizedPosition,
                    NormalizedPosition - (NORMALIZED_HITOBJECT_RADIUS - playerPositioningError),
                    NormalizedPosition + (NORMALIZED_HITOBJECT_RADIUS - playerPositioningError)) // Obtain new lazy position, but be stricter by allowing for an error of a certain degree of the player.
                - NormalizedPosition; // Subtract HitObject position to obtain offset

            LastMovement = DistanceTo(previousHitObject);
            double addition = spacingWeight(LastMovement);

            if (NormalizedPosition < previousHitObject.NormalizedPosition)
            {
                LastMovement = -LastMovement;
            }

            CatchHitObject previousHitCircle = previousHitObject.BaseHitObject;

            double additionBonus = 0;
            double sqrtTime = Math.Sqrt(Math.Max(timeElapsed, 25));

            // Direction changes give an extra point!
            if (Math.Abs(LastMovement) > 0.1)
            {
                if (Math.Abs(previousHitObject.LastMovement) > 0.1 && Math.Sign(LastMovement) != Math.Sign(previousHitObject.LastMovement))
                {
                    double bonus = DIRECTION_CHANGE_BONUS / sqrtTime;

                    // Weight bonus by how 
                    double bonusFactor = Math.Min(playerPositioningError, Math.Abs(LastMovement)) / playerPositioningError;

                    // We want time to play a role twice here!
                    addition += bonus * bonusFactor;

                    // Bonus for tougher direction switches and "almost" hyperdashes at this point
                    if (previousHitCircle != null && previousHitCircle.DistanceToHyperDash <= 10.0f / CatchPlayfield.BASE_WIDTH)
                    {
                        additionBonus += 0.3 * bonusFactor;
                    }
                }

                // Base bonus for every movement, giving some weight to streams.
                addition += 7.5 * Math.Min(Math.Abs(LastMovement), NORMALIZED_HITOBJECT_RADIUS * 2) / (NORMALIZED_HITOBJECT_RADIUS * 6) / sqrtTime;
            }

            // Bonus for "almost" hyperdashes at corner points
            if (previousHitCircle != null && previousHitCircle.DistanceToHyperDash <= 10.0f / CatchPlayfield.BASE_WIDTH)
            {
                if (!previousHitCircle.HyperDash)
                {
                    additionBonus += 1.0;
                }
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    PlayerPositionOffset = 0;
                }

                addition *= 1.0 + additionBonus * ((10 - previousHitCircle.DistanceToHyperDash * CatchPlayfield.BASE_WIDTH) / 10);
            }

            addition *= 850.0 / Math.Max(timeElapsed, 25);

            Strain = previousHitObject.Strain * decay + addition;
        }

        private static double spacingWeight(float distance)
        {
            return Math.Pow(distance, 1.3) / 500;
        }

        internal float DistanceTo(CatchDifficultyHitObject other)
        {
            return Math.Abs(ActualNormalizedPosition - other.ActualNormalizedPosition);
        }
    }
}
