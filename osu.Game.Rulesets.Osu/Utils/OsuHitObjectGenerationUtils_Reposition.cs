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

namespace osu.Game.Rulesets.Osu.Utils
{
    public static partial class OsuHitObjectGenerationUtils
    {
        /// <summary>
        /// Number of previous hitobjects to be shifted together when an object is being moved.
        /// </summary>
        private const int preceding_hitobjects_to_shift = 10;

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
                float absoluteAngle = MathF.Atan2(relativePosition.Y, relativePosition.X);
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
            WorkingObject? previous = null;

            for (int i = 0; i < workingObjects.Count; i++)
            {
                var current = workingObjects[i];
                var hitObject = current.HitObject;

                if (hitObject is Spinner)
                {
                    previous = current;
                    continue;
                }

                computeModifiedPosition(current, previous, i > 1 ? workingObjects[i - 2] : null);

                // Move hit objects back into the playfield if they are outside of it
                Vector2 shift = Vector2.Zero;

                switch (hitObject)
                {
                    case HitCircle:
                        shift = clampHitCircleToPlayfield(current);
                        break;

                    case Slider:
                        shift = clampSliderToPlayfield(current);
                        break;
                }

                if (shift != Vector2.Zero)
                {
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

                previous = current;
            }

            return workingObjects.Select(p => p.HitObject).ToList();
        }

