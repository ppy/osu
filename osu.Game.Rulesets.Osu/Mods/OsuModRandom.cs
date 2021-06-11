// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that randomises the positions of the <see cref="HitObject"/>s
    /// </summary>
    public class OsuModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => "It never gets boring!";

        // The relative distance to the edge of the playfield before objects' positions should start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const float playfield_edge_ratio = 0.375f;

        private static readonly float border_distance_x = OsuPlayfield.BASE_SIZE.X * playfield_edge_ratio;
        private static readonly float border_distance_y = OsuPlayfield.BASE_SIZE.Y * playfield_edge_ratio;

        private static readonly Vector2 playfield_middle = OsuPlayfield.BASE_SIZE / 2;

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

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

                // rateOfChangeMultiplier only changes every i iterations to prevent shaky-line-shaped streams
                if (i % 3 == 0)
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;

                if (hitObject is Spinner)
                {
                    previous = null;
                    continue;
                }

                applyRandomisation(rateOfChangeMultiplier, previous, current);

                hitObject.Position = current.PositionRandomised;

                // update end position as it may have changed as a result of the position update.
                current.EndPositionRandomised = current.PositionRandomised;

                if (hitObject is Slider slider)
                    moveSliderIntoPlayfield(slider, current);

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
            var randomAngleRad = rateOfChangeMultiplier * 2 * Math.PI * distanceToPrev / playfield_diagonal;

            current.AngleRad = (float)randomAngleRad + previous.AngleRad;
            if (current.AngleRad < 0)
                current.AngleRad += 2 * (float)Math.PI;

            var posRelativeToPrev = new Vector2(
                distanceToPrev * (float)Math.Cos(current.AngleRad),
                distanceToPrev * (float)Math.Sin(current.AngleRad)
            );

            posRelativeToPrev = getRotatedVector(previous.EndPositionRandomised, posRelativeToPrev);

            current.AngleRad = (float)Math.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);

            var position = previous.EndPositionRandomised + posRelativeToPrev;

            // Move hit objects back into the playfield if they are outside of it,
            // which would sometimes happen during big jumps otherwise.
            position.X = MathHelper.Clamp(position.X, 0, OsuPlayfield.BASE_SIZE.X);
            position.Y = MathHelper.Clamp(position.Y, 0, OsuPlayfield.BASE_SIZE.Y);

            current.PositionRandomised = position;
        }

        /// <summary>
        /// Moves the <see cref="Slider"/> and all necessary nested <see cref="OsuHitObject"/>s into the <see cref="OsuPlayfield"/> if they aren't already.
        /// </summary>
        private void moveSliderIntoPlayfield(Slider slider, RandomObjectInfo currentObjectInfo)
        {
            var minMargin = getMinSliderMargin(slider);

            slider.Position = new Vector2(
                Math.Clamp(slider.Position.X, minMargin.Left, OsuPlayfield.BASE_SIZE.X - minMargin.Right),
                Math.Clamp(slider.Position.Y, minMargin.Top, OsuPlayfield.BASE_SIZE.Y - minMargin.Bottom)
            );

            currentObjectInfo.PositionRandomised = slider.Position;
            currentObjectInfo.EndPositionRandomised = slider.EndPosition;

            shiftNestedObjects(slider, currentObjectInfo.PositionRandomised - currentObjectInfo.PositionOriginal);
        }

        /// <summary>
        /// Calculates the min. distances from the <see cref="Slider"/>'s position to the playfield border for the slider to be fully inside of the playfield.
        /// </summary>
        private MarginPadding getMinSliderMargin(Slider slider)
        {
            var pathPositions = new List<Vector2>();
            slider.Path.GetPathToProgress(pathPositions, 0, 1);

            var minMargin = new MarginPadding();

            foreach (var pos in pathPositions)
            {
                minMargin.Left = Math.Max(minMargin.Left, -pos.X);
                minMargin.Right = Math.Max(minMargin.Right, pos.X);
                minMargin.Top = Math.Max(minMargin.Top, -pos.Y);
                minMargin.Bottom = Math.Max(minMargin.Bottom, pos.Y);
            }

            minMargin.Left = Math.Min(minMargin.Left, OsuPlayfield.BASE_SIZE.X - minMargin.Right);
            minMargin.Top = Math.Min(minMargin.Top, OsuPlayfield.BASE_SIZE.Y - minMargin.Bottom);

            return minMargin;
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
        /// Determines the position of the current hit object relative to the previous one.
        /// </summary>
        /// <returns>The position of the current hit object relative to the previous one</returns>
        private Vector2 getRotatedVector(Vector2 prevPosChanged, Vector2 posRelativeToPrev)
        {
            var relativeRotationDistance = 0f;

            if (prevPosChanged.X < playfield_middle.X)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_x - prevPosChanged.X) / border_distance_x,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevPosChanged.X - (OsuPlayfield.BASE_SIZE.X - border_distance_x)) / border_distance_x,
                    relativeRotationDistance
                );
            }

            if (prevPosChanged.Y < playfield_middle.Y)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_y - prevPosChanged.Y) / border_distance_y,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevPosChanged.Y - (OsuPlayfield.BASE_SIZE.Y - border_distance_y)) / border_distance_y,
                    relativeRotationDistance
                );
            }

            return rotateVectorTowardsVector(posRelativeToPrev, playfield_middle - prevPosChanged, relativeRotationDistance / 2);
        }

        /// <summary>
        /// Rotates vector "initial" towards vector "destinantion"
        /// </summary>
        /// <param name="initial">Vector to rotate to "destination"</param>
        /// <param name="destination">Vector "initial" should be rotated to</param>
        /// <param name="relativeDistance">The angle the vector should be rotated relative to the difference between the angles of the the two vectors.</param>
        /// <returns>Resulting vector</returns>
        private Vector2 rotateVectorTowardsVector(Vector2 initial, Vector2 destination, float relativeDistance)
        {
            var initialAngleRad = Math.Atan2(initial.Y, initial.X);
            var destAngleRad = Math.Atan2(destination.Y, destination.X);

            var diff = destAngleRad - initialAngleRad;

            while (diff < -Math.PI) diff += 2 * Math.PI;

            while (diff > Math.PI) diff -= 2 * Math.PI;

            var finalAngleRad = initialAngleRad + relativeDistance * diff;

            return new Vector2(
                initial.Length * (float)Math.Cos(finalAngleRad),
                initial.Length * (float)Math.Sin(finalAngleRad)
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
