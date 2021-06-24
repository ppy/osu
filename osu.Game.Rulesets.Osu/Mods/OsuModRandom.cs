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

            posRelativeToPrev = VectorHelper.RotateAwayFromEdge(previous.EndPositionRandomised, posRelativeToPrev);

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
