// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Objects;
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

        public const int NORMALISED_DIAMETER = NORMALISED_RADIUS * 2;

        public const int MIN_DELTA_TIME = 25;

        public const double SLIDER_RADIUS_MULTIPLIER = 2.16949152542;

        protected new OsuHitObject BaseObject => (OsuHitObject)base.BaseObject;
        protected new OsuHitObject LastObject => (OsuHitObject)base.LastObject;

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public readonly double StrainTime;

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
        /// The position of the cursor at the point of completion of this <see cref="OsuDifficultyHitObject"/> if it is a <see cref="Slider"/>
        /// and was hit with as few movements as possible.
        /// </summary>
        public Vector2? LazyEndPosition { get; private set; }

        /// <summary>
        /// The distance travelled by the cursor upon completion of this <see cref="OsuDifficultyHitObject"/> if it is a <see cref="Slider"/>
        /// and was hit with as few movements as possible.
        /// </summary>
        public double LazyTravelDistance { get; private set; }

        /// <summary>
        /// The time taken by the cursor upon completion of this <see cref="OsuDifficultyHitObject"/> if it is a <see cref="Slider"/>
        /// and was hit with as few movements as possible.
        /// </summary>
        public double LazyTravelTime { get; private set; }

        /// <summary>
        /// Angle the player has to take to hit this <see cref="OsuDifficultyHitObject"/>.
        /// Calculated as the angle between the circles (current-2, current-1, current).
        /// </summary>
        public double? Angle { get; private set; }

        /// <summary>
        /// Retrieves the full hit window for a Great <see cref="HitResult"/>.
        /// </summary>
        public double HitWindowGreat { get; private set; }

        /// <summary>
        /// Selective bonus for maps with higher circle size.
        /// </summary>
        public double SmallCircleBonus { get; private set; }

        /// <summary>
        /// Selective bonus for maps with higher circle size.
        /// </summary>
        public double SliderTailVelocityVelocity { get; private set; }

        private double? tailSliderAngle;

        private Vector2 lastCursorSliderPosition;

        private readonly OsuDifficultyHitObject? lastLastDifficultyObject;
        private readonly OsuDifficultyHitObject? lastDifficultyObject;

        public OsuDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            lastLastDifficultyObject = index > 1 ? (OsuDifficultyHitObject)objects[index - 2] : null;
            lastDifficultyObject = index > 0 ? (OsuDifficultyHitObject)objects[index - 1] : null;

            // Capped to 25ms to prevent difficulty calculation breaking from simultaneous objects.
            StrainTime = Math.Max(DeltaTime, MIN_DELTA_TIME);

            SmallCircleBonus = Math.Max(1.0, 1.0 + (30 - BaseObject.Radius) / 40);

            if (BaseObject is Slider sliderObject)
            {
                HitWindowGreat = 2 * sliderObject.HeadCircle.HitWindows.WindowFor(HitResult.Great) / clockRate;
            }
            else
            {
                HitWindowGreat = 2 * BaseObject.HitWindows.WindowFor(HitResult.Great) / clockRate;
            }

            computeSliderCursorPosition();
            setDistances(clockRate);
        }

        public double OpacityAt(double time, bool hidden)
        {
            if (time > BaseObject.StartTime)
            {
                // Consider a hitobject as being invisible when its start time is passed.
                // In reality the hitobject will be visible beyond its start time up until its hittable window has passed,
                // but this is an approximation and such a case is unlikely to be hit where this function is used.
                return 0.0;
            }

            double fadeInStartTime = BaseObject.StartTime - BaseObject.TimePreempt;
            double fadeInDuration = BaseObject.TimeFadeIn;

            if (hidden)
            {
                // Taken from OsuModHidden.
                double fadeOutStartTime = BaseObject.StartTime - BaseObject.TimePreempt + BaseObject.TimeFadeIn;
                double fadeOutDuration = BaseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

                return Math.Min
                (
                    Math.Clamp((time - fadeInStartTime) / fadeInDuration, 0.0, 1.0),
                    1.0 - Math.Clamp((time - fadeOutStartTime) / fadeOutDuration, 0.0, 1.0)
                );
            }

            return Math.Clamp((time - fadeInStartTime) / fadeInDuration, 0.0, 1.0);
        }

        /// <summary>
        /// Returns how possible is it to doubletap this object together with the next one and get perfect judgement in range from 0 to 1
        /// </summary>
        public double GetDoubletapness(OsuDifficultyHitObject? osuNextObj)
        {
            if (osuNextObj != null)
            {
                double currDeltaTime = Math.Max(1, DeltaTime);
                double nextDeltaTime = Math.Max(1, osuNextObj.DeltaTime);
                double deltaDifference = Math.Abs(nextDeltaTime - currDeltaTime);
                double speedRatio = currDeltaTime / Math.Max(currDeltaTime, deltaDifference);
                double windowRatio = Math.Pow(Math.Min(1, currDeltaTime / HitWindowGreat), 2);
                return 1.0 - Math.Pow(speedRatio, 1 - windowRatio);
            }

            return 0;
        }

        private void setDistances(double clockRate)
        {
            if (BaseObject is Slider)
            {
                // Bonus for repeat sliders until a better per nested object strain system can be achieved.
                TravelDistance = LazyTravelDistance;
                TravelTime = Math.Max(LazyTravelTime / clockRate, MIN_DELTA_TIME);
            }

            // We don't need to calculate either angle or distance when one of the last->curr objects is a spinner
            if (BaseObject is Spinner || LastObject is Spinner)
                return;

            // We will scale distances by this factor, so we can assume a uniform CircleSize among beatmaps.
            double scalingFactor = NORMALISED_RADIUS / BaseObject.Radius;

            Vector2 lastCursorPosition = lastDifficultyObject != null ? getEndCursorPosition(lastDifficultyObject) : LastObject.StackedPosition;

            LazyJumpDistance = Vector2.Subtract(BaseObject.StackedPosition, lastCursorPosition).Length * scalingFactor;
            MinimumJumpTime = StrainTime;
            MinimumJumpDistance = LazyJumpDistance;

            Vector2 lastObjStackedPosition = LastObject is Slider lastslider ? lastslider.TailCircle.StackedPosition : LastObject.StackedPosition;

            if (lastLastDifficultyObject != null && lastLastDifficultyObject.BaseObject is not Spinner)
            {
                bool sliderValidation = lastLastDifficultyObject.BaseObject is Slider &&
                                        TravelDistance > 0;
                Vector2 lastLastCursorPosition = sliderValidation ? lastCursorSliderPosition : getEndCursorPosition(lastLastDifficultyObject);

                Angle = Math.Abs(calculateAngle(lastLastCursorPosition,
                                                lastObjStackedPosition,
                                                BaseObject.StackedPosition));
            }

            if (LastObject is Slider lastSlider && lastDifficultyObject != null)
            {
                double lastTravelTime = Math.Max(lastDifficultyObject.LazyTravelTime / clockRate, MIN_DELTA_TIME);
                MinimumJumpTime = Math.Max(StrainTime - lastTravelTime, MIN_DELTA_TIME);

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

                const double slider_radius = NORMALISED_RADIUS * SLIDER_RADIUS_MULTIPLIER;
                const double jump_slider_radius = NORMALISED_RADIUS * 1.62711864447; // magic number to match the previous calculation of jump distance.

                double tailJumpDistance = Vector2.Subtract(lastSlider.TailCircle.StackedPosition, BaseObject.StackedPosition).Length * scalingFactor;
                MinimumJumpDistance = Math.Max(0, Math.Min(LazyJumpDistance - (slider_radius - jump_slider_radius), tailJumpDistance - slider_radius));

                double sliderAngle = tailSliderAngle != null ? tailSliderAngle.Value : 0;
                double sliderAngleWideness = DifficultyCalculationUtils.Smoothstep(sliderAngle, double.DegreesToRadians(140), double.DegreesToRadians(160));

                double lastAngle = lastDifficultyObject.Angle != null ? lastDifficultyObject.Angle.Value : 180;
                double lastAngleWideness = DifficultyCalculationUtils.Smoothstep(lastAngle, double.DegreesToRadians(50), double.DegreesToRadians(20));

                double cheeseNerf = NORMALISED_RADIUS * (sliderAngleWideness + lastAngleWideness);

                TravelDistance = Math.Max(TravelDistance - cheeseNerf, 0);
            }
        }

        private void computeSliderCursorPosition()
        {
            if (BaseObject is not Slider slider)
                return;

            if (LazyEndPosition != null)
                return;

            IList<OsuHitObject> nestedObjects = slider.NestedHitObjects.Cast<OsuHitObject>().ToList();

            double endLeniency = SliderEventGenerator.TAIL_LENIENCY;

            if (nestedObjects.LastOrDefault(n => n is not SliderTailCircle) is OsuHitObject lastNestedObjects)
                endLeniency = Math.Max(-(nestedObjects.Last().StartTime - lastNestedObjects.StartTime), endLeniency);

            double trackingEndTime = slider.StartTime + slider.Duration + endLeniency;

            double currentSliderRadius = BaseObject.Radius * SLIDER_RADIUS_MULTIPLIER;

            LazyTravelTime = trackingEndTime - slider.StartTime;

            LazyEndPosition = slider.StackedPositionAt(trackingEndTime / slider.SpanDuration);

            Vector2 currCursorPosition = slider.StackedPosition;

            double lastStartTime = slider.HeadCircle.StartTime;
            Vector2 lastCursorPosition = slider.HeadCircle.StackedPosition;

            double scalingFactor = NORMALISED_RADIUS / currentSliderRadius;

            double totalSliderValue = 0;

            for (int i = 1; i < nestedObjects.Count; i++)
            {
                OsuHitObject currMovementObj = nestedObjects[i];

                double currMovementLength = 0;

                // Amount of movement required so that the cursor position needs to be updated.
                double requiredMovement = NORMALISED_RADIUS;

                bool lastIsRepeat = nestedObjects[i - 1] is SliderRepeat;
                bool lastIsHead = nestedObjects[i - 1] is SliderHeadCircle;

                // For SliderRepeat objects, we need to adjust the required movement.
                if (lastIsRepeat)
                    requiredMovement *= 1.75;

                double currentStrainValue = currMovementObj.StartTime - lastStartTime;

                double middleStrainValue = (currMovementObj.StartTime + lastStartTime) / 2 - lastStartTime;

                double middleToCurrStrainValue = currentStrainValue - middleStrainValue;

                Vector2 currMovement = Vector2.Subtract(slider.StackedPositionAt(currentStrainValue / slider.SpanDuration), currCursorPosition);

                Vector2 additionalSliderPath = slider.StackedPositionAt(middleStrainValue / slider.SpanDuration);

                double lastToMiddleMovementLength = Vector2.Subtract(lastCursorPosition, additionalSliderPath).Length * scalingFactor;
                double middleToCurrMovementLength = Vector2.Subtract(additionalSliderPath, slider.StackedPositionAt(currentStrainValue / slider.SpanDuration)).Length * scalingFactor;

                currMovementLength = lastToMiddleMovementLength + middleToCurrMovementLength;

                double velocityChangeBonus = 1 + calcVelocityChange(lastToMiddleMovementLength / middleStrainValue, middleToCurrMovementLength / middleToCurrStrainValue);

                double angle = Math.Abs(calculateAngle(lastCursorPosition, additionalSliderPath, currCursorPosition));

                double angleAcutenessBonus = DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(140), double.DegreesToRadians(70));
                double angleWidenessBonus = DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(70), double.DegreesToRadians(140));

                angleAcutenessBonus -= DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(70), double.DegreesToRadians(0));
                angleWidenessBonus -= DifficultyCalculationUtils.Smoothstep(angle, double.DegreesToRadians(140), double.DegreesToRadians(180));

                double angleBonus = Math.Max(angleAcutenessBonus + angleWidenessBonus, 0);

                if (lastIsRepeat || currMovementObj is SliderRepeat)
                    velocityChangeBonus *= Math.Pow(0.73 + angleBonus, 2);
                else if (lastIsHead && currMovementObj is SliderTailCircle)
                    velocityChangeBonus *= Math.Pow(0.86 + angleBonus, 2);
                else
                    velocityChangeBonus *= Math.Pow(1 + angleBonus, 2);

                if (currMovementObj is SliderTailCircle)
                {
                    // this finds the positional delta from the required radius and the current position, and updates the currCursorPosition accordingly, as well as rewarding distance.
                    currCursorPosition = slider.StackedPositionAt(currentStrainValue);
                    currMovementLength = Math.Max(currMovementLength - requiredMovement, 0) * velocityChangeBonus;
                    totalSliderValue += currMovementLength;

                    SliderTailVelocityVelocity = middleToCurrMovementLength / middleToCurrStrainValue;
                    LazyEndPosition = currCursorPosition;

                    tailSliderAngle = angle;
                    lastCursorSliderPosition = additionalSliderPath;
                }

                LazyTravelDistance += totalSliderValue;

                lastStartTime = currMovementObj.StartTime;
            }
        }

        private Vector2 getEndCursorPosition(OsuDifficultyHitObject difficultyHitObject)
        {
            return difficultyHitObject.LazyEndPosition ?? difficultyHitObject.BaseObject.StackedPosition;
        }

        private double calculateAngle(Vector2 lastLastPosition, Vector2 lastPosition, Vector2 currPosition)
        {
            Vector2 v1 = lastLastPosition - lastPosition;
            Vector2 v2 = currPosition - lastPosition;

            float dot = Vector2.Dot(v1, v2);
            float det = v1.X * v2.Y - v1.Y * v2.X;

            return Math.Atan2(det, dot);
        }

        private double calcVelocityChange(double prevVelocity, double currVelocity)
        {
            if (prevVelocity == 0 || currVelocity == 0)
                return 0;
            return DifficultyCalculationUtils.Smoothstep(Math.Abs(prevVelocity - currVelocity), 0, prevVelocity + currVelocity);
        }
    }
}
