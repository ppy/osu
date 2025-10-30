// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Statistics
{
    public partial class AccuracyHeatmap : CompositeDrawable
    {
        /// <summary>
        /// Size of the inner circle containing the "hit" points, relative to the size of this <see cref="AccuracyHeatmap"/>.
        /// All other points outside of the inner circle are "miss" points.
        /// </summary>
        private const float inner_portion = 0.8f;

        /// <summary>
        /// Number of rows/columns of points.
        /// ~4px per point @ 128x128 size (the contents of the <see cref="AccuracyHeatmap"/> are always square). 1089 total points.
        /// </summary>
        private const int points_per_dimension = 33;

        private const float rotation = 45;

        private BufferedContainer bufferedGrid = null!;
        private GridContainer pointGrid = null!;

        private readonly ScoreInfo score;
        private readonly IBeatmap playableBeatmap;

        private const float line_thickness = 2;

        /// <summary>
        /// The highest count of any point currently being displayed.
        /// </summary>
        protected float PeakValue { get; private set; }

        public AccuracyHeatmap(ScoreInfo score, IBeatmap playableBeatmap)
        {
            this.score = score;
            this.playableBeatmap = playableBeatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float line_extension = 0.2f;

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new CircularContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(inner_portion),
                                Masking = true,
                                BorderThickness = line_thickness,
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
                                Padding = new MarginPadding(1),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Rotation = rotation,
                                Child = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Circle
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Y,
                                            Width = line_thickness,
                                            Height = inner_portion + line_extension,
                                            Rotation = -rotation * 2,
                                            Alpha = 0.6f,
                                        },
                                        new Circle
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Y,
                                            Width = line_thickness,
                                            Height = inner_portion + line_extension,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "Overshoot",
                                            Font = OsuFont.GetFont(size: 12),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.BottomLeft,
                                            Padding = new MarginPadding(2),
                                            Rotation = -rotation,
                                            RelativePositionAxes = Axes.Both,
                                            Y = -(inner_portion + line_extension) / 2,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "Undershoot",
                                            Font = OsuFont.GetFont(size: 12),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.TopRight,
                                            Rotation = -rotation,
                                            Padding = new MarginPadding(2),
                                            RelativePositionAxes = Axes.Both,
                                            Y = (inner_portion + line_extension) / 2,
                                        },
                                        new Circle
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.TopCentre,
                                            RelativePositionAxes = Axes.Both,
                                            Y = -(inner_portion + line_extension) / 2,
                                            Margin = new MarginPadding(-line_thickness / 2),
                                            Width = line_thickness,
                                            Height = 10,
                                            Rotation = 45,
                                        },
                                        new Circle
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.TopCentre,
                                            RelativePositionAxes = Axes.Both,
                                            Y = -(inner_portion + line_extension) / 2,
                                            Margin = new MarginPadding(-line_thickness / 2),
                                            Width = line_thickness,
                                            Height = 10,
                                            Rotation = -45,
                                        }
                                    }
                                },
                            },
                        }
                    },
                    bufferedGrid = new BufferedContainer(cachedFrameBuffer: true)
                    {
                        RelativeSizeAxes = Axes.Both,
                        BackgroundColour = Color4Extensions.FromHex("#202624").Opacity(0),
                        Child = pointGrid = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
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
                    bool isHit = Vector2.Distance(new Vector2(c + 0.5f, r + 0.5f), centre) <= innerRadius;

                    if (isHit)
                    {
                        points[r][c] = new HitPoint(this)
                        {
                            BaseColour = new Color4(102, 255, 204, 255)
                        };
                    }
                    else
                    {
                        points[r][c] = new MissPoint
                        {
                            BaseColour = new Color4(255, 102, 102, 255)
                        };
                    }
                }
            }

            pointGrid.Content = points;

            if (score.HitEvents.Count == 0)
                return;

            float radius = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(playableBeatmap.Difficulty.CircleSize, true);

            foreach (var e in score.HitEvents.Where(e => e.HitObject is HitCircle && !(e.HitObject is SliderTailCircle)))
            {
                if (e.LastHitObject == null || e.Position == null)
                    continue;

                AddPoint(((OsuHitObject)e.LastHitObject).StackedEndPosition, ((OsuHitObject)e.HitObject).StackedEndPosition, e.Position.Value, radius);
            }
        }

        protected void AddPoint(Vector2 start, Vector2 end, Vector2 hitPoint, float radius)
        {
            if (pointGrid.Content.Count == 0)
                return;

            Vector2 relativePosition = FindRelativeHitPosition(start, end, hitPoint, radius, rotation);

            var localCentre = new Vector2(points_per_dimension - 1) / 2;
            float localRadius = localCentre.X * inner_portion;
            var localPoint = localCentre + localRadius * relativePosition;

            // Find the most relevant hit point.
            int r = (int)Math.Round(localPoint.Y);
            int c = (int)Math.Round(localPoint.X);

            if (r < 0 || r >= points_per_dimension || c < 0 || c >= points_per_dimension)
                return;

            PeakValue = Math.Max(PeakValue, ((GridPoint)pointGrid.Content[r][c]).Increment());

            bufferedGrid.ForceRedraw();
        }

        /// <summary>
        /// Normalises the position of a hit on a circle such that it is relative to the movement that was performed to arrive at said circle.
        /// </summary>
        /// <param name="previousObjectPosition">The position of the object prior to the one getting hit.</param>
        /// <param name="nextObjectPosition">The position of the object which is getting hit.</param>
        /// <param name="hitPoint">The point at which the user hit.</param>
        /// <param name="objectRadius">The radius of <paramref name="previousObjectPosition"/> and <paramref name="nextObjectPosition"/>.</param>
        /// <param name="rotation">
        /// The rotation of the axis which is to be considered in the same direction as the vector
        /// leading from <paramref name="previousObjectPosition"/> to <paramref name="nextObjectPosition"/>.
        /// </param>
        /// <returns>
        /// A 2D vector representing the <paramref name="hitPoint"/> as relative to the movement between <paramref name="previousObjectPosition"/> and <paramref name="nextObjectPosition"/>
        /// and relative to the <paramref name="objectRadius"/>.
        /// If the object was hit perfectly in the middle, the return value will be <see cref="Vector2.Zero"/>.
        /// If the object was hit perfectly at its edge, the returned vector will have a magnitude of 1.
        /// </returns>
        public static Vector2 FindRelativeHitPosition(Vector2 previousObjectPosition, Vector2 nextObjectPosition, Vector2 hitPoint, float objectRadius, float rotation)
        {
            double angle1 = Math.Atan2(nextObjectPosition.Y - hitPoint.Y, hitPoint.X - nextObjectPosition.X); // Angle between the end point and the hit point.
            double angle2 = Math.Atan2(nextObjectPosition.Y - previousObjectPosition.Y, previousObjectPosition.X - nextObjectPosition.X); // Angle between the end point and the start point.
            double finalAngle = angle2 - angle1; // Angle between start, end, and hit points.
            float normalisedDistance = Vector2.Distance(hitPoint, nextObjectPosition) / objectRadius; // Distance between the hit point and the end point.

            // Consider two objects placed horizontally, with the start on the left and the end on the right.
            // The above calculated the angle between {end, start}, and the angle between {end, hitPoint}, in the form:
            //             +pi | 0
            //     O --------- O ----->      Note: Math.Atan2 has a range (-pi <= theta <= +pi)
            //             -pi | 0
            // E.g. If the hit point was directly above end, it would have an angle pi/2.
            //
            // It also calculated the angle separating hitPoint from the line joining {start, end}, that is anti-clockwise in the form:
            //               0 | pi
            //     O --------- O ----->
            //             2pi | pi
            //
            // However keep in mind that cos(0)=1 and cos(2pi)=1, whereas we actually want these values to appear on the left, so the x-coordinate needs to be inverted.
            // Likewise sin(pi/2)=1 and sin(3pi/2)=-1, whereas we actually want these values to appear on the bottom/top respectively, so the y-coordinate also needs to be inverted.
            //
            // We also need to apply the anti-clockwise rotation.
            double rotatedAngle = finalAngle - float.DegreesToRadians(rotation);
            return -normalisedDistance * new Vector2((float)Math.Cos(rotatedAngle), (float)Math.Sin(rotatedAngle));
        }

        private abstract partial class GridPoint : CompositeDrawable
        {
            /// <summary>
            /// The base colour which will be lightened/darkened depending on the value of this <see cref="HitPoint"/>.
            /// </summary>
            public Color4 BaseColour;

            public override bool IsPresent => Count > 0;

            protected int Count { get; private set; }

            /// <summary>
            /// Increment the value of this point by one.
            /// </summary>
            /// <returns>The value after incrementing.</returns>
            public int Increment()
            {
                return ++Count;
            }
        }

        private partial class MissPoint : GridPoint
        {
            public MissPoint()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.Times
                };
            }

            protected override void Update()
            {
                Alpha = 0.8f;
                Colour = BaseColour;
            }
        }

        private partial class HitPoint : GridPoint
        {
            private readonly AccuracyHeatmap heatmap;

            public HitPoint(AccuracyHeatmap heatmap)
            {
                this.heatmap = heatmap;

                RelativeSizeAxes = Axes.Both;

                InternalChild = new Circle { RelativeSizeAxes = Axes.Both };
            }

            protected override void Update()
            {
                base.Update();

                // the point at which alpha is saturated and we begin to adjust colour lightness.
                const float lighten_cutoff = 0.95f;

                // the amount of lightness to attribute regardless of relative value to peak point.
                const float non_relative_portion = 0.2f;

                float amount = 0;

                // give some amount of alpha regardless of relative count
                amount += non_relative_portion * Math.Min(1, Count / 10f);

                // add relative portion
                amount += (1 - non_relative_portion) * (Count / heatmap.PeakValue);

                // apply easing
                amount = (float)Interpolation.ApplyEasing(Easing.OutQuint, Math.Min(1, amount));

                Debug.Assert(amount <= 1);

                Alpha = Math.Min(amount / lighten_cutoff, 1);
                Colour = BaseColour.Lighten(Math.Max(0, amount - lighten_cutoff));
            }
        }
    }
}
