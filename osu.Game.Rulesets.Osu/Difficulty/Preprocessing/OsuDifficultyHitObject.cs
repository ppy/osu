// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    /// <summary>
    /// A wrapper around <see cref="OsuHitObject"/> extending it with additional data required for difficulty calculation.
    /// </summary>
    public class OsuDifficultyHitObject
    {
        private const int normalized_radius = 52;

        /// <summary>
        /// The <see cref="OsuHitObject"/> this <see cref="OsuDifficultyHitObject"/> refers to.
        /// </summary>
        public OsuHitObject BaseObject { get; }

        /// <summary>
        /// Normalized distance from the <see cref="OsuHitObject.StackedPosition"/> of the previous <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double Distance { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the StartTime of the previous <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double DeltaTime { get; private set; }

        private readonly OsuHitObject lastObject;
        private readonly double timeRate;

        /// <summary>
        /// Initializes the object calculating extra data required for difficulty calculation.
        /// </summary>
        public OsuDifficultyHitObject(OsuHitObject currentObject, OsuHitObject lastObject, double timeRate)
        {
            this.lastObject = lastObject;
            this.timeRate = timeRate;

            BaseObject = currentObject;

            setDistances();
            setTimingValues();
            // Calculate angle here
        }

        private void setDistances()
        {
            // We will scale distances by this factor, so we can assume a uniform CircleSize among beatmaps.
            double scalingFactor = normalized_radius / BaseObject.Radius;
            if (BaseObject.Radius < 30)
            {
                double smallCircleBonus = Math.Min(30 - BaseObject.Radius, 5) / 50;
                scalingFactor *= 1 + smallCircleBonus;
            }

            Vector2 lastCursorPosition = lastObject.StackedPosition;
            float lastTravelDistance = 0;

            var lastSlider = lastObject as Slider;
            if (lastSlider != null)
            {
                computeSliderCursorPosition(lastSlider);
                lastCursorPosition = lastSlider.LazyEndPosition ?? lastCursorPosition;
                lastTravelDistance = lastSlider.LazyTravelDistance;
            }

            Distance = (lastTravelDistance + (BaseObject.StackedPosition - lastCursorPosition).Length) * scalingFactor;
        }

        private void setTimingValues()
        {
            // Every timing inverval is hard capped at the equivalent of 375 BPM streaming speed as a safety measure.
            DeltaTime = Math.Max(50, (BaseObject.StartTime - lastObject.StartTime) / timeRate);
        }

        private void computeSliderCursorPosition(Slider slider)
        {
            if (slider.LazyEndPosition != null)
                return;
            slider.LazyEndPosition = slider.StackedPosition;

            float approxFollowCircleRadius = (float)(slider.Radius * 3);
            var computeVertex = new Action<double>(t =>
            {
                // ReSharper disable once PossibleInvalidOperationException (bugged in current r# version)
                var diff = slider.StackedPositionAt(t) - slider.LazyEndPosition.Value;
                float dist = diff.Length;

                if (dist > approxFollowCircleRadius)
                {
                    // The cursor would be outside the follow circle, we need to move it
                    diff.Normalize(); // Obtain direction of diff
                    dist -= approxFollowCircleRadius;
                    slider.LazyEndPosition += diff * dist;
                    slider.LazyTravelDistance += dist;
                }
            });

            // Skip the head circle
            var scoringTimes = slider.NestedHitObjects.Skip(1).Select(t => t.StartTime);
            foreach (var time in scoringTimes)
                computeVertex(time);
            computeVertex(slider.EndTime);
        }
    }
}