        /// <summary>
        /// Compute the modified position of a hit object while attempting to keep it inside the playfield.
        /// </summary>
        /// <param name="current">The <see cref="WorkingObject"/> representing the hit object to have the modified position computed for.</param>
        /// <param name="previous">The <see cref="WorkingObject"/> representing the hit object immediately preceding the current one.</param>
        /// <param name="beforePrevious">The <see cref="WorkingObject"/> representing the hit object immediately preceding the <paramref name="previous"/> one.</param>
        private static void computeModifiedPosition(WorkingObject current, WorkingObject? previous, WorkingObject? beforePrevious)
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
                    Vector2 earliestPosition = beforePrevious?.HitObject.EndPosition ?? playfield_centre;
                    Vector2 relativePosition = previous.HitObject.Position - earliestPosition;
                    previousAbsoluteAngle = MathF.Atan2(relativePosition.Y, relativePosition.X);
                }
            }

            float absoluteAngle = previousAbsoluteAngle + current.PositionInfo.RelativeAngle;

            var posRelativeToPrev = new Vector2(
                current.PositionInfo.DistanceFromPrevious * MathF.Cos(absoluteAngle),
                current.PositionInfo.DistanceFromPrevious * MathF.Sin(absoluteAngle)
            );

            Vector2 lastEndPosition = previous?.EndPositionModified ?? playfield_centre;

            posRelativeToPrev = RotateAwayFromEdge(lastEndPosition, posRelativeToPrev);

            current.PositionModified = lastEndPosition + posRelativeToPrev;

            if (!(current.HitObject is Slider slider))
                return;

            absoluteAngle = MathF.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);

            Vector2 centreOfMassOriginal = calculateCentreOfMass(slider);
            Vector2 centreOfMassModified = rotateVector(centreOfMassOriginal, current.PositionInfo.Rotation + absoluteAngle - getSliderRotation(slider));
            centreOfMassModified = RotateAwayFromEdge(current.PositionModified, centreOfMassModified);

            float relativeRotation = MathF.Atan2(centreOfMassModified.Y, centreOfMassModified.X) - MathF.Atan2(centreOfMassOriginal.Y, centreOfMassOriginal.X);
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
            workingObject.EndPositionModified = workingObject.PositionModified = clampToPlayfieldWithPadding(
                workingObject.PositionModified,
                (float)workingObject.HitObject.Radius
            );

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
            var possibleMovementBounds = CalculatePossibleMovementBounds(slider);

            // The slider rotation applied in computeModifiedPosition might make it impossible to fit the slider into the playfield
            // For example, a long horizontal slider will be off-screen when rotated by 90 degrees
            // In this case, limit the rotation to either 0 or 180 degrees
            if (possibleMovementBounds.Width < 0 || possibleMovementBounds.Height < 0)
            {
                float currentRotation = getSliderRotation(slider);
                float diff1 = getAngleDifference(workingObject.RotationOriginal, currentRotation);
                float diff2 = getAngleDifference(workingObject.RotationOriginal + MathF.PI, currentRotation);

                if (diff1 < diff2)
                {
                    RotateSlider(slider, workingObject.RotationOriginal - getSliderRotation(slider));
                }
                else
                {
                    RotateSlider(slider, workingObject.RotationOriginal + MathF.PI - getSliderRotation(slider));
                }

                possibleMovementBounds = CalculatePossibleMovementBounds(slider);
            }

            var previousPosition = workingObject.PositionModified;

            // Clamp slider position to the placement area
            // If the slider is larger than the playfield, at least make sure that the head circle is inside the playfield
            float newX = possibleMovementBounds.Width < 0
                ? Math.Clamp(possibleMovementBounds.Left, 0, OsuPlayfield.BASE_SIZE.X)
                : Math.Clamp(previousPosition.X, possibleMovementBounds.Left, possibleMovementBounds.Right);

            float newY = possibleMovementBounds.Height < 0
                ? Math.Clamp(possibleMovementBounds.Top, 0, OsuPlayfield.BASE_SIZE.Y)
                : Math.Clamp(previousPosition.Y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);

            slider.Position = workingObject.PositionModified = new Vector2(newX, newY);
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

                hitObject.Position = clampToPlayfieldWithPadding(position, (float)hitObject.Radius);
            }
        }

        /// <summary>
        /// Calculates a <see cref="RectangleF"/> which contains all of the possible movements of the slider (in relative X/Y coordinates)
        /// such that the entire slider is inside the playfield.
        /// </summary>
        /// <param name="slider">The <see cref="Slider"/> for which to calculate a movement bounding box.</param>
        /// <returns>A <see cref="RectangleF"/> which contains all of the possible movements of the slider such that the entire slider is inside the playfield.</returns>
        /// <remarks>
        /// If the slider is larger than the playfield, the returned <see cref="RectangleF"/> may have negative width/height.
        /// </remarks>
        public static RectangleF CalculatePossibleMovementBounds(Slider slider)
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

            // Take the circle radius into account.
            float radius = (float)slider.Radius;

            minX -= radius;
            minY -= radius;

            maxX += radius;
            maxY += radius;

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
        /// Clamp a position to playfield, keeping a specified distance from the edges.
        /// </summary>
        /// <param name="position">The position to be clamped.</param>
        /// <param name="padding">The minimum distance allowed from playfield edges.</param>
        /// <returns>The clamped position.</returns>
        private static Vector2 clampToPlayfieldWithPadding(Vector2 position, float padding)
        {
            return new Vector2(
                Math.Clamp(position.X, padding, OsuPlayfield.BASE_SIZE.X - padding),
                Math.Clamp(position.Y, padding, OsuPlayfield.BASE_SIZE.Y - padding)
            );
        }

        /// <summary>
        /// Estimate the centre of mass of a slider relative to its start position.
        /// </summary>
        /// <param name="slider">The slider to process.</param>
        /// <returns>The centre of mass of the slider.</returns>
        private static Vector2 calculateCentreOfMass(Slider slider)
        {
            const double sample_step = 50;

            // just sample the start and end positions if the slider is too short
            if (slider.Distance <= sample_step)
            {
                return Vector2.Divide(slider.Path.PositionAt(1), 2);
            }

            int count = 0;
            Vector2 sum = Vector2.Zero;
            double pathDistance = slider.Distance;

            for (double i = 0; i < pathDistance; i += sample_step)
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
            return MathF.Atan2(endPositionVector.Y, endPositionVector.X);
        }

        /// <summary>
        /// Get the absolute difference between 2 angles measured in Radians.
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>The absolute difference with interval <c>[0, MathF.PI)</c></returns>
        private static float getAngleDifference(float angle1, float angle2)
        {
            float diff = MathF.Abs(angle1 - angle2) % (MathF.PI * 2);
            return MathF.Min(diff, MathF.PI * 2 - diff);
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
            public float RotationOriginal { get; }
            public Vector2 PositionModified { get; set; }
            public Vector2 EndPositionModified { get; set; }

            public ObjectPositionInfo PositionInfo { get; }
            public OsuHitObject HitObject => PositionInfo.HitObject;

            public WorkingObject(ObjectPositionInfo positionInfo)
            {
                PositionInfo = positionInfo;
                RotationOriginal = HitObject is Slider slider ? getSliderRotation(slider) : 0;
                PositionModified = HitObject.Position;
                EndPositionModified = HitObject.EndPosition;
            }
        }
    }
}
