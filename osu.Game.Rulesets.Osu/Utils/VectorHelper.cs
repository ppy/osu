// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Utils
{
    public static class VectorHelper
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
            var relativeRotationDistance = 0f;

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

            return RotateVectorTowardsVector(posRelativeToPrev, playfield_middle - prevObjectPos, relativeRotationDistance * rotationRatio);
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
            var initialAngleRad = Math.Atan2(initial.Y, initial.X);
            var destAngleRad = Math.Atan2(destination.Y, destination.X);

            var diff = destAngleRad - initialAngleRad;

            while (diff < -Math.PI) diff += 2 * Math.PI;

            while (diff > Math.PI) diff -= 2 * Math.PI;

            var finalAngleRad = initialAngleRad + rotationRatio * diff;

            return new Vector2(
                initial.Length * (float)Math.Cos(finalAngleRad),
                initial.Length * (float)Math.Sin(finalAngleRad)
            );
        }
    }
}
