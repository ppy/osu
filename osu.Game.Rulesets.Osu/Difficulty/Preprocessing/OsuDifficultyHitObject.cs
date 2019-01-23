// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

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
        /// Normalized distance from the end position of the previous <see cref="OsuDifficultyHitObject"/> to the start position of this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double JumpDistance { get; private set; }

        /// <summary>
        /// Normalized distance between the start and end position of the previous <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double TravelDistance { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the StartTime of the previous <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double DeltaTime { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 50ms.
        /// </summary>
        public double StrainTime { get; private set; }

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
            float scalingFactor = normalized_radius / (float)BaseObject.Radius;
            if (BaseObject.Radius < 30)
            {
                float smallCircleBonus = Math.Min(30 - (float)BaseObject.Radius, 5) / 50;
                scalingFactor *= 1 + smallCircleBonus;
            }

            Vector2 lastCursorPosition = lastObject.StackedPosition;

            var lastSlider = lastObject as Slider;
            if (lastSlider != null)
            {
                computeSliderCursorPosition(lastSlider);
                lastCursorPosition = lastSlider.LazyEndPosition ?? lastCursorPosition;

                TravelDistance = lastSlider.LazyTravelDistance * scalingFactor;
            }

            // Don't need to jump to reach spinners
            if (!(BaseObject is Spinner))
                JumpDistance = (BaseObject.StackedPosition * scalingFactor - lastCursorPosition * scalingFactor).Length;
        }

        private void setTimingValues()
        {
            DeltaTime = (BaseObject.StartTime - lastObject.StartTime) / timeRate;

            // Every strain interval is hard capped at the equivalent of 375 BPM streaming speed as a safety measure
            StrainTime = Math.Max(50, DeltaTime);
        }

        private void computeSliderCursorPosition(Slider slider)
        {
            if (slider.LazyEndPosition != null)
                return;
            slider.LazyEndPosition = slider.StackedPosition;

            float approxFollowCircleRadius = (float)(slider.Radius * 3);
            var computeVertex = new Action<double>(t =>
            {
                double progress = (t - slider.StartTime) / slider.SpanDuration;
                if (progress % 2 >= 1)
                    progress = 1 - progress % 1;
                else
                    progress = progress % 1;

                // ReSharper disable once PossibleInvalidOperationException (bugged in current r# version)
                var diff = slider.StackedPosition + slider.Path.PositionAt(progress) - slider.LazyEndPosition.Value;
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
