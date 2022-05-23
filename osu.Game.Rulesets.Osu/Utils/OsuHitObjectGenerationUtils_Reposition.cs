// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Primitives;
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

                positionInfos.Add(new ObjectPositionInfo(hitObject)
                {
                    RelativeAngle = relativeAngle,
                    DistanceFromPrevious = relativePosition.Length
                });

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
                    previous = null;
                    continue;
                }

                computeModifiedPosition(current, previous, i > 1 ? workingObjects[i - 2] : null);

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
                Vector2 earliestPosition = beforePrevious?.HitObject.EndPosition ?? playfield_centre;
                Vector2 relativePosition = previous.HitObject.Position - earliestPosition;
                previousAbsoluteAngle = (float)Math.Atan2(relativePosition.Y, relativePosition.X);
            }

            float absoluteAngle = previousAbsoluteAngle + current.PositionInfo.RelativeAngle;

            var posRelativeToPrev = new Vector2(
                current.PositionInfo.DistanceFromPrevious * (float)Math.Cos(absoluteAngle),
                current.PositionInfo.DistanceFromPrevious * (float)Math.Sin(absoluteAngle)
            );

            Vector2 lastEndPosition = previous?.EndPositionModified ?? playfield_centre;

            posRelativeToPrev = RotateAwayFromEdge(lastEndPosition, posRelativeToPrev);

            current.PositionModified = lastEndPosition + posRelativeToPrev;
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
            var possibleMovementBounds = calculatePossibleMovementBounds(slider);

            var previousPosition = workingObject.PositionModified;

            // Clamp slider position to the placement area
            // If the slider is larger than the playfield, force it to stay at the original position
            float newX = possibleMovementBounds.Width < 0
                ? workingObject.PositionOriginal.X
                : Math.Clamp(previousPosition.X, possibleMovementBounds.Left, possibleMovementBounds.Right);

            float newY = possibleMovementBounds.Height < 0
                ? workingObject.PositionOriginal.Y
                : Math.Clamp(previousPosition.Y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);

            slider.Position = workingObject.PositionModified = new Vector2(newX, newY);
            workingObject.EndPositionModified = slider.EndPosition;

            shiftNestedObjects(slider, workingObject.PositionModified - workingObject.PositionOriginal);

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
            public Vector2 PositionModified { get; set; }
            public Vector2 EndPositionModified { get; set; }

            public ObjectPositionInfo PositionInfo { get; }
            public OsuHitObject HitObject => PositionInfo.HitObject;

            public WorkingObject(ObjectPositionInfo positionInfo)
            {
                PositionInfo = positionInfo;
                PositionModified = PositionOriginal = HitObject.Position;
                EndPositionModified = HitObject.EndPosition;
            }
        }
    }
}
