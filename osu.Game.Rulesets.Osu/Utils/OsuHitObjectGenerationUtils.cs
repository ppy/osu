// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Utils
{
    public static partial class OsuHitObjectGenerationUtils
    {
        // The relative distance to the edge of the playfield before objects' positions should start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const float playfield_edge_ratio = 0.375f;

        private static readonly float border_distance_x = OsuPlayfield.BASE_SIZE.X * playfield_edge_ratio;
        private static readonly float border_distance_y = OsuPlayfield.BASE_SIZE.Y * playfield_edge_ratio;

        private static readonly Vector2 playfield_middle = OsuPlayfield.BASE_SIZE / 2;

        /// <summary>
        /// Rotate a hit object away from the playfield edge, while keeping a constant distance
        /// from the previous object.
        /// </summary>
        /// <remarks>
        /// The extent of rotation depends on the position of the hit object. Hit objects
        /// closer to the playfield edge will be rotated to a larger extent.
        /// </remarks>
        /// <param name="prevObjectPos">Position of the previous hit object.</param>
        /// <param name="posRelativeToPrev">Position of the hit object to be rotated, relative to the previous hit object.</param>
        /// <param name="rotationRatio">
        /// The extent of rotation.
        /// 0 means the hit object is never rotated.
        /// 1 means the hit object will be fully rotated towards playfield center when it is originally at playfield edge.
        /// </param>
        /// <returns>The new position of the hit object, relative to the previous one.</returns>
        public static Vector2 RotateAwayFromEdge(Vector2 prevObjectPos, Vector2 posRelativeToPrev, float rotationRatio = 0.5f)
        {
            float relativeRotationDistance = 0f;

            if (prevObjectPos.X < playfield_middle.X)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_x - prevObjectPos.X) / border_distance_x,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevObjectPos.X - (OsuPlayfield.BASE_SIZE.X - border_distance_x)) / border_distance_x,
                    relativeRotationDistance
                );
            }

            if (prevObjectPos.Y < playfield_middle.Y)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_y - prevObjectPos.Y) / border_distance_y,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevObjectPos.Y - (OsuPlayfield.BASE_SIZE.Y - border_distance_y)) / border_distance_y,
                    relativeRotationDistance
                );
            }

            return RotateVectorTowardsVector(
                posRelativeToPrev,
                playfield_middle - prevObjectPos,
                Math.Min(1, relativeRotationDistance * rotationRatio)
            );
        }

        /// <summary>
        /// Rotates vector "initial" towards vector "destination".
        /// </summary>
        /// <param name="initial">The vector to be rotated.</param>
        /// <param name="destination">The vector that "initial" should be rotated towards.</param>
        /// <param name="rotationRatio">How much "initial" should be rotated. 0 means no rotation. 1 means "initial" is fully rotated to equal "destination".</param>
        /// <returns>The rotated vector.</returns>
        public static Vector2 RotateVectorTowardsVector(Vector2 initial, Vector2 destination, float rotationRatio)
        {
            float initialAngleRad = MathF.Atan2(initial.Y, initial.X);
            float destAngleRad = MathF.Atan2(destination.Y, destination.X);

            float diff = destAngleRad - initialAngleRad;

            while (diff < -MathF.PI) diff += 2 * MathF.PI;

            while (diff > MathF.PI) diff -= 2 * MathF.PI;

            float finalAngleRad = initialAngleRad + rotationRatio * diff;

            return new Vector2(
                initial.Length * MathF.Cos(finalAngleRad),
                initial.Length * MathF.Sin(finalAngleRad)
            );
        }

        /// <summary>
        /// Reflects the position of the <see cref="OsuHitObject"/> in the playfield horizontally.
        /// </summary>
        /// <param name="osuObject">The object to reflect.</param>
        public static void ReflectHorizontallyAlongPlayfield(OsuHitObject osuObject)
        {
            osuObject.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - osuObject.X, osuObject.Position.Y);

            if (osuObject is not Slider slider)
                return;

            void reflectNestedObject(OsuHitObject nested) => nested.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - nested.Position.X, nested.Position.Y);
            static void reflectControlPoint(PathControlPoint point) => point.Position = new Vector2(-point.Position.X, point.Position.Y);

            modifySlider(slider, reflectNestedObject, reflectControlPoint);
        }

        /// <summary>
        /// Reflects the position of the <see cref="OsuHitObject"/> in the playfield vertically.
        /// </summary>
        /// <param name="osuObject">The object to reflect.</param>
        public static void ReflectVerticallyAlongPlayfield(OsuHitObject osuObject)
        {
            osuObject.Position = new Vector2(osuObject.Position.X, OsuPlayfield.BASE_SIZE.Y - osuObject.Y);

            if (osuObject is not Slider slider)
                return;

            void reflectNestedObject(OsuHitObject nested) => nested.Position = new Vector2(nested.Position.X, OsuPlayfield.BASE_SIZE.Y - nested.Position.Y);
            static void reflectControlPoint(PathControlPoint point) => point.Position = new Vector2(point.Position.X, -point.Position.Y);

            modifySlider(slider, reflectNestedObject, reflectControlPoint);
        }

        /// <summary>
        /// Flips the position of the <see cref="Slider"/> around its start position horizontally.
        /// </summary>
        /// <param name="slider">The slider to be flipped.</param>
        public static void FlipSliderInPlaceHorizontally(Slider slider)
        {
            void flipNestedObject(OsuHitObject nested) => nested.Position = new Vector2(slider.X - (nested.X - slider.X), nested.Y);
            static void flipControlPoint(PathControlPoint point) => point.Position = new Vector2(-point.Position.X, point.Position.Y);

            modifySlider(slider, flipNestedObject, flipControlPoint);
        }

        /// <summary>
        /// Rotate a slider about its start position by the specified angle.
        /// </summary>
        /// <param name="slider">The slider to be rotated.</param>
        /// <param name="rotation">The angle, measured in radians, to rotate the slider by.</param>
        public static void RotateSlider(Slider slider, float rotation)
        {
            void rotateNestedObject(OsuHitObject nested) => nested.Position = rotateVector(nested.Position - slider.Position, rotation) + slider.Position;
            void rotateControlPoint(PathControlPoint point) => point.Position = rotateVector(point.Position, rotation);

            modifySlider(slider, rotateNestedObject, rotateControlPoint);
        }

        private static void modifySlider(Slider slider, Action<OsuHitObject> modifyNestedObject, Action<PathControlPoint> modifyControlPoint)
        {
            // No need to update the head and tail circles, since slider handles that when the new slider path is set
            slider.NestedHitObjects.OfType<SliderTick>().ForEach(modifyNestedObject);
            slider.NestedHitObjects.OfType<SliderRepeat>().ForEach(modifyNestedObject);

            var controlPoints = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                modifyControlPoint(point);

            slider.Path = new SliderPath(controlPoints, slider.Path.ExpectedDistance.Value);
        }

        /// <summary>
        /// Rotate a vector by the specified angle.
        /// </summary>
        /// <param name="vector">The vector to be rotated.</param>
        /// <param name="rotation">The angle, measured in radians, to rotate the vector by.</param>
        /// <returns>The rotated vector.</returns>
        private static Vector2 rotateVector(Vector2 vector, float rotation)
        {
            float angle = MathF.Atan2(vector.Y, vector.X) + rotation;
            float length = vector.Length;
            return new Vector2(
                length * MathF.Cos(angle),
                length * MathF.Sin(angle)
            );
        }

        /// <param name="beatmap">The beatmap hitObject is a part of.</param>
        /// <param name="hitObject">The <see cref="OsuHitObject"/> that should be checked.</param>
        /// <param name="downbeatsOnly">If true, this method only returns true if hitObject is on a downbeat.
        /// If false, it returns true if hitObject is on any beat.</param>
        /// <returns>true if hitObject is on a (down-)beat, false otherwise.</returns>
        public static bool IsHitObjectOnBeat(OsuBeatmap beatmap, OsuHitObject hitObject, bool downbeatsOnly = false)
        {
            var timingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);

            double timeSinceTimingPoint = hitObject.StartTime - timingPoint.Time;

            double beatLength = timingPoint.BeatLength;

            if (downbeatsOnly)
                beatLength *= timingPoint.TimeSignature.Numerator;

            // Ensure within 1ms of expected location.
            return Math.Abs(timeSinceTimingPoint + 1) % beatLength < 2;
        }

        /// <summary>
        /// Generates a random number from a normal distribution using the Box-Muller transform.
        /// </summary>
        public static float RandomGaussian(Random rng, float mean = 0, float stdDev = 1)
        {
            // Generate 2 random numbers in the interval (0,1].
            // x1 must not be 0 since log(0) = undefined.
            double x1 = 1 - rng.NextDouble();
            double x2 = 1 - rng.NextDouble();

            double stdNormal = Math.Sqrt(-2 * Math.Log(x1)) * Math.Sin(2 * Math.PI * x2);
            return mean + stdDev * (float)stdNormal;
        }
    }
}
