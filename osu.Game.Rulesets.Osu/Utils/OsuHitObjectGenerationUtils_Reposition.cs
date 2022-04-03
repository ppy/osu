// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

#nullable enable

namespace osu.Game.Rulesets.Osu.Utils
{
    public static partial class OsuHitObjectGenerationUtils
    {
        /// <summary>
        /// Number of previous hitobjects to be shifted together when an object is being moved.
        /// </summary>
        private const int preceding_hitobjects_to_shift = 10;

        /// <summary>
        /// How far an object has to be out of bounds before it gets rotated towards playfield center.
        /// </summary>
        private const double out_of_bounds_tolerance = 10;

        private static readonly Vector2 playfield_centre = OsuPlayfield.BASE_SIZE / 2;

        /// <summary>
        /// Generate a list of <see cref="ObjectPositionInfo"/>s containing information for how the given list of
        /// <see cref="OsuHitObject"/>s are positioned.
        /// </summary>
        /// <param name="hitObjects">A list of <see cref="OsuHitObject"/>s to process.</param>
        /// <returns>A list of <see cref="ObjectPositionInfo"/>s describing how each hit object is positioned relative to the previous one.</returns>
        public static List<ObjectPositionInfo> GeneratePositionInfos(IEnumerable<OsuHitObject> hitObjects)
        {
            var positionInfos = new List<ObjectPositionInfo>();
            Vector2 previousPosition = playfield_centre;
            float previousAngle = 0;

            foreach (OsuHitObject hitObject in hitObjects)
            {
                Vector2 relativePosition = hitObject.Position - previousPosition;
                float absoluteAngle = (float)Math.Atan2(relativePosition.Y, relativePosition.X);
                float relativeAngle = absoluteAngle - previousAngle;

                ObjectPositionInfo positionInfo;
                positionInfos.Add(positionInfo = new ObjectPositionInfo(hitObject)
                {
                    RelativeAngle = relativeAngle,
                    DistanceFromPrevious = relativePosition.Length
                });

                if (hitObject is Slider slider)
                {
                    float absoluteRotation = getSliderRotation(slider);
                    positionInfo.Rotation = absoluteRotation - absoluteAngle;
                    absoluteAngle = absoluteRotation;
                }

                previousPosition = hitObject.EndPosition;
                previousAngle = absoluteAngle;
            }

            return positionInfos;
        }

        /// <summary>
        /// Reposition the hit objects according to the information in <paramref name="objectPositionInfos"/>.
        /// </summary>
        /// <param name="objectPositionInfos">Position information for each hit object.</param>
        /// <returns>The repositioned hit objects.</returns>
        public static List<OsuHitObject> RepositionHitObjects(IEnumerable<ObjectPositionInfo> objectPositionInfos)
        {
            List<WorkingObject> workingObjects = objectPositionInfos.Select(o => new WorkingObject(o)).ToList();
            bool rotateAwayFromEdge = false;
            int furthestIndex = 0;
            int furthestIndexWithRotation = 0;

            for (int i = 0; i < workingObjects.Count; i++)
            {
                var current = workingObjects[i];
                WorkingObject? previous = i > 0 ? workingObjects[i - 1] : null;
                var hitObject = current.HitObject;

                if (rotateAwayFromEdge)
                    furthestIndexWithRotation = Math.Max(i, furthestIndexWithRotation);

                if (current.PositionInfo.StayInPlace)
                {
                    continue;
                }

                computeModifiedPosition(current, previous, i > 1 ? workingObjects[i - 2] : null, rotateAwayFromEdge);

                // Move hit objects back into the playfield if they are outside of it
                Vector2 shift = Vector2.Zero;

                switch (hitObject)
                {
                    case HitCircle _:
                        shift = clampHitCircleToPlayfield(current);
                        break;

                    case Slider _:
                        shift = clampSliderToPlayfield(current);
                        break;
                }

                if (shift.LengthSquared >= 1)
                {
                    if (!rotateAwayFromEdge && shift.LengthSquared > out_of_bounds_tolerance * out_of_bounds_tolerance)
                    {
                        furthestIndex = i;
                        rotateAwayFromEdge = true;
                        i = Math.Max(furthestIndexWithRotation, i - preceding_hitobjects_to_shift);
                        continue;
                    }

                    var toBeShifted = new List<OsuHitObject>();

                    for (int j = i - 1; j >= i - preceding_hitobjects_to_shift && j >= 0; j--)
                    {
                        // only shift hit circles
                        if (!(workingObjects[j].HitObject is HitCircle)) break;

                        toBeShifted.Add(workingObjects[j].HitObject);
                    }

                    if (toBeShifted.Count > 0)
                        applyDecreasingShift(toBeShifted, shift);
                }
                else if (i > furthestIndex)
                {
                    furthestIndex = i;
                    rotateAwayFromEdge = false;
                }
            }

            return workingObjects.Select(p => p.HitObject).ToList();
        }

