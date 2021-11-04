// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that randomises the positions of the <see cref="HitObject"/>s
    /// </summary>
    public class OsuModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => "It never gets boring!";

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

        /// <summary>
        /// Number of previous hitobjects to be shifted together when another object is being moved.
        /// </summary>
        private const int preceding_hitobjects_to_shift = 10;

        private Random rng;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            var hitObjects = osuBeatmap.HitObjects;

            Seed.Value ??= RNG.Next();

            rng = new Random((int)Seed.Value);

            RandomObjectInfo previous = null;

            float rateOfChangeMultiplier = 0;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                var hitObject = hitObjects[i];

                var current = new RandomObjectInfo(hitObject);

                // rateOfChangeMultiplier only changes every 5 iterations in a combo
                // to prevent shaky-line-shaped streams
                if (hitObject.IndexInCurrentCombo % 5 == 0)
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;

                if (hitObject is Spinner)
                {
                    previous = null;
                    continue;
                }

                applyRandomisation(rateOfChangeMultiplier, previous, current);

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
        /// Returns the final position of the hit object
        /// </summary>
        /// <returns>Final position of the hit object</returns>
        private void applyRandomisation(float rateOfChangeMultiplier, RandomObjectInfo previous, RandomObjectInfo current)
        {
            if (previous == null)
            {
                var playfieldSize = OsuPlayfield.BASE_SIZE;

                current.AngleRad = (float)(rng.NextDouble() * 2 * Math.PI - Math.PI);
                current.PositionRandomised = new Vector2((float)rng.NextDouble() * playfieldSize.X, (float)rng.NextDouble() * playfieldSize.Y);

                return;
            }

            float distanceToPrev = Vector2.Distance(previous.EndPositionOriginal, current.PositionOriginal);

            // The max. angle (relative to the angle of the vector pointing from the 2nd last to the last hit object)
            // is proportional to the distance between the last and the current hit object
            // to allow jumps and prevent too sharp turns during streams.

            // Allow maximum jump angle when jump distance is more than half of playfield diagonal length
            double randomAngleRad = rateOfChangeMultiplier * 2 * Math.PI * Math.Min(1f, distanceToPrev / (playfield_diagonal * 0.5f));

            current.AngleRad = (float)randomAngleRad + previous.AngleRad;
            if (current.AngleRad < 0)
                current.AngleRad += 2 * (float)Math.PI;

            var posRelativeToPrev = new Vector2(
                distanceToPrev * (float)Math.Cos(current.AngleRad),
                distanceToPrev * (float)Math.Sin(current.AngleRad)
            );

            posRelativeToPrev = OsuHitObjectGenerationUtils.RotateAwayFromEdge(previous.EndPositionRandomised, posRelativeToPrev);

            current.AngleRad = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);

            current.PositionRandomised = previous.EndPositionRandomised + posRelativeToPrev;
        }

        /// <summary>
        /// Move the randomised position of a hit circle so that it fits inside the playfield.
        /// </summary>
        /// <returns>The deviation from the original randomised position in order to fit within the playfield.</returns>
        private Vector2 clampHitCircleToPlayfield(HitCircle circle, RandomObjectInfo objectInfo)
        {
            var previousPosition = objectInfo.PositionRandomised;
            objectInfo.EndPositionRandomised = objectInfo.PositionRandomised = clampToPlayfieldWithPadding(
                objectInfo.PositionRandomised,
                (float)circle.Radius
            );

            circle.Position = objectInfo.PositionRandomised;

            return objectInfo.PositionRandomised - previousPosition;
        }

        /// <summary>
        /// Moves the <see cref="Slider"/> and all necessary nested <see cref="OsuHitObject"/>s into the <see cref="OsuPlayfield"/> if they aren't already.
        /// </summary>
        /// <returns>The deviation from the original randomised position in order to fit within the playfield.</returns>
        private Vector2 clampSliderToPlayfield(Slider slider, RandomObjectInfo objectInfo)
        {
            var possibleMovementBounds = calculatePossibleMovementBounds(slider);

            var previousPosition = objectInfo.PositionRandomised;

            // Clamp slider position to the placement area
            // If the slider is larger than the playfield, force it to stay at the original position
            float newX = possibleMovementBounds.Width < 0
                ? objectInfo.PositionOriginal.X
                : Math.Clamp(previousPosition.X, possibleMovementBounds.Left, possibleMovementBounds.Right);

            float newY = possibleMovementBounds.Height < 0
                ? objectInfo.PositionOriginal.Y
                : Math.Clamp(previousPosition.Y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);

            slider.Position = objectInfo.PositionRandomised = new Vector2(newX, newY);
            objectInfo.EndPositionRandomised = slider.EndPosition;

            shiftNestedObjects(slider, objectInfo.PositionRandomised - objectInfo.PositionOriginal);

            return objectInfo.PositionRandomised - previousPosition;
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
        /// Clamp a position to playfield, keeping a specified distance from the edges.
        /// </summary>
        /// <param name="position">The position to be clamped.</param>
        /// <param name="padding">The minimum distance allowed from playfield edges.</param>
        /// <returns>The clamped position.</returns>
        private Vector2 clampToPlayfieldWithPadding(Vector2 position, float padding)
        {
            return new Vector2(
                Math.Clamp(position.X, padding, OsuPlayfield.BASE_SIZE.X - padding),
                Math.Clamp(position.Y, padding, OsuPlayfield.BASE_SIZE.Y - padding)
            );
        }

        private class RandomObjectInfo
        {
            public float AngleRad { get; set; }

            public Vector2 PositionOriginal { get; }
            public Vector2 PositionRandomised { get; set; }

            public Vector2 EndPositionOriginal { get; }
            public Vector2 EndPositionRandomised { get; set; }

            public RandomObjectInfo(OsuHitObject hitObject)
            {
                PositionRandomised = PositionOriginal = hitObject.Position;
                EndPositionRandomised = EndPositionOriginal = hitObject.EndPosition;
                AngleRad = 0;
            }
        }
    }
}
