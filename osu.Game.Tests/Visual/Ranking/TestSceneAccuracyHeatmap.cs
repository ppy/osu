// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneAccuracyHeatmap : OsuManualInputManagerTestScene
    {
        private readonly Box background;
        private readonly Drawable object1;
        private readonly Drawable object2;
        private readonly Heatmap heatmap;

        public TestSceneAccuracyHeatmap()
        {
            Children = new[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#333"),
                },
                object1 = new BorderCircle
                {
                    Position = new Vector2(256, 192),
                    Colour = Color4.Yellow,
                },
                object2 = new BorderCircle
                {
                    Position = new Vector2(500, 300),
                },
                heatmap = new Heatmap
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(() =>
            {
                var randomPos = new Vector2(
                    RNG.NextSingle(object1.DrawPosition.X - object1.DrawSize.X / 2, object1.DrawPosition.X + object1.DrawSize.X / 2),
                    RNG.NextSingle(object1.DrawPosition.Y - object1.DrawSize.Y / 2, object1.DrawPosition.Y + object1.DrawSize.Y / 2));

                // The background is used for ToLocalSpace() since we need to go _inside_ the DrawSizePreservingContainer (Content of TestScene).
                heatmap.AddPoint(object2.Position, object1.Position, randomPos, RNG.NextSingle(10, 500));
                InputManager.MoveMouseTo(background.ToScreenSpace(randomPos));
            }, 1, true);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            heatmap.AddPoint(object2.Position, object1.Position, background.ToLocalSpace(e.ScreenSpaceMouseDownPosition), 50);
            return true;
        }

        private class Heatmap : CompositeDrawable
        {
            /// <summary>
            /// Full size of the heatmap.
            /// </summary>
            private const float size = 130;

            /// <summary>
            /// Size of the inner circle containing the "hit" points, relative to <see cref="size"/>.
            /// All other points outside of the inner circle are "miss" points.
            /// </summary>
            private const float inner_portion = 0.8f;

            private const float rotation = 45;
            private const float point_size = 4;

            private Container<HitPoint> allPoints;

            public Heatmap()
            {
                Size = new Vector2(size);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
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
                    allPoints = new Container<HitPoint> { RelativeSizeAxes = Axes.Both }
                };

                Vector2 centre = new Vector2(size / 2);
                int rows = (int)Math.Ceiling(size / point_size);
                int cols = (int)Math.Ceiling(size / point_size);

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        Vector2 pos = new Vector2(c * point_size, r * point_size);
                        HitPointType pointType = HitPointType.Hit;

                        if (Vector2.Distance(pos, centre) > size * inner_portion / 2)
                            pointType = HitPointType.Miss;

                        allPoints.Add(new HitPoint(pos, pointType)
                        {
                            Size = new Vector2(point_size),
                            Colour = pointType == HitPointType.Hit ? new Color4(102, 255, 204, 255) : new Color4(255, 102, 102, 255)
                        });
                    }
                }
            }

            public void AddPoint(Vector2 start, Vector2 end, Vector2 hitPoint, float radius)
            {
                double angle1 = Math.Atan2(end.Y - hitPoint.Y, hitPoint.X - end.X); // Angle between the end point and the hit point.
                double angle2 = Math.Atan2(end.Y - start.Y, start.X - end.X); // Angle between the end point and the start point.
                double finalAngle = angle2 - angle1; // Angle between start, end, and hit points.

                float normalisedDistance = Vector2.Distance(hitPoint, end) / radius;

                // Find the most relevant hit point.
                double minDist = double.PositiveInfinity;
                HitPoint point = null;

                foreach (var p in allPoints)
                {
                    Vector2 localCentre = new Vector2(size / 2);
                    float localRadius = localCentre.X * inner_portion * normalisedDistance;
                    double localAngle = finalAngle + 3 * Math.PI / 4;
                    Vector2 localPoint = localCentre + localRadius * new Vector2((float)Math.Cos(localAngle), (float)Math.Sin(localAngle));

                    float dist = Vector2.Distance(p.DrawPosition + p.DrawSize / 2, localPoint);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        point = p;
                    }
                }

                Debug.Assert(point != null);
                point.Increment();
            }
        }

        private class HitPoint : Circle
        {
            private readonly HitPointType pointType;

            public HitPoint(Vector2 position, HitPointType pointType)
            {
                this.pointType = pointType;

                Position = position;
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

        private class BorderCircle : CircularContainer
        {
            public BorderCircle()
            {
                Origin = Anchor.Centre;
                Size = new Vector2(100);

                Masking = true;
                BorderThickness = 2;
                BorderColour = Color4.White;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    },
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(4),
                    }
                };
            }
        }
    }
}
