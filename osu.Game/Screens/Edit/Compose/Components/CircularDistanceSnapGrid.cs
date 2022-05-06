// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract class CircularDistanceSnapGrid : DistanceSnapGrid
    {
        protected CircularDistanceSnapGrid(HitObject referenceObject, Vector2 startPosition, double startTime, double? endTime = null)
            : base(referenceObject, startPosition, startTime, endTime)
        {
        }

        protected override void CreateContent()
        {
            const float crosshair_thickness = 1;
            const float crosshair_max_size = 10;

            AddRangeInternal(new[]
            {
                new Box
                {
                    Origin = Anchor.Centre,
                    Position = StartPosition,
                    Width = crosshair_thickness,
                    EdgeSmoothness = new Vector2(1),
                    Height = Math.Min(crosshair_max_size, DistanceBetweenTicks * 2),
                },
                new Box
                {
                    Origin = Anchor.Centre,
                    Position = StartPosition,
                    EdgeSmoothness = new Vector2(1),
                    Width = Math.Min(crosshair_max_size, DistanceBetweenTicks * 2),
                    Height = crosshair_thickness,
                }
            });

            float dx = Math.Max(StartPosition.X, DrawWidth - StartPosition.X);
            float dy = Math.Max(StartPosition.Y, DrawHeight - StartPosition.Y);
            float maxDistance = new Vector2(dx, dy).Length;
            int requiredCircles = Math.Min(MaxIntervals, (int)(maxDistance / DistanceBetweenTicks));

            for (int i = 0; i < requiredCircles; i++)
            {
                float radius = (i + 1) * DistanceBetweenTicks * 2;

                AddInternal(new CircularProgress
                {
                    Origin = Anchor.Centre,
                    Position = StartPosition,
                    Current = { Value = 1 },
                    Size = new Vector2(radius),
                    InnerRadius = 4 * 1f / radius,
                    Colour = GetColourForIndexFromPlacement(i)
                });
            }
        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position)
        {
            if (MaxIntervals == 0)
                return (StartPosition, StartTime);

            // This grid implementation factors in the user's distance spacing specification,
            // which is usually not considered by an `IDistanceSnapProvider`.
            float distanceSpacing = (float)DistanceSpacingMultiplier.Value;

            Vector2 travelVector = (position - StartPosition);

            if (travelVector == Vector2.Zero)
                return (StartPosition, StartTime);

            float travelLength = travelVector.Length;

            // FindSnappedDistance will always round down, but we want to potentially round upwards.
            travelLength += DistanceBetweenTicks / 2;

            // When interacting with the resolved snap provider, the distance spacing multiplier should first be removed
            // to allow for snapping at a non-multiplied ratio.
            float snappedDistance = SnapProvider.FindSnappedDistance(ReferenceObject, travelLength / distanceSpacing);
            double snappedTime = StartTime + SnapProvider.DistanceToDuration(ReferenceObject, snappedDistance);

            if (snappedTime > LatestEndTime)
            {
                snappedDistance = SnapProvider.DurationToDistance(ReferenceObject, LatestEndTime.Value - ReferenceObject.StartTime);
            }

            // The multiplier can then be reapplied to the final position.
            Vector2 snappedPosition = StartPosition + travelVector.Normalized() * snappedDistance * distanceSpacing;

            return (snappedPosition, snappedTime);
        }
    }
}
