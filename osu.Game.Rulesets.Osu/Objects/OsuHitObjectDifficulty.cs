// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using System;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Objects
{
    internal class OsuHitObjectDifficulty
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
        /// Almost the normed diameter of a circle (104 osu pixel). That is -after- position transforming.
        /// </summary>
        private const double almost_diameter = 90;

        internal OsuHitObject BaseHitObject;
        internal double[] Strains = { 1, 1 };

        internal int MaxCombo = 1;

        private readonly float scalingFactor;
        private float lazySliderLength;

        private readonly Vector2 startPosition;
        private readonly Vector2 endPosition;

        internal OsuHitObjectDifficulty(OsuHitObject baseHitObject)
        {
            BaseHitObject = baseHitObject;
            float circleRadius = baseHitObject.Scale * 64;

            Slider slider = BaseHitObject as Slider;
            if (slider != null)
                MaxCombo += slider.Ticks.Count();

            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            scalingFactor = 52.0f / circleRadius;
            if (circleRadius < 30)
            {
                float smallCircleBonus = Math.Min(30.0f - circleRadius, 5.0f) / 50.0f;
                scalingFactor *= 1.0f + smallCircleBonus;
            }

            lazySliderLength = 0;
            startPosition = baseHitObject.StackedPosition;

            // Calculate approximation of lazy movement on the slider
            if (slider != null)
            {
                float sliderFollowCircleRadius = circleRadius * 3; // Not sure if this is correct, but here we do not need 100% exact values. This comes pretty darn close in my tests.

                // For simplifying this step we use actual osu! coordinates and simply scale the length, that we obtain by the ScalingFactor later
                Vector2 cursorPos = startPosition;

                Action<Vector2> addSliderVertex = delegate (Vector2 pos)
                {
                    Vector2 difference = pos - cursorPos;
                    float distance = difference.Length;

                    // Did we move away too far?
                    if (distance > sliderFollowCircleRadius)
                    {
                        // Yep, we need to move the cursor
                        difference.Normalize(); // Obtain the direction of difference. We do no longer need the actual difference
                        distance -= sliderFollowCircleRadius;
                        cursorPos += difference * distance; // We move the cursor just as far as needed to stay in the follow circle
                        lazySliderLength += distance;
                    }
                };

                // Actual computation of the first lazy curve
                foreach (var tick in slider.Ticks)
                    addSliderVertex(tick.StackedPosition);

                addSliderVertex(baseHitObject.StackedEndPosition);

                lazySliderLength *= scalingFactor;
                endPosition = cursorPos;
            }
            // We have a normal HitCircle or a spinner
            else
                endPosition = startPosition;
        }

        internal void CalculateStrains(OsuHitObjectDifficulty previousHitObject, double timeRate)
        {
            calculateSpecificStrain(previousHitObject, OsuDifficultyCalculator.DifficultyType.Speed, timeRate);
            calculateSpecificStrain(previousHitObject, OsuDifficultyCalculator.DifficultyType.Aim, timeRate);
        }

        // Caution: The subjective values are strong with this one
        private static double spacingWeight(double distance, OsuDifficultyCalculator.DifficultyType type)
        {
            switch (type)
            {
                case OsuDifficultyCalculator.DifficultyType.Speed:
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

                case OsuDifficultyCalculator.DifficultyType.Aim:
                    return Math.Pow(distance, 0.99);
            }

            Debug.Assert(false, "Invalid osu difficulty hit object type.");
            return 0;
        }

        private void calculateSpecificStrain(OsuHitObjectDifficulty previousHitObject, OsuDifficultyCalculator.DifficultyType type, double timeRate)
        {
            double addition = 0;
            double timeElapsed = (BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime) / timeRate;
            double decay = Math.Pow(DECAY_BASE[(int)type], timeElapsed / 1000);

            if (BaseHitObject is Spinner)
            {
                // Do nothing for spinners
            }
            else if (BaseHitObject is Slider)
            {
                switch (type)
                {
                    case OsuDifficultyCalculator.DifficultyType.Speed:

                        // For speed strain we treat the whole slider as a single spacing entity, since "Speed" is about how hard it is to click buttons fast.
                        // The spacing weight exists to differentiate between being able to easily alternate or having to single.
                        addition =
                            spacingWeight(previousHitObject.lazySliderLength +
                                          DistanceTo(previousHitObject), type) *
                            spacing_weight_scaling[(int)type];

                        break;
                    case OsuDifficultyCalculator.DifficultyType.Aim:

                        // For Aim strain we treat each slider segment and the jump after the end of the slider as separate jumps, since movement-wise there is no difference
                        // to multiple jumps.
                        addition =
                            (
                                spacingWeight(previousHitObject.lazySliderLength, type) +
                                spacingWeight(DistanceTo(previousHitObject), type)
                            ) *
                            spacing_weight_scaling[(int)type];

                        break;
                }
            }
            else if (BaseHitObject is HitCircle)
            {
                addition = spacingWeight(DistanceTo(previousHitObject), type) * spacing_weight_scaling[(int)type];
            }

            // Scale addition by the time, that elapsed. Filter out HitObjects that are too close to be played anyway to avoid crazy values by division through close to zero.
            // You will never find maps that require this amongst ranked maps.
            addition /= Math.Max(timeElapsed, 50);

            Strains[(int)type] = previousHitObject.Strains[(int)type] * decay + addition;
        }

        internal double DistanceTo(OsuHitObjectDifficulty other)
        {
            // Scale the distance by circle size.
            return (startPosition - other.endPosition).Length * scalingFactor;
        }
    }
}
