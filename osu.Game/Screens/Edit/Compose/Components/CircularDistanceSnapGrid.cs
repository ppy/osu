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
        protected CircularDistanceSnapGrid(HitObject hitObject, Vector2 centrePosition)
            : base(hitObject, centrePosition)
        {
        }

        protected override void CreateContent(Vector2 centrePosition)
        {
            const float crosshair_thickness = 1;
            const float crosshair_max_size = 10;

            AddRangeInternal(new[]
            {
                new Box
                {
                    Origin = Anchor.Centre,
                    Position = centrePosition,
                    Width = crosshair_thickness,
                    EdgeSmoothness = new Vector2(1),
                    Height = Math.Min(crosshair_max_size, DistanceSpacing * 2),
                },
                new Box
                {
                    Origin = Anchor.Centre,
                    Position = centrePosition,
                    EdgeSmoothness = new Vector2(1),
                    Width = Math.Min(crosshair_max_size, DistanceSpacing * 2),
                    Height = crosshair_thickness,
                }
            });

            float dx = Math.Max(centrePosition.X, DrawWidth - centrePosition.X);
            float dy = Math.Max(centrePosition.Y, DrawHeight - centrePosition.Y);
            float maxDistance = new Vector2(dx, dy).Length;
            int requiredCircles = (int)(maxDistance / DistanceSpacing);

            for (int i = 0; i < requiredCircles; i++)
            {
                float radius = (i + 1) * DistanceSpacing * 2;

                AddInternal(new CircularProgress
                {
                    Origin = Anchor.Centre,
                    Position = centrePosition,
                    Current = { Value = 1 },
                    Size = new Vector2(radius),
                    InnerRadius = 4 * 1f / radius,
                    Colour = GetColourForBeatIndex(i)
                });
            }
        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position)
        {
            Vector2 direction = position - CentrePosition;

            if (direction == Vector2.Zero)
                direction = new Vector2(0.001f, 0.001f);

            float distance = direction.Length;

            float radius = DistanceSpacing;
            int radialCount = Math.Max(1, (int)Math.Round(distance / radius));

            Vector2 normalisedDirection = direction * new Vector2(1f / distance);
            Vector2 snappedPosition = CentrePosition + normalisedDirection * radialCount * radius;

            return (snappedPosition, StartTime + SnapProvider.GetSnappedDurationFromDistance(StartTime, (snappedPosition - CentrePosition).Length));
        }
    }
}