        /// <summary>
        /// Compute the modified position of a hit object while attempting to keep it inside the playfield.
        /// </summary>
        /// <param name="current">The <see cref="WorkingObject"/> representing the hit object to have the modified position computed for.</param>
        /// <param name="previous">The <see cref="WorkingObject"/> representing the hit object immediately preceding the current one.</param>
        /// <param name="beforePrevious">The <see cref="WorkingObject"/> representing the hit object immediately preceding the <paramref name="previous"/> one.</param>
        /// <param name="rotateAwayFromEdge">Whether to preemptively rotate this object away from playfield edges.</param>
        private static void computeModifiedPosition(WorkingObject current, WorkingObject? previous, WorkingObject? beforePrevious, bool rotateAwayFromEdge)
        {
            float previousAbsoluteAngle = 0f;

            if (previous != null)
            {
                if (previous.HitObject is Slider s)
                {
                    previousAbsoluteAngle = getSliderRotation(s);
                }
                else
                {
                    Vector2 earliestPosition;
                    if (previous.PositionInfo.StayInPlace)
                        // beforePrevious should not affect this object if the previous object stays in place
                        earliestPosition = beforePrevious?.EndPositionOriginal ?? playfield_centre;
                    else
                        earliestPosition = beforePrevious?.HitObject.EndPosition ?? playfield_centre;
                    Vector2 relativePosition = previous.HitObject.Position - earliestPosition;
                    previousAbsoluteAngle = (float)Math.Atan2(relativePosition.Y, relativePosition.X);
                }
            }

            float absoluteAngle = previousAbsoluteAngle + current.PositionInfo.RelativeAngle;

            var posRelativeToPrev = new Vector2(
                current.PositionInfo.DistanceFromPrevious * (float)Math.Cos(absoluteAngle),
                current.PositionInfo.DistanceFromPrevious * (float)Math.Sin(absoluteAngle)
            );

            Vector2 lastEndPosition = previous?.EndPositionModified ?? playfield_centre;

            if (rotateAwayFromEdge)
                posRelativeToPrev = RotateAwayFromEdge(lastEndPosition, posRelativeToPrev);

            current.PositionModified = lastEndPosition + posRelativeToPrev;

            if (!(current.HitObject is Slider slider))
                return;

            absoluteAngle = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);

            Vector2 centreOfMassOriginal = calculateCentreOfMass(slider);
            Vector2 centreOfMassModified = rotateVector(centreOfMassOriginal, current.PositionInfo.Rotation + absoluteAngle - getSliderRotation(slider));
            if (rotateAwayFromEdge)
                centreOfMassModified = RotateAwayFromEdge(current.PositionModified, centreOfMassModified);

            float relativeRotation = (float)Math.Atan2(centreOfMassModified.Y, centreOfMassModified.X) - (float)Math.Atan2(centreOfMassOriginal.Y, centreOfMassOriginal.X);
            if (!Precision.AlmostEquals(relativeRotation, 0))
                RotateSlider(slider, relativeRotation);
        }

        /// <summary>
        /// Move the modified position of a <see cref="HitCircle"/> so that it fits inside the playfield.
        /// </summary>
        /// <returns>The deviation from the original modified position in order to fit within the playfield.</returns>
        private static Vector2 clampHitCircleToPlayfield(WorkingObject workingObject)
        {
            var previousPosition = workingObject.PositionModified;
            workingObject.EndPositionModified = workingObject.PositionModified = clampToPlayfield(workingObject.PositionModified);

            workingObject.HitObject.Position = workingObject.PositionModified;

            return workingObject.PositionModified - previousPosition;
        }

