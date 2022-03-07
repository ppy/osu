// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Places hit objects according to information in <see cref="HitObjectPositions"/> while keeping objects inside the playfield.
    /// </summary>
    public class OsuHitObjectPositionModifier
    {
        /// <summary>
        /// Number of previous hitobjects to be shifted together when an object is being moved.
        /// </summary>
        private const int preceding_hitobjects_to_shift = 10;

        private static readonly Vector2 playfield_centre = OsuPlayfield.BASE_SIZE / 2;

        private readonly List<OsuHitObject> hitObjects;

        private readonly List<HitObjectPositionInfo> hitObjectPositions = new List<HitObjectPositionInfo>();

        /// <summary>
        /// Contains information specifying how each hit object should be placed.
        /// <para>The default values correspond to how objects are originally placed in the beatmap.</para>
        /// </summary>
        public IReadOnlyList<IHitObjectPositionInfo> HitObjectPositions => hitObjectPositions;

        public OsuHitObjectPositionModifier(List<OsuHitObject> hitObjects)
        {
            this.hitObjects = hitObjects;
            populateHitObjectPositions();
        }

        private void populateHitObjectPositions()
        {
            Vector2 lastPosition = playfield_centre;
            float lastAngle = 0;

            foreach (OsuHitObject hitObject in hitObjects)
            {
                Vector2 relativePosition = hitObject.Position - lastPosition;
                float absoluteAngle = (float)Math.Atan2(relativePosition.Y, relativePosition.X);
                float relativeAngle = absoluteAngle - lastAngle;

                hitObjectPositions.Add(new HitObjectPositionInfo(hitObject)
                {
                    RelativeAngle = relativeAngle,
                    Distance = relativePosition.Length
                });

                lastPosition = hitObject.EndPosition;
                lastAngle = absoluteAngle;
            }
        }

        /// <summary>
        /// Reposition all hit objects according to information in <see cref="HitObjectPositions"/>.
        /// </summary>
        public void RepositionHitObjects()
        {
            HitObjectPositionInfo previous = null;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                var hitObject = hitObjects[i];
                var current = hitObjectPositions[i];

                // Spinners are not moved, but their positions are still considered by subsequent hit objects
                if (hitObject is Spinner)
                {
                    previous = current;
                    continue;
                }

                float lastAngleAbsolute = 0;

                if (i == 1)
                {
                    lastAngleAbsolute = previous!.RelativeAngle;
                }
                else if (i > 1)
                {
                    Vector2 lastPositionRelative = hitObjects[i - 1].Position - hitObjects[i - 2].EndPosition;
                    lastAngleAbsolute = (float)Math.Atan2(lastPositionRelative.Y, lastPositionRelative.X);
                }

                applyModification(lastAngleAbsolute, previous, current);

                // Move hit objects back into the playfield if they are outside of it
                Vector2 shift = Vector2.Zero;

                switch (hitObject)
                {
                    case HitCircle circle:
                        shift = clampHitCircleToPlayfield(circle, current);
                        break;

                    case Slider slider:
                        shift = clampSliderToPlayfield(slider, current);
                        break;
                }

                if (shift != Vector2.Zero)
                {
                    var toBeShifted = new List<OsuHitObject>();

                    for (int j = i - 1; j >= i - preceding_hitobjects_to_shift && j >= 0; j--)
                    {
                        // only shift hit circles
                        if (!(hitObjects[j] is HitCircle)) break;

                        toBeShifted.Add(hitObjects[j]);
                    }

                    if (toBeShifted.Count > 0)
                        applyDecreasingShift(toBeShifted, shift);
                }

                previous = current;
            }
        }

        /// <summary>
        /// Calculate the modified position of a hit object, trying to keep it inside the playfield in the process.
        /// </summary>
        /// <param name="prevAngleAbsolute">The absolute jump angle of the previous object, used for resolving the relative angle of the current object.</param>
        /// <param name="previous">Info for the previous hit object.</param>
        /// <param name="current">Info for the hit object to be processed.</param>
        private void applyModification(float prevAngleAbsolute, HitObjectPositionInfo previous, HitObjectPositionInfo current)
        {
            double absoluteAngle = prevAngleAbsolute + current.RelativeAngle;

            var posRelativeToPrev = new Vector2(
                current.Distance * (float)Math.Cos(absoluteAngle),
                current.Distance * (float)Math.Sin(absoluteAngle)
            );

            Vector2 lastEndPosition = previous?.EndPositionModified ?? playfield_centre;

            posRelativeToPrev = OsuHitObjectGenerationUtils.RotateAwayFromEdge(lastEndPosition, posRelativeToPrev);

            current.RelativeAngle = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X) - prevAngleAbsolute;

            current.PositionModified = lastEndPosition + posRelativeToPrev;
        }

        /// <summary>
        /// Move the modified position of a hit circle so that it fits inside the playfield.
        /// </summary>
        /// <returns>The deviation from the original modified position in order to fit within the playfield.</returns>
        private Vector2 clampHitCircleToPlayfield(HitCircle circle, HitObjectPositionInfo objectInfo)
        {
            var previousPosition = objectInfo.PositionModified;
            objectInfo.EndPositionModified = objectInfo.PositionModified = clampToPlayfield(objectInfo.PositionModified);

            circle.Position = objectInfo.PositionModified;

            return objectInfo.PositionModified - previousPosition;
        }

        /// <summary>
        /// Moves the <see cref="Slider"/> and all necessary nested <see cref="OsuHitObject"/>s into the <see cref="OsuPlayfield"/> if they aren't already.
        /// </summary>
        /// <returns>The deviation from the original modified position in order to fit within the playfield.</returns>
        private Vector2 clampSliderToPlayfield(Slider slider, HitObjectPositionInfo objectInfo)
        {
            var possibleMovementBounds = calculatePossibleMovementBounds(slider);

            var previousPosition = objectInfo.PositionModified;

            // Clamp slider position to the placement area
            // If the slider is larger than the playfield, force it to stay at the original position
            float newX = possibleMovementBounds.Width < 0
                ? objectInfo.PositionOriginal.X
                : Math.Clamp(previousPosition.X, possibleMovementBounds.Left, possibleMovementBounds.Right);

            float newY = possibleMovementBounds.Height < 0
                ? objectInfo.PositionOriginal.Y
                : Math.Clamp(previousPosition.Y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);

            slider.Position = objectInfo.PositionModified = new Vector2(newX, newY);
            objectInfo.EndPositionModified = slider.EndPosition;

            shiftNestedObjects(slider, objectInfo.PositionModified - objectInfo.PositionOriginal);

            return objectInfo.PositionModified - previousPosition;
        }

        /// <summary>
        /// Decreasingly shift a list of <see cref="OsuHitObject"/>s by a specified amount.
        /// The first item in the list is shifted by the largest amount, while the last item is shifted by the smallest amount.
        /// </summary>
        /// <param name="hitObjects">The list of hit objects to be shifted.</param>
        /// <param name="shift">The amount to be shifted.</param>
        private void applyDecreasingShift(IList<OsuHitObject> hitObjects, Vector2 shift)
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
        private RectangleF calculatePossibleMovementBounds(Slider slider)
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
        private void shiftNestedObjects(Slider slider, Vector2 shift)
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
        private Vector2 clampToPlayfield(Vector2 position)
        {
            return new Vector2(
                Math.Clamp(position.X, 0, OsuPlayfield.BASE_SIZE.X),
                Math.Clamp(position.Y, 0, OsuPlayfield.BASE_SIZE.Y)
            );
        }

        public interface IHitObjectPositionInfo
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
            /// <see cref="Distance"/> of the first hit object in a beatmap is relative to the playfield center.
            /// </remarks>
            public float Distance { get; set; }

            public OsuHitObject HitObject { get; }
        }

        private class HitObjectPositionInfo : IHitObjectPositionInfo
        {
            public float RelativeAngle { get; set; }

            public float Distance { get; set; }

            public OsuHitObject HitObject { get; }

            public Vector2 PositionOriginal { get; }
            public Vector2 PositionModified { get; set; }
            public Vector2 EndPositionModified { get; set; }

            public HitObjectPositionInfo(OsuHitObject hitObject)
            {
                HitObject = hitObject;
                PositionOriginal = PositionModified = hitObject.Position;
                EndPositionModified = hitObject.EndPosition;
            }
        }
    }
}
