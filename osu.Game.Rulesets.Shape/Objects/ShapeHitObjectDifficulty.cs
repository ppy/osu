using OpenTK;
using System;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Rulesets.Shape.Objects
{
    internal class ShapeHitObjectDifficulty
    {
        /// <summary>
        /// Factor by how much speed / aim strain decays per second.
        /// </summary>
        /// <remarks>
        /// These values are results of tweaking a lot and taking into account general feedback.
        /// Opinionated observation: Speed is easier to maintain than accurate jumps.
        /// </remarks>
        internal static readonly double[] DECAY_BASE = { 0.3, 0.15 };

        /// <summary>
        /// Pseudo threshold values to distinguish between "singles" and "streams"
        /// </summary>
        /// <remarks>
        ///  Of course the border can not be defined clearly, therefore the algorithm has a smooth transition between those values.
        ///  They also are based on tweaking and general feedback.
        /// </remarks>
        private const double stream_spacing_threshold = 110,
                             single_spacing_threshold = 125;

        /// <summary>
        /// Scaling values for weightings to keep aim and speed difficulty in balance.
        /// </summary>
        /// <remarks>
        /// Found from testing a very large map pool (containing all ranked maps) and keeping the average values the same.
        /// </remarks>
        private static readonly double[] spacing_weight_scaling = { 1400, 26.25 };

        /// <summary>
        /// Almost the normed diameter of a hitbox (104 osu pixel). That is -after- position transforming.
        /// </summary>
        private const double almost_diameter = 90;

        internal ShapeHitObject BaseHitObject;
        internal double[] Strains = { 1, 1 };

        internal int MaxCombo = 1;

        private float scalingFactor;

        private Vector2 startPosition = new Vector2(0);
        private Vector2 endPosition;

        internal ShapeHitObjectDifficulty(ShapeHitObject baseHitObject)
        {
            BaseHitObject = baseHitObject;
            float hitboxRadius = baseHitObject.Scale * 64;

            // We will scale everything by this factor, so we can assume a uniform HitboxSize among beatmaps.
            scalingFactor = 52.0f / hitboxRadius;
            if (hitboxRadius < 4)
            {
                float smallHitboxBonus = Math.Min(30.0f - hitboxRadius, 5.0f) / 50.0f;
                scalingFactor *= 1.0f + smallHitboxBonus;
            }

            else
                endPosition = startPosition;
        }

        internal void CalculateStrains(ShapeHitObjectDifficulty previousHitObject, double timeRate)
        {
            calculateSpecificStrain(previousHitObject, ShapeDifficultyCalculator.DifficultyType.Speed, timeRate);
            calculateSpecificStrain(previousHitObject, ShapeDifficultyCalculator.DifficultyType.Aim, timeRate);
        }

        // Caution: The subjective values are strong with this one
        private static double spacingWeight(double distance, ShapeDifficultyCalculator.DifficultyType type)
        {
            switch (type)
            {
                case ShapeDifficultyCalculator.DifficultyType.Speed:
                    if (distance > single_spacing_threshold)
                        return 2.5;
                    else if (distance > stream_spacing_threshold)
                        return 1.6 + 0.9 * (distance - stream_spacing_threshold) / (single_spacing_threshold - stream_spacing_threshold);
                    else if (distance > almost_diameter)
                        return 1.2 + 0.4 * (distance - almost_diameter) / (stream_spacing_threshold - almost_diameter);
                    else if (distance > almost_diameter / 2)
                        return 0.95 + 0.25 * (distance - almost_diameter / 2) / (almost_diameter / 2);
                    else
                        return 0.95;

                case ShapeDifficultyCalculator.DifficultyType.Aim:
                    return Math.Pow(distance, 0.99);
            }

            Debug.Assert(false, "Invalid Shape difficulty hit object type.");
            return 0;
        }

        private void calculateSpecificStrain(ShapeHitObjectDifficulty previousHitObject, ShapeDifficultyCalculator.DifficultyType type, double timeRate)
        {
            double addition = 0;
            double timeElapsed = (BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime) / timeRate;
            double decay = Math.Pow(DECAY_BASE[(int)type], timeElapsed / 1000);

            addition = spacingWeight(DistanceTo(previousHitObject), type) * spacing_weight_scaling[(int)type];

            // You will never find maps that require this amongst ranked maps.
            addition /= Math.Max(timeElapsed, 50);

            Strains[(int)type] = previousHitObject.Strains[(int)type] * decay + addition;
        }

        internal double DistanceTo(ShapeHitObjectDifficulty other)
        {
            // Scale the distance by hitbox size.
            return (startPosition - other.endPosition).Length * scalingFactor;
        }
    }
}