        /// <summary>
        /// Moves the <see cref="Slider"/> and all necessary nested <see cref="OsuHitObject"/>s into the <see cref="OsuPlayfield"/> if they aren't already.
        /// </summary>
        /// <returns>The deviation from the original modified position in order to fit within the playfield.</returns>
        private static Vector2 clampSliderToPlayfield(WorkingObject workingObject)
        {
            var slider = (Slider)workingObject.HitObject;
            var possibleMovementBounds = calculatePossibleMovementBounds(slider);

            var previousPosition = workingObject.PositionModified;

            // Clamp slider position to the placement area
            // If the slider is larger than the playfield, at least make sure that the head circle is inside the playfield
            float newX = possibleMovementBounds.Width < 0
                ? Math.Clamp(possibleMovementBounds.Left, 0, OsuPlayfield.BASE_SIZE.X)
                : Math.Clamp(previousPosition.X, possibleMovementBounds.Left, possibleMovementBounds.Right);

            float newY = possibleMovementBounds.Height < 0
                ? Math.Clamp(possibleMovementBounds.Top, 0, OsuPlayfield.BASE_SIZE.Y)
                : Math.Clamp(previousPosition.Y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);

            workingObject.PositionModified = new Vector2(newX, newY);

            shiftNestedObjects(slider, workingObject.PositionModified - slider.Position);

            slider.Position = workingObject.PositionModified;
            workingObject.EndPositionModified = slider.EndPosition;

            return workingObject.PositionModified - previousPosition;
        }

        /// <summary>
        /// Decreasingly shift a list of <see cref="OsuHitObject"/>s by a specified amount.
        /// The first item in the list is shifted by the largest amount, while the last item is shifted by the smallest amount.
        /// </summary>
        /// <param name="hitObjects">The list of hit objects to be shifted.</param>
        /// <param name="shift">The amount to be shifted.</param>
        private static void applyDecreasingShift(IList<OsuHitObject> hitObjects, Vector2 shift)
        {
            for (int i = 0; i < hitObjects.Count; i++)
            {
                var hitObject = hitObjects[i];
                // The first object is shifted by a vector slightly smaller than shift
                // The last object is shifted by a vector slightly larger than zero
                Vector2 position = hitObject.Position + shift * ((hitObjects.Count - i) / (float)(hitObjects.Count + 1));

                hitObject.Position = clampToPlayfield(position);
            }
        }

