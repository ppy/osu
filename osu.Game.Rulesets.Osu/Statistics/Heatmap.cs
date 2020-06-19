// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Statistics
{
    public class Heatmap : CompositeDrawable
    {
        /// <summary>
        /// Size of the inner circle containing the "hit" points, relative to the size of this <see cref="Heatmap"/>.
        /// All other points outside of the inner circle are "miss" points.
        /// </summary>
        private const float inner_portion = 0.8f;

        /// <summary>
        /// Number of rows/columns of points.
        /// 4px per point @ 128x128 size (the contents of the <see cref="Heatmap"/> are always square). 1024 total points.
        /// </summary>
        private const int points_per_dimension = 32;

        private const float rotation = 45;

        private GridContainer pointGrid;

        private readonly BeatmapInfo beatmap;
        private readonly IReadOnlyList<HitEvent> hitEvents;
        private readonly LayoutValue sizeLayout = new LayoutValue(Invalidation.DrawSize);

        public Heatmap(BeatmapInfo beatmap, IReadOnlyList<HitEvent> hitEvents)
        {
            this.beatmap = beatmap;
            this.hitEvents = hitEvents;

            AddLayout(sizeLayout);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Children = new Drawable[]
                {
                    new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(inner_portion),
                        Masking = true,
                        BorderThickness = 2f,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#202624")
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Y,
                                Height = 2, // We're rotating along a diagonal - we don't really care how big this is.
                                Width = 1f,
                                Rotation = -rotation,
                                Alpha = 0.3f,
                            },
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Y,
                                Height = 2, // We're rotating along a diagonal - we don't really care how big this is.
                                Width = 1f,
                                Rotation = rotation
                            },
                            new Box
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Width = 10,
                                Height = 2f,
                            },
                            new Box
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Y = -1,
                                Width = 2f,
                                Height = 10,
                            }
                        }
                    },
                    pointGrid = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };

            Vector2 centre = new Vector2(points_per_dimension) / 2;
            float innerRadius = centre.X * inner_portion;

            Drawable[][] points = new Drawable[points_per_dimension][];

            for (int r = 0; r < points_per_dimension; r++)
            {
                points[r] = new Drawable[points_per_dimension];

                for (int c = 0; c < points_per_dimension; c++)
                {
                    HitPointType pointType = Vector2.Distance(new Vector2(c, r), centre) <= innerRadius
                        ? HitPointType.Hit
                        : HitPointType.Miss;

                    var point = new HitPoint(pointType)
                    {
                        Colour = pointType == HitPointType.Hit ? new Color4(102, 255, 204, 255) : new Color4(255, 102, 102, 255)
                    };

                    points[r][c] = point;
                }
            }

            pointGrid.Content = points;

            if (hitEvents.Count > 0)
            {
                // Todo: This should probably not be done like this.
                float radius = OsuHitObject.OBJECT_RADIUS * (1.0f - 0.7f * (beatmap.BaseDifficulty.CircleSize - 5) / 5) / 2;

                foreach (var e in hitEvents)
                {
                    if (e.LastHitObject == null || e.PositionOffset == null)
                        continue;

                    AddPoint(((OsuHitObject)e.LastHitObject).StackedEndPosition, ((OsuHitObject)e.HitObject).StackedEndPosition, e.PositionOffset.Value, radius);
                }
            }
        }

        protected void AddPoint(Vector2 start, Vector2 end, Vector2 hitPoint, float radius)
        {
            if (pointGrid.Content.Length == 0)
                return;

            double angle1 = Math.Atan2(end.Y - hitPoint.Y, hitPoint.X - end.X); // Angle between the end point and the hit point.
            double angle2 = Math.Atan2(end.Y - start.Y, start.X - end.X); // Angle between the end point and the start point.
            double finalAngle = angle2 - angle1; // Angle between start, end, and hit points.
            float normalisedDistance = Vector2.Distance(hitPoint, end) / radius;

            // Convert the above into the local search space.
            Vector2 localCentre = new Vector2(points_per_dimension) / 2;
            float localRadius = localCentre.X * inner_portion * normalisedDistance; // The radius inside the inner portion which of the heatmap which the closest point lies.
            double localAngle = finalAngle + 3 * Math.PI / 4; // The angle inside the heatmap on which the closest point lies.
            Vector2 localPoint = localCentre + localRadius * new Vector2((float)Math.Cos(localAngle), (float)Math.Sin(localAngle));

            // Find the most relevant hit point.
            double minDist = double.PositiveInfinity;
            HitPoint point = null;

            for (int r = 0; r < points_per_dimension; r++)
            {
                for (int c = 0; c < points_per_dimension; c++)
                {
                    float dist = Vector2.Distance(new Vector2(c, r), localPoint);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        point = (HitPoint)pointGrid.Content[r][c];
                    }
                }
            }

            Debug.Assert(point != null);
            point.Increment();
        }

        private class HitPoint : Circle
        {
            private readonly HitPointType pointType;

            public HitPoint(HitPointType pointType)
            {
                this.pointType = pointType;

                RelativeSizeAxes = Axes.Both;
                Alpha = 0;
            }

            public void Increment()
            {
                if (Alpha < 1)
                    Alpha += 0.1f;
                else if (pointType == HitPointType.Hit)
                    Colour = ((Color4)Colour).Lighten(0.1f);
            }
        }

        private enum HitPointType
        {
            Hit,
            Miss
        }
    }
}
