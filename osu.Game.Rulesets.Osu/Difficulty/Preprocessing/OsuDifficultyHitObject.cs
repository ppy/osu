// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuDifficultyHitObject : DifficultyHitObject
    {
        /// <summary>
        /// A distance by which all distances should be scaled in order to assume a uniform circle size.
        /// </summary>
        public const int NORMALISED_RADIUS = 50; // Change radius to 50 to make 100 the diameter. Easier for mental maths.

        private const int min_delta_time = 25;
        private const float maximum_slider_radius = NORMALISED_RADIUS * 2.4f;
        private const float assumed_slider_radius = NORMALISED_RADIUS * 1.8f;

        protected new OsuHitObject BaseObject => (OsuHitObject)base.BaseObject;

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public readonly double StrainTime;

        /// <summary>
        /// Saved version of <see cref="OsuHitObject.StackedPosition"/> to decrease overhead.
        /// </summary>
        public readonly Vector2 StackedPosition;

        /// <summary>
        /// Normalised distance from the "lazy" end position of the previous <see cref="OsuDifficultyHitObject"/> to the start position of this <see cref="OsuDifficultyHitObject"/>.
        /// <para>
        /// The "lazy" end position is the position at which the cursor ends up if the previous hitobject is followed with as minimal movement as possible (i.e. on the edge of slider follow circles).
        /// </para>
        /// </summary>
        public double LazyJumpDistance { get; private set; }

        /// <summary>
        /// Normalised shortest distance to consider for a jump between the previous <see cref="OsuDifficultyHitObject"/> and this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        /// <remarks>
        /// This is bounded from above by <see cref="LazyJumpDistance"/>, and is smaller than the former if a more natural path is able to be taken through the previous <see cref="OsuDifficultyHitObject"/>.
        /// </remarks>
        /// <example>
        /// Suppose a linear slider - circle pattern.
        /// <br />
        /// Following the slider lazily (see: <see cref="LazyJumpDistance"/>) will result in underestimating the true end position of the slider as being closer towards the start position.
        /// As a result, <see cref="LazyJumpDistance"/> overestimates the jump distance because the player is able to take a more natural path by following through the slider to its end,
        /// such that the jump is felt as only starting from the slider's true end position.
        /// <br />
        /// Now consider a slider - circle pattern where the circle is stacked along the path inside the slider.
        /// In this case, the lazy end position correctly estimates the true end position of the slider and provides the more natural movement path.
        /// </example>
        public double MinimumJumpDistance { get; private set; }

        /// <summary>
        /// The time taken to travel through <see cref="MinimumJumpDistance"/>, with a minimum value of 25ms.
        /// </summary>
        public double MinimumJumpTime { get; private set; }

        /// <summary>
        /// Normalised distance between the start and end position of this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double TravelDistance { get; private set; }

        /// <summary>
        /// The time taken to travel through <see cref="TravelDistance"/>, with a minimum value of 25ms for <see cref="Slider"/> objects.
        /// </summary>
        public double TravelTime { get; private set; }

        /// <summary>
        /// Absolute angle the player has to take to hit this <see cref="OsuDifficultyHitObject"/>.
        /// Calculated as the angle between the circles (current-2, current-1, current).
        /// Ranges from 0 to PI
        /// </summary>
        public double? Angle { get; private set; }

        /// <summary>
        /// Signed version of the Angle.
        /// Potentially should be used for more accurate angle bonuses
        /// Ranges from -PI to PI
        /// </summary>
        public double? AngleSigned { get; private set; }

        /// <summary>
        /// Retrieves the full hit window for a Great <see cref="HitResult"/>.
        /// </summary>
        public double HitWindowGreat { get; private set; }

        /// <summary>
        /// Density of the object for given preempt. Saved for optimization, density calculation is expensive.
        /// </summary>
        public double Density { get; private set; }

        /// <summary>
        /// Predictabiliy of the angle. Gives high values only in exceptionally repetitive patterns.
        /// </summary>
        public double AnglePredictability { get; private set; }

        /// <summary>
        /// Objects that was visible after the note was hit together with cumulative overlapping difficulty. Saved for optimization to avoid O(x^4) time complexity.
        /// </summary>
        public IList<ReadingObject> ReadingObjects { get; private set; }

        /// <summary>
        /// NON ZERO overlap values for each visible object on the moment this object appeared. Key is <see cref="DifficultyHitObject.Index"/>. Saved for optimization.
        /// </summary>
        public IDictionary<int, double> OverlapValues { get; private set; }

        /// <summary>
        /// Time in ms between appearence of this <see cref="OsuDifficultyHitObject"/> and moment to click on it.
        /// </summary>
        public readonly double Preempt;

        private readonly OsuHitObject? lastLastObject;
        private readonly OsuHitObject lastObject;

        public OsuDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject? lastLastObject, double clockRate, List<DifficultyHitObject> objects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            OsuHitObject currObject = (OsuHitObject)hitObject;
            this.lastObject = (OsuHitObject)lastObject;
            this.lastLastObject = lastLastObject as OsuHitObject;

            StackedPosition = currObject.StackedPosition;
            Preempt = BaseObject.TimePreempt / clockRate;

            // Capped to 25ms to prevent difficulty calculation breaking from simultaneous objects.
            StrainTime = Math.Max(DeltaTime, min_delta_time);

            if (BaseObject is Slider sliderObject)
            {
                HitWindowGreat = 2 * sliderObject.HeadCircle.HitWindows.WindowFor(HitResult.Great) / clockRate;
            }
            else
            {
                HitWindowGreat = 2 * BaseObject.HitWindows.WindowFor(HitResult.Great) / clockRate;
            }

            setDistances(clockRate);

            AnglePredictability = CalculateAnglePredictability();

            (ReadingObjects, OverlapValues) = getReadingObjects();

            Density = ReadingEvaluator.EvaluateDensityOf(this);
        }

        private (IList<ReadingObject>, IDictionary<int, double>) getReadingObjects()
        {
            double totalOverlapnessDifficulty = 0;
            double currentTime = DeltaTime;
            List<double> historicTimes = [];
            List<double> historicAngles = [];

            OsuDifficultyHitObject prevObject = this;

            // The fastest way to do it I've seen so far. Still - one of the slowest parts of the reading calc
            var visibleObjects = retrieveCurrentVisibleObjects(this);

            var readingObjects = new List<ReadingObject>(visibleObjects.Count);
            OverlapValues = new Dictionary<int, double>();

            //foreach (var loopObj in visibleObjects)
            for (int loopIndex = 0; loopIndex < visibleObjects.Count; loopIndex++)
            {
                var loopObj = visibleObjects[loopIndex];

                // Overlapness with this object
                double currentOverlapness = calculateOverlapness(this, loopObj);

                // Save it for future use. Saving only non-zero to make it faster
                if (currentOverlapness > 0) OverlapValues[loopObj.Index] = currentOverlapness;

                if (prevObject.Angle.IsNull())
                {
                    currentTime += prevObject.DeltaTime;
                    continue;
                }

                // Previous angle because order is reversed;
                double angle = (double)prevObject.Angle;

                // Overlapness between current and prev to make streams have 0 buff
                double instantOverlapness = 0;
                prevObject.OverlapValues?.TryGetValue(loopObj.Index, out instantOverlapness);

                // Nerf overlaps on wide angles
                double angleFactor = 1;
                angleFactor += (-Math.Cos(angle) + 1) / 2; // =2 for wide angles, =1 for acute angles
                instantOverlapness = Math.Min(1, (0.5 + instantOverlapness) * angleFactor); // wide angles are more predictable

                currentOverlapness *= (1 - instantOverlapness) * 2; // wide angles will have close-to-zero buff

                // Control overlap repetitivness
                if (currentOverlapness > 0)
                {
                    currentOverlapness *= getOpacitiyMultiplier(loopObj); // Increase stability by using opacity

                    double currentMinOverlapness = currentOverlapness;
                    double cumulativeTimeWithCurrent = currentTime;

                    // For every cumulative time with current
                    for (int i = historicTimes.Count - 1; i >= 0; i--)
                    {
                        double cumulativeTimeWithoutCurrent = 0;

                        // Get every possible cumulative time without current
                        for (int j = i; j >= 0; j--)
                        {
                            cumulativeTimeWithoutCurrent += historicTimes[j];

                            // Check how similar cumulative times are
                            double potentialMinOverlapness = currentOverlapness * getTimeDifference(cumulativeTimeWithCurrent, cumulativeTimeWithoutCurrent);
                            potentialMinOverlapness *= 1 - getAngleSimilarity(angle, historicAngles[j]) * (1 - getTimeDifference(loopObj.StrainTime, prevObject.StrainTime));
                            currentMinOverlapness = Math.Min(currentMinOverlapness, potentialMinOverlapness);

                            // Check how similar current time with cumulative time
                            potentialMinOverlapness = currentOverlapness * getTimeDifference(currentTime, cumulativeTimeWithoutCurrent);
                            potentialMinOverlapness *= 1 - getAngleSimilarity(angle, historicAngles[j]) * (1 - getTimeDifference(loopObj.StrainTime, prevObject.StrainTime));
                            currentMinOverlapness = Math.Min(currentMinOverlapness, potentialMinOverlapness);

                            // Starting from this point - we will never have better match, so stop searching
                            if (cumulativeTimeWithoutCurrent >= cumulativeTimeWithCurrent)
                                break;
                        }
                        cumulativeTimeWithCurrent += historicTimes[i];
                    }

                    currentOverlapness = currentMinOverlapness;

                    historicTimes.Add(currentTime);
                    historicAngles.Add(angle);

                    currentTime = prevObject.DeltaTime;
                }
                else
                {
                    currentTime += prevObject.DeltaTime;
                }

                totalOverlapnessDifficulty += currentOverlapness;

                ReadingObject newObj = new ReadingObject(loopObj, totalOverlapnessDifficulty);
                readingObjects.Add(newObj);
                prevObject = loopObj;
            }

            return (readingObjects, OverlapValues);
        }

        private double getOpacitiyMultiplier(OsuDifficultyHitObject loopObj)
        {
            const double threshold = 0.3;

            // Get raw opacity
            double opacity = OpacityAt(loopObj.BaseObject.StartTime, false);

            opacity = Math.Min(1, opacity + threshold); // object with opacity 0.7 are still perfectly visible
            opacity -= threshold; // return opacity 0 objects back to 0
            opacity /= 1 - threshold; // fix scaling to be 0-1 again
            opacity = Math.Sqrt(opacity); // change curve

            return opacity;
        }

        private static double getTimeDifference(double timeA, double timeB)
        {
            double similarity = Math.Min(timeA, timeB) / Math.Max(timeA, timeB);
            if (Math.Max(timeA, timeB) == 0) similarity = 1;

            if (similarity < 0.75) return 1.0;
            if (similarity > 0.9) return 0.0;

            return (Math.Cos((similarity - 0.75) * Math.PI / 0.15) + 1) / 2; // drops from 1 to 0 as similarity increase from 0.75 to 0.9
        }

        private static double getAngleSimilarity(double angle1, double angle2)
        {
            double difference = Math.Abs(angle1 - angle2);
            double threeshold = Math.PI / 12;

            if (difference > threeshold) return 0;
            return 1 - difference / threeshold;
        }

        private static double calculateOverlapness(OsuDifficultyHitObject odho1, OsuDifficultyHitObject odho2)
        {
            const double area_coef = 0.85;

            double distance = Vector2.Distance(odho1.StackedPosition, odho2.StackedPosition); // Distance func is kinda slow for some reason
            double radius = odho1.BaseObject.Radius;

            double distance_sqr = distance * distance;
            double radius_sqr = radius * radius;

            if (distance > radius * 2)
                return 0;

            double s1 = Math.Acos(distance / (2 * radius)) * radius_sqr; // Area of sector
            double s2 = distance * Math.Sqrt(radius_sqr - distance_sqr / 4) / 2; // Area of triangle

            double overlappingAreaNormalized = (s1 - s2) * 2 / (Math.PI * radius_sqr);

            // don't ask me how i get this value, looks oddly similar to PI - 3
            const double stack_distance_ratio = 0.1414213562373;

            double perfectStackBuff = (stack_distance_ratio - distance / radius) / stack_distance_ratio; // scale from 0 on normal stack to 1 on perfect stack
            perfectStackBuff = Math.Max(perfectStackBuff, 0); // can't be negative

            return overlappingAreaNormalized * area_coef + perfectStackBuff * (1 - area_coef);
        }

        private static List<OsuDifficultyHitObject> retrieveCurrentVisibleObjects(OsuDifficultyHitObject current)
        {

            var visibleObjects = new List<OsuDifficultyHitObject>();

            for (int i = 0; i < current.Count; i++)
            {
                OsuDifficultyHitObject hitObject = (OsuDifficultyHitObject)current.Previous(i);

                if (hitObject.IsNull() ||
                    hitObject.StartTime < current.StartTime - current.Preempt)
                    break;

                visibleObjects.Add(hitObject);
            }

            return visibleObjects;
        }

        public double CalculateAnglePredictability()
        {
            OsuDifficultyHitObject? prevObj0 = (OsuDifficultyHitObject?)Previous(0);
            OsuDifficultyHitObject? prevObj1 = (OsuDifficultyHitObject?)Previous(1);
            OsuDifficultyHitObject? prevObj2 = (OsuDifficultyHitObject?)Previous(2);

            if (Angle.IsNull() || prevObj0.IsNull() || prevObj0.Angle.IsNull())
                return 1.0;

            double angleDifference = Math.Abs(prevObj0.Angle.Value - Angle.Value);

            // Assume that very low spacing difference means that angles don't matter
            if (prevObj0.LazyJumpDistance < NORMALISED_RADIUS)
                angleDifference *= Math.Pow(prevObj0.LazyJumpDistance / NORMALISED_RADIUS, 2);
            if (LazyJumpDistance < NORMALISED_RADIUS)
                angleDifference *= Math.Pow(LazyJumpDistance / NORMALISED_RADIUS, 2);

            // Now research previous angles
            double angleDifferencePrev = 0;

            // How close the smallest angle of curr and prev is to 0
            double zeroAngleFactor = 1.0;

            // Nerf alternating angles case
            if (prevObj1.IsNotNull() && prevObj2.IsNotNull() && prevObj1.Angle.IsNotNull())
            {
                angleDifferencePrev = Math.Abs(prevObj1.Angle.Value - Angle.Value);
                zeroAngleFactor = Math.Pow(1 - Math.Min(Angle.Value, prevObj0.Angle.Value) / Math.PI, 10);
            }

            // Will be close to 1 if angleDifferencePrev is close to 0
            double rescaleFactor = Math.Pow(1 - angleDifferencePrev / Math.PI, 5);

            // 0 on different rhythm, 1 on same rhythm
            double rhythmFactor = 1 - getTimeDifference(StrainTime, prevObj0.StrainTime);

            if (prevObj1.IsNotNull())
                rhythmFactor *= 1 - getTimeDifference(prevObj0.StrainTime, prevObj1.StrainTime);
            if (prevObj1.IsNotNull() && prevObj2.IsNotNull())
                rhythmFactor *= 1 - getTimeDifference(prevObj1.StrainTime, prevObj2.StrainTime);

            // Get the base - how much alternating difference is lower than current difference
            double prevAngleAdjust = Math.Max(angleDifference - angleDifferencePrev, 0);

            // Don't apply the nerf when angleDifferencePrev is too high
            prevAngleAdjust *= rescaleFactor;

            // Don't apply the nerf if rhythm is changing
            prevAngleAdjust *= rhythmFactor;

            // Don't apply the nerf if neither of previous angles isn't close to 0
            prevAngleAdjust *= zeroAngleFactor;

            angleDifference -= prevAngleAdjust;

            // Bandaid to fix Rubik's Cube +EZ
            double wideness = 0;
            if (Angle!.Value > Math.PI * 0.5)
            {
                // Goes from 0 to 1 as angle increasing from 90 degrees to 180
                wideness = (Angle.Value / Math.PI - 0.5) * 2;

                // Transform into cubic scaling
                wideness = 1 - Math.Pow(1 - wideness, 3);
            }

            // Angle difference will be considered as 2 times lower if angle is wide
            angleDifference /= 1 + wideness;

            // Angle difference more than 15 degrees gets no penalty
            double adjustedAngleDifference = Math.Min(Math.PI / 12, angleDifference);
            return rhythmFactor * Math.Cos(Math.Min(Math.PI / 2, 6 * adjustedAngleDifference));
        }

        public double OpacityAt(double time, bool hidden)
        {
            var baseObject = BaseObject; // Optimization

            if (time > baseObject.StartTime)
            {
                // Consider a hitobject as being invisible when its start time is passed.
                // In reality the hitobject will be visible beyond its start time up until its hittable window has passed,
                // but this is an approximation and such a case is unlikely to be hit where this function is used.
                return 0.0;
            }

            double fadeInStartTime = baseObject.StartTime - baseObject.TimePreempt;
            double fadeInDuration = baseObject.TimeFadeInRaw;

            if (hidden)
            {
                // Taken from OsuModHidden.
                double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeInRaw;
                double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

                return Math.Min
                (
                    Math.Clamp((time - fadeInStartTime) / fadeInDuration, 0.0, 1.0),
                    1.0 - Math.Clamp((time - fadeOutStartTime) / fadeOutDuration, 0.0, 1.0)
                );
            }

            return Math.Clamp((time - fadeInStartTime) / fadeInDuration, 0.0, 1.0);
        }

        private void setDistances(double clockRate)
        {
            if (BaseObject is Slider currentSlider)
            {
                computeSliderCursorPosition(currentSlider);
                // Bonus for repeat sliders until a better per nested object strain system can be achieved.
                TravelDistance = currentSlider.LazyTravelDistance * (float)Math.Pow(1 + currentSlider.RepeatCount / 2.5, 1.0 / 2.5);
                TravelTime = Math.Max(currentSlider.LazyTravelTime / clockRate, min_delta_time);
            }

            // We don't need to calculate either angle or distance when one of the last->curr objects is a spinner
            if (BaseObject is Spinner || lastObject is Spinner)
                return;

            // We will scale distances by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = NORMALISED_RADIUS / (float)BaseObject.Radius;

            if (BaseObject.Radius < 30)
            {
                float smallCircleBonus = Math.Min(30 - (float)BaseObject.Radius, 5) / 50;
                scalingFactor *= 1 + smallCircleBonus;
            }

            Vector2 lastCursorPosition = getEndCursorPosition(lastObject);

            LazyJumpDistance = (BaseObject.StackedPosition * scalingFactor - lastCursorPosition * scalingFactor).Length;
            MinimumJumpTime = StrainTime;
            MinimumJumpDistance = LazyJumpDistance;

            if (lastObject is Slider lastSlider)
            {
                double lastTravelTime = Math.Max(lastSlider.LazyTravelTime / clockRate, min_delta_time);
                MinimumJumpTime = Math.Max(StrainTime - lastTravelTime, min_delta_time);

                //
                // There are two types of slider-to-object patterns to consider in order to better approximate the real movement a player will take to jump between the hitobjects.
                //
                // 1. The anti-flow pattern, where players cut the slider short in order to move to the next hitobject.
                //
                //      <======o==>  ← slider
                //             |     ← most natural jump path
                //             o     ← a follow-up hitcircle
                //
                // In this case the most natural jump path is approximated by LazyJumpDistance.
                //
                // 2. The flow pattern, where players follow through the slider to its visual extent into the next hitobject.
                //
                //      <======o==>---o
                //                  ↑
                //        most natural jump path
                //
                // In this case the most natural jump path is better approximated by a new distance called "tailJumpDistance" - the distance between the slider's tail and the next hitobject.
                //
                // Thus, the player is assumed to jump the minimum of these two distances in all cases.
                //

                float tailJumpDistance = Vector2.Subtract(lastSlider.TailCircle.StackedPosition, BaseObject.StackedPosition).Length * scalingFactor;
                MinimumJumpDistance = Math.Max(0, Math.Min(LazyJumpDistance - (maximum_slider_radius - assumed_slider_radius), tailJumpDistance - maximum_slider_radius));
            }

            if (lastLastObject != null && !(lastLastObject is Spinner))
            {
                Vector2 lastLastCursorPosition = getEndCursorPosition(lastLastObject);

                Vector2 v1 = lastLastCursorPosition - lastObject.StackedPosition;
                Vector2 v2 = BaseObject.StackedPosition - lastCursorPosition;

                float dot = Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                AngleSigned = Math.Atan2(det, dot);
                Angle = Math.Abs((double)AngleSigned);
            }
        }

        private void computeSliderCursorPosition(Slider slider)
        {
            if (slider.LazyEndPosition != null)
                return;

            // TODO: This commented version is actually correct by the new lazer implementation, but intentionally held back from
            // difficulty calculator to preserve known behaviour.
            // double trackingEndTime = Math.Max(
            //     // SliderTailCircle always occurs at the final end time of the slider, but the player only needs to hold until within a lenience before it.
            //     slider.Duration + SliderEventGenerator.TAIL_LENIENCY,
            //     // There's an edge case where one or more ticks/repeats fall within that leniency range.
            //     // In such a case, the player needs to track until the final tick or repeat.
            //     slider.NestedHitObjects.LastOrDefault(n => n is not SliderTailCircle)?.StartTime ?? double.MinValue
            // );

            double trackingEndTime = Math.Max(
                slider.StartTime + slider.Duration + SliderEventGenerator.TAIL_LENIENCY,
                slider.StartTime + slider.Duration / 2
            );

            IList<HitObject> nestedObjects = slider.NestedHitObjects;

            SliderTick? lastRealTick = null;

            foreach (var hitobject in slider.NestedHitObjects)
            {
                if (hitobject is SliderTick tick)
                    lastRealTick = tick;
            }

            if (lastRealTick?.StartTime > trackingEndTime)
            {
                trackingEndTime = lastRealTick.StartTime;

                // When the last tick falls after the tracking end time, we need to re-sort the nested objects
                // based on time. This creates a somewhat weird ordering which is counter to how a user would
                // understand the slider, but allows a zero-diff with known diffcalc output.
                //
                // To reiterate, this is definitely not correct from a difficulty calculation perspective
                // and should be revisited at a later date (likely by replacing this whole code with the commented
                // version above).
                List<HitObject> reordered = nestedObjects.ToList();

                reordered.Remove(lastRealTick);
                reordered.Add(lastRealTick);

                nestedObjects = reordered;
            }

            slider.LazyTravelTime = trackingEndTime - slider.StartTime;

            double endTimeMin = slider.LazyTravelTime / slider.SpanDuration;
            if (endTimeMin % 2 >= 1)
                endTimeMin = 1 - endTimeMin % 1;
            else
                endTimeMin %= 1;

            slider.LazyEndPosition = slider.StackedPosition + slider.Path.PositionAt(endTimeMin); // temporary lazy end position until a real result can be derived.

            Vector2 currCursorPosition = slider.StackedPosition;

            double scalingFactor = NORMALISED_RADIUS / slider.Radius; // lazySliderDistance is coded to be sensitive to scaling, this makes the maths easier with the thresholds being used.

            for (int i = 1; i < nestedObjects.Count; i++)
            {
                var currMovementObj = (OsuHitObject)nestedObjects[i];

                Vector2 currMovement = Vector2.Subtract(currMovementObj.StackedPosition, currCursorPosition);
                double currMovementLength = scalingFactor * currMovement.Length;

                // Amount of movement required so that the cursor position needs to be updated.
                double requiredMovement = assumed_slider_radius;

                if (i == nestedObjects.Count - 1)
                {
                    // The end of a slider has special aim rules due to the relaxed time constraint on position.
                    // There is both a lazy end position as well as the actual end slider position. We assume the player takes the simpler movement.
                    // For sliders that are circular, the lazy end position may actually be farther away than the sliders true end.
                    // This code is designed to prevent buffing situations where lazy end is actually a less efficient movement.
                    Vector2 lazyMovement = Vector2.Subtract((Vector2)slider.LazyEndPosition, currCursorPosition);

                    if (lazyMovement.Length < currMovement.Length)
                        currMovement = lazyMovement;

                    currMovementLength = scalingFactor * currMovement.Length;
                }
                else if (currMovementObj is SliderRepeat)
                {
                    // For a slider repeat, assume a tighter movement threshold to better assess repeat sliders.
                    requiredMovement = NORMALISED_RADIUS;
                }

                if (currMovementLength > requiredMovement)
                {
                    // this finds the positional delta from the required radius and the current position, and updates the currCursorPosition accordingly, as well as rewarding distance.
                    currCursorPosition = Vector2.Add(currCursorPosition, Vector2.Multiply(currMovement, (float)((currMovementLength - requiredMovement) / currMovementLength)));
                    currMovementLength *= (currMovementLength - requiredMovement) / currMovementLength;
                    slider.LazyTravelDistance += (float)currMovementLength;
                }

                if (i == nestedObjects.Count - 1)
                    slider.LazyEndPosition = currCursorPosition;
            }
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

        public struct ReadingObject
        {
            public OsuDifficultyHitObject HitObject;
            public double Overlapness;

            public ReadingObject(OsuDifficultyHitObject hitObject, double overlapness)
            {
                HitObject = hitObject;
                Overlapness = overlapness;
            }
        }
    }
}