        /// <summary>
        /// Calculates a <see cref="RectangleF"/> which contains all of the possible movements of the slider (in relative X/Y coordinates)
        /// such that the entire slider is inside the playfield.
        /// </summary>
        /// <remarks>
        /// If the slider is larger than the playfield, the returned <see cref="RectangleF"/> may have negative width/height.
        /// </remarks>
        private static RectangleF calculatePossibleMovementBounds(Slider slider)
        {
            var pathPositions = new List<Vector2>();
            slider.Path.GetPathToProgress(pathPositions, 0, 1);

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;

            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            // Compute the bounding box of the slider.
            foreach (var pos in pathPositions)
            {
                minX = MathF.Min(minX, pos.X);
                maxX = MathF.Max(maxX, pos.X);

                minY = MathF.Min(minY, pos.Y);
                maxY = MathF.Max(maxY, pos.Y);
            }

            // Given the bounding box of the slider (via min/max X/Y),
            // the amount that the slider can move to the left is minX (with the sign flipped, since positive X is to the right),
            // and the amount that it can move to the right is WIDTH - maxX.
            // Same calculation applies for the Y axis.
            float left = -minX;
            float right = OsuPlayfield.BASE_SIZE.X - maxX;
            float top = -minY;
            float bottom = OsuPlayfield.BASE_SIZE.Y - maxY;

            return new RectangleF(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Shifts all nested <see cref="SliderTick"/>s and <see cref="SliderRepeat"/>s by the specified shift.
        /// </summary>
        /// <param name="slider"><see cref="Slider"/> whose nested <see cref="SliderTick"/>s and <see cref="SliderRepeat"/>s should be shifted</param>
        /// <param name="shift">The <see cref="Vector2"/> the <see cref="Slider"/>'s nested <see cref="SliderTick"/>s and <see cref="SliderRepeat"/>s should be shifted by</param>
        private static void shiftNestedObjects(Slider slider, Vector2 shift)
        {
            foreach (var hitObject in slider.NestedHitObjects.Where(o => o is SliderTick || o is SliderRepeat))
            {
                if (!(hitObject is OsuHitObject osuHitObject))
                    continue;

                osuHitObject.Position += shift;
            }
        }

        /// <summary>
        /// Clamp a position to playfield.
        /// </summary>
        /// <param name="position">The position to be clamped.</param>
        /// <returns>The clamped position.</returns>
        private static Vector2 clampToPlayfield(Vector2 position)
        {
            return new Vector2(
                Math.Clamp(position.X, 0, OsuPlayfield.BASE_SIZE.X),
                Math.Clamp(position.Y, 0, OsuPlayfield.BASE_SIZE.Y)
            );
        }

        /// <summary>
        /// Estimate the centre of mass of a slider relative to its start position.
        /// </summary>
        /// <param name="slider">The slider to process.</param>
        /// <returns>The centre of mass of the slider.</returns>
        private static Vector2 calculateCentreOfMass(Slider slider)
        {
            if (slider.Distance < 1) return Vector2.Zero;

            int count = 0;
            Vector2 sum = Vector2.Zero;
            double pathDistance = slider.Distance;

            for (double i = 0; i < pathDistance; i++)
            {
                sum += slider.Path.PositionAt(i / pathDistance);
                count++;
            }

            return sum / count;
        }

        /// <summary>
        /// Get the absolute rotation of a slider, defined as the angle from its start position to the end of its path.
        /// </summary>
        /// <param name="slider">The slider to process.</param>
        /// <returns>The angle in radians.</returns>
        private static float getSliderRotation(Slider slider)
        {
            var endPositionVector = slider.Path.PositionAt(1);
            return (float)Math.Atan2(endPositionVector.Y, endPositionVector.X);
        }

        public class ObjectPositionInfo
        {
            /// <summary>
            /// The jump angle from the previous hit object to this one, relative to the previous hit object's jump angle.
            /// </summary>
            /// <remarks>
            /// <see cref="RelativeAngle"/> of the first hit object in a beatmap represents the absolute angle from playfield center to the object.
            /// </remarks>
            /// <example>
            /// If <see cref="RelativeAngle"/> is 0, the player's cursor doesn't need to change its direction of movement when passing
            /// the previous object to reach this one.
            /// </example>
            public float RelativeAngle { get; set; }

            /// <summary>
            /// The jump distance from the previous hit object to this one.
            /// </summary>
            /// <remarks>
            /// <see cref="DistanceFromPrevious"/> of the first hit object in a beatmap is relative to the playfield center.
            /// </remarks>
            public float DistanceFromPrevious { get; set; }

            /// <summary>
            /// The rotation of the hit object, relative to its jump angle.
            /// For sliders, this is defined as the angle from the slider's start position to the end of its path, relative to its jump angle.
            /// For hit circles and spinners, this property is ignored.
            /// </summary>
            public float Rotation { get; set; }

            private bool stayInPlace;

            /// <summary>
            /// Forces this object to never be moved by the generation algorithm.
            /// This is always true for spinners.
            /// </summary>
            public bool StayInPlace
            {
                get => stayInPlace || HitObject is Spinner;
                set => stayInPlace = value;
            }

            /// <summary>
            /// The hit object associated with this <see cref="ObjectPositionInfo"/>.
            /// </summary>
            public OsuHitObject HitObject { get; }

            public ObjectPositionInfo(OsuHitObject hitObject)
            {
                HitObject = hitObject;
            }
        }

        private class WorkingObject
        {
            public Vector2 PositionOriginal { get; }
            public Vector2 EndPositionOriginal { get; }
            public Vector2 PositionModified { get; set; }
            public Vector2 EndPositionModified { get; set; }

            public ObjectPositionInfo PositionInfo { get; }
            public OsuHitObject HitObject => PositionInfo.HitObject;

            public WorkingObject(ObjectPositionInfo positionInfo)
            {
                PositionInfo = positionInfo;
                PositionModified = PositionOriginal = HitObject.Position;
                EndPositionModified = EndPositionOriginal = HitObject.EndPosition;
            }
        }
    }
}
