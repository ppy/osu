// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuDifficultyHitObject : DifficultyHitObject
    {
        private const int normalized_radius = 50; // Change radius to 50 to make 100 the diameter. Easier for mental maths.
        private const int min_delta_time = 25;
        private const float minimum_slider_radius = normalized_radius * 2.4f;

        protected new OsuHitObject BaseObject => (OsuHitObject)base.BaseObject;

        /// <summary>
        /// Normalized distance from the end position of the previous <see cref="OsuDifficultyHitObject"/> to the start position of this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double JumpDistance { get; private set; }

        /// <summary>
        /// Minimum distance from the end position of the previous <see cref="OsuDifficultyHitObject"/> to the start position of this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double MovementDistance { get; private set; }

        /// <summary>
        /// Normalized distance between the start and end position of the previous <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double TravelDistance { get; private set; }

        /// <summary>
        /// Angle the player has to take to hit this <see cref="OsuDifficultyHitObject"/>.
        /// Calculated as the angle between the circles (current-2, current-1, current).
        /// </summary>
        public double? Angle { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the end time of the previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public double MovementTime { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="OsuDifficultyHitObject"/> to the end time of the same previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public double TravelTime { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public readonly double StrainTime;

        private readonly OsuHitObject lastLastObject;
        private readonly OsuHitObject lastObject;

        public OsuDifficultyHitObject(HitObject hitObject, HitObject lastLastObject, HitObject lastObject, double clockRate)
            : base(hitObject, lastObject, clockRate)
        {
            this.lastLastObject = (OsuHitObject)lastLastObject;
            this.lastObject = (OsuHitObject)lastObject;

            // Capped to 25ms to prevent difficulty calculation breaking from simultaneous objects.
            StrainTime = Math.Max(DeltaTime, min_delta_time);

            setDistances(clockRate);
        }

        private void setDistances(double clockRate)
        {
            // We don't need to calculate either angle or distance when one of the last->curr objects is a spinner
            if (BaseObject is Spinner || lastObject is Spinner)
                return;

            // We will scale distances by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = normalized_radius / (float)BaseObject.Radius;

            if (BaseObject.Radius < 30)
            {
                float smallCircleBonus = Math.Min(30 - (float)BaseObject.Radius, 5) / 50;
                scalingFactor *= 1 + smallCircleBonus;
            }

            Vector2 lastCursorPosition = getEndCursorPosition(lastObject);
            JumpDistance = (BaseObject.StackedPosition * scalingFactor - lastCursorPosition * scalingFactor).Length;

            if (lastObject is Slider lastSlider)
            {
                computeSliderCursorPosition(lastSlider);
                TravelDistance = 0;
                TravelTime = Math.Max(lastSlider.LazyTravelTime / clockRate, min_delta_time);
                MovementTime = Math.Max(StrainTime - TravelTime, min_delta_time);
                MovementDistance = Vector2.Subtract(lastSlider.TailCircle.StackedPosition, BaseObject.StackedPosition).Length * scalingFactor;

                int repeatCount = 0;

                Vector2 currSliderPosition = ((OsuHitObject)lastSlider.NestedHitObjects[0]).StackedPosition;

                for (int i = 1; i < lastSlider.NestedHitObjects.Count; i++)
                {
                    var currSliderObj = (OsuHitObject)lastSlider.NestedHitObjects[i];

                    Vector2 currSlider = Vector2.Subtract(currSliderObj.StackedPosition, currSliderPosition);
                    double currSliderLength = currSlider.Length * scalingFactor;

                    if (currSliderObj is SliderEndCircle && !(currSliderObj is SliderRepeat))
                    {
                        Vector2 lazySlider = Vector2.Subtract((Vector2)lastSlider.LazyEndPosition, currSliderPosition);
                        if (lazySlider.Length < currSlider.Length)
                            currSlider = lazySlider; // Take the least distance from slider end vs lazy end.

                        currSliderLength = currSlider.Length * scalingFactor;
                    }

                    if (currSliderObj is SliderTick)
                    {
                        if (currSliderLength > minimum_slider_radius) // minimum_slider_radius is used here as 120 = 2.4 * radius, which means that the cursor assumes the position of least movement required to reach the active tick window.
                        {
                            currSliderPosition = Vector2.Add(currSliderPosition, Vector2.Multiply(currSlider, (float)((currSliderLength - minimum_slider_radius) / currSliderLength)));
                            currSliderLength *= (currSliderLength - minimum_slider_radius) / currSliderLength;
                        }
                        else
                            currSliderLength = 0;
                    }
                    else if (currSliderObj is SliderRepeat)
                    {
                        if (currSliderLength > normalized_radius) // normalized_radius is used here as 50 = radius. This is a way to reward motion of back and forths sliders where we assume the player moves to atleast the rim of the hitcircle.
                        {
                            currSliderPosition = Vector2.Add(currSliderPosition, Vector2.Multiply(currSlider, (float)((currSliderLength - normalized_radius) / currSliderLength)));
                            currSliderLength *= (currSliderLength - normalized_radius) / currSliderLength;
                        }
                        else
                            currSliderLength = 0;
                    }
                    else
                    {
                        currSliderPosition = Vector2.Add(currSliderPosition, currSlider);
                    }

                    if (currSliderObj is SliderRepeat)
                        repeatCount++;

                    TravelDistance += currSliderLength;
                }

                TravelDistance *= Math.Pow(1 + repeatCount / 2.5, 1.0 / 2.5); // Bonus for repeat sliders until a better per nested object strain system can be achieved.

                // Jump distance from the slider tail to the next object, as opposed to the lazy position of JumpDistance.
                float tailJumpDistance = Vector2.Subtract(lastSlider.TailCircle.StackedPosition, BaseObject.StackedPosition).Length * scalingFactor;

                // For hitobjects which continue in the direction of the slider, the player will normally follow through the slider,
                // such that they're not jumping from the lazy position but rather from very close to (or the end of) the slider.
                // In such cases, a leniency is applied by also considering the jump distance from the tail of the slider, and taking the minimum jump distance.
                // Additional distance is removed based on position of jump relative to slider follow circle radius.
                // JumpDistance is normalized_radius because lazyCursorPos uses a tighter 1.4 followCircle. tailJumpDistance is minimum_slider_radius since the full distance of radial leniency is still possible.
                MovementDistance = Math.Max(0, Math.Min(JumpDistance - normalized_radius, tailJumpDistance - 120));
            }
            else
            {
                MovementTime = StrainTime;
                MovementDistance = JumpDistance;
            }

            if (lastLastObject != null && !(lastLastObject is Spinner))
            {
                Vector2 lastLastCursorPosition = getEndCursorPosition(lastLastObject);

                Vector2 v1 = lastLastCursorPosition - lastObject.StackedPosition;
                Vector2 v2 = BaseObject.StackedPosition - lastCursorPosition;

                float dot = Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                Angle = Math.Abs(Math.Atan2(det, dot));
            }
        }

        private void computeSliderCursorPosition(Slider slider)
        {
            if (slider.LazyEndPosition != null)
                return;

            slider.LazyEndPosition = slider.StackedPosition;

            float approxFollowCircleRadius = (float)(slider.Radius * 1.4); // using 1.4 to better follow the real movement of a cursor.
            var computeVertex = new Action<double>(t =>
            {
                double progress = (t - slider.StartTime) / slider.SpanDuration;
                if (progress % 2 >= 1)
                    progress = 1 - progress % 1;
                else
                    progress %= 1;

                // ReSharper disable once PossibleInvalidOperationException (bugged in current r# version)
                var diff = slider.StackedPosition + slider.Path.PositionAt(progress) - slider.LazyEndPosition.Value;
                float dist = diff.Length;

                slider.LazyTravelTime = t - slider.StartTime;

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
            foreach (double time in scoringTimes)
                computeVertex(time);
        }

        private Vector2 getEndCursorPosition(OsuHitObject hitObject)
        {
            Vector2 pos = hitObject.StackedPosition;

            if (hitObject is Slider slider)
            {
                computeSliderCursorPosition(slider);
                pos = slider.LazyEndPosition ?? pos;
            }

            return pos;
        }
    }
}
