// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class RatingDistributionGraph : CompositeDrawable, IHasCustomTooltip<RatingDistributionGraph.RatingDistributionGraphTooltipData>
    {
        private const int y_divisions = 4;
        private const int x_divisions = 16;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Container yAxisLeftContainer = null!;
        private Container yAxisRightContainer = null!;
        private Container xAxisContainer = null!;
        private Container chartContainer = null!;

        private Container gridContainer = null!;
        private Container barsContainer = null!;
        private Container userRatingContainer = null!;
        private PointPath cumulativePath = null!;

        private Drawable hoverMarker = null!;
        private Drawable hoverMarkerFill = null!;
        private OsuTextFlowContainer descriptionText = null!;

        private (int x, int y)[] data = [];
        private int? userRating;
        private (int min, int max, int step) xRange;
        private (int min, int max) yRange;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 20 },
                RowDimensions =
                [
                    // Chart
                    new Dimension(),
                    // x-axis
                    new Dimension(GridSizeMode.AutoSize),
                    // "Rating"
                    new Dimension(GridSizeMode.AutoSize),
                    // Description text
                    new Dimension(GridSizeMode.AutoSize)
                ],
                Content = new[]
                {
                    new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColumnDimensions = new[]
                            {
                                // "Players"
                                new Dimension(GridSizeMode.AutoSize),
                                // Left y-axis
                                new Dimension(GridSizeMode.AutoSize),
                                // Chart
                                new Dimension(),
                                // Right y-axis
                                new Dimension(GridSizeMode.AutoSize),
                                // "Cumulative"
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.TopCentre,
                                        Text = "Players",
                                        Font = OsuFont.Default.With(size: 12),
                                        Rotation = -90,
                                        Colour = colourProvider.Foreground1
                                    },
                                    yAxisLeftContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Margin = new MarginPadding { Left = 5, Right = 5 },
                                    },
                                    chartContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new[]
                                        {
                                            gridContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.Both
                                            },
                                            barsContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Masking = true
                                            },
                                            userRatingContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.Both
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                // Margin and padding to better align the paths.
                                                Margin = new MarginPadding { Left = -2 },
                                                Padding = new MarginPadding { Right = -2 },
                                                Children = new Drawable[]
                                                {
                                                    new PointPath
                                                    {
                                                        AutoSizeAxes = Axes.None,
                                                        RelativeSizeAxes = Axes.Both,
                                                        PathRadius = 2,
                                                        Colour = colourProvider.Colour0,
                                                    },
                                                    cumulativePath = new PointPath
                                                    {
                                                        AutoSizeAxes = Axes.None,
                                                        RelativeSizeAxes = Axes.Both,
                                                        PathRadius = 2,
                                                        Colour = colours.Yellow,
                                                        Offset = new Vector2(0, -3)
                                                    },
                                                }
                                            },
                                            hoverMarker = new CircularContainer
                                            {
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(12),
                                                Masking = true,
                                                BorderThickness = 2,
                                                BorderColour = Color4.White,
                                                Alpha = 0,
                                                Child = hoverMarkerFill = new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both
                                                }
                                            }
                                        }
                                    },
                                    yAxisRightContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Margin = new MarginPadding { Left = 5 },
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.TopCentre,
                                        Text = "Cumulative",
                                        Font = OsuFont.Default.With(size: 12),
                                        Rotation = 90,
                                        Colour = colourProvider.Foreground1
                                    },
                                }
                            }
                        }
                    },
                    new Drawable[]
                    {
                        xAxisContainer = new Container
                        {
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Top = 8 }
                        }
                    },
                    new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding { Top = 2 },
                            Text = "Rating",
                            Font = OsuFont.Default.With(size: 12),
                            Colour = colourProvider.Foreground1
                        },
                    },
                    new Drawable[]
                    {
                        descriptionText = new OsuTextFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                            Height = 16,
                            Margin = new MarginPadding { Top = 4 }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateGraph();
        }

        public void SetData((int x, int y)[] data, int? userRating)
        {
            this.data = data;
            this.userRating = userRating;

            xRange = (
                data.Select(d => d.x).DefaultIfEmpty().Min(),
                data.Select(d => d.x).DefaultIfEmpty().Max(),
                data.Zip(data.Skip(1), (a, b) => Math.Abs(b.x - a.x)).DefaultIfEmpty().Min()
            );

            if (userRating < xRange.min)
            {
                this.data = this.data.Prepend((userRating.Value, 1)).ToArray();
                xRange.min = userRating.Value;
            }

            if (userRating > xRange.max)
            {
                this.data = this.data.Append((userRating.Value, 1)).ToArray();
                xRange.max = userRating.Value;
            }

            yRange = (
                0,
                (int)roundToSignificant(this.data.Select(d => d.y).DefaultIfEmpty().Max())
            );

            updateGraph();
        }

        protected override void Update()
        {
            base.Update();

            xAxisContainer.X = xAxisContainer.Parent!.ToLocalSpace(chartContainer.ScreenSpaceDrawQuad.TopLeft).X;
            xAxisContainer.Width = chartContainer.DrawWidth;
        }

        private void updateGraph() => Scheduler.AddOnce(() =>
        {
            xAxisContainer.Clear();
            yAxisLeftContainer.Clear();
            yAxisRightContainer.Clear();
            gridContainer.Clear();
            barsContainer.Clear();
            userRatingContainer.Clear();

            for (int step = 0; step <= x_divisions; step++)
            {
                gridContainer.Add(new VerticalLine
                {
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    X = (float)step / x_divisions,
                    Colour = colourProvider.Background1
                });

                xAxisContainer.Add(new OsuSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.CentreRight,
                    RelativePositionAxes = Axes.X,
                    X = (float)step / x_divisions,
                    Margin = new MarginPadding { Right = -2 },
                    Rotation = -40,
                    Text = (xRange.min + (xRange.max - xRange.min) / x_divisions * step).ToString(),
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = colourProvider.Foreground1
                });
            }

            for (int step = 0; step <= y_divisions; step++)
            {
                gridContainer.Add(new HorizontalLine
                {
                    RelativeSizeAxes = Axes.X,
                    RelativePositionAxes = Axes.Y,
                    Y = (float)step / y_divisions,
                    Colour = colourProvider.Background1
                });

                yAxisLeftContainer.Add(new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.CentreRight,
                    RelativePositionAxes = Axes.Y,
                    Y = 1f - (float)step / y_divisions,
                    Text = (yRange.min + (yRange.max - yRange.min) / y_divisions * step).ToString(),
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = colourProvider.Foreground1
                });

                yAxisRightContainer.Add(new OsuSpriteText
                {
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Y,
                    Y = 1f - (float)step / y_divisions,
                    Text = $"{(float)step / y_divisions:P1}",
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = colourProvider.Foreground1
                });
            }

            foreach (var point in data)
            {
                barsContainer.Add(new Container
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.X,
                    RelativeSizeAxes = Axes.Both,
                    X = pointOnGraph(point.x, point.y).X,
                    Height = 1 - pointOnGraph(point.x, point.y).Y,
                    Width = pointOnGraph(xRange.min + xRange.step, 0).X,
                    Colour = colourProvider.Colour0,
                    Masking = true,
                    CornerRadius = 2,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                });
            }

            if (userRating != null)
            {
                userRatingContainer.Add(new UserRatingLine(userRating.Value)
                {
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    X = pointOnGraph(userRating.Value, 0).X,
                    Colour = colours.Green
                });
            }

            if (data.Length == 0)
                descriptionText.Text = "No games have been played yet.";
            else if (userRating == null)
                descriptionText.Text = "Play more games to get rated!";
            else
            {
                int countPlayersBelow = data.Where(d => d.x < userRating).Sum(d => d.y);
                int countPlayersAbove = data.Where(d => d.x >= userRating).Sum(d => d.y);
                float p = (float)countPlayersBelow / (countPlayersBelow + countPlayersAbove);

                descriptionText.Clear();
                descriptionText.AddText("You are better than ");
                descriptionText.AddText($"{p:P1}", s =>
                {
                    s.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);
                    s.Colour = colours.Green;
                });
                descriptionText.AddText(" of players.");
            }

            int currentCount = 0;
            int totalCount = data.Sum(d => d.y);

            cumulativePath.Points = data.Select(d =>
            {
                currentCount += d.y;
                float p = (float)currentCount / totalCount;
                return new Vector2(pointOnGraph(d.x, 0).X, 1 - p);
            }).ToArray();
        });

        private Vector2 pointOnGraph(int x, int y)
        {
            float xPos = ((float)x - xRange.min) / (xRange.max - xRange.min);
            float yPos = 1 - ((float)y - yRange.min) / (yRange.max - yRange.min);
            return new Vector2(xPos, yPos);
        }

        private static double roundToSignificant(double value)
        {
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(value)));
            return Math.Ceiling(value / scale) * scale;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            return chartContainer.DrawRectangle.Inflate(20).Contains(chartContainer.ToLocalSpace(screenSpacePos));
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverMarker.FadeTo(1f, 200);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverMarker.FadeTo(0f, 200);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            float minDistToCursor = float.MaxValue;
            Vector2 closestPointToCursor = Vector2.Zero;
            Color4 closestColourToCursor = Color4.White;
            int closestRatingToCursor = 0;
            string closestValueToCursor = string.Empty;

            if (userRating != null)
            {
                Vector2 userRatingPos1 = userRatingContainer.ToScreenSpace(pointOnGraph(userRating.Value, 0) * userRatingContainer.DrawSize);
                Vector2 userRatingPos2 = userRatingContainer.ToScreenSpace(pointOnGraph(userRating.Value, yRange.max) * userRatingContainer.DrawSize);

                minDistToCursor = Vector2.Distance(e.ScreenSpaceMousePosition, userRatingPos1);
                closestPointToCursor = userRatingPos1;
                closestColourToCursor = colours.Green;
                closestRatingToCursor = userRating.Value;
                closestValueToCursor = $"Your rating ({userRating})";

                float d = Vector2.Distance(e.ScreenSpaceMousePosition, userRatingPos2);

                if (d < minDistToCursor)
                {
                    minDistToCursor = d;
                    closestPointToCursor = userRatingPos2;
                }
            }

            for (int i = 0; i < data.Length; i++)
            {
                Vector2 pos = barsContainer.ToScreenSpace(pointOnGraph(data[i].x, data[i].y) * barsContainer.DrawSize);
                float d = Vector2.Distance(e.ScreenSpaceMousePosition, pos);

                if (d < minDistToCursor)
                {
                    minDistToCursor = d;
                    closestPointToCursor = pos;
                    closestColourToCursor = colourProvider.Colour0;
                    closestRatingToCursor = data[i].x;
                    closestValueToCursor = $"Players: {data[i].y}";
                }
            }

            int currentCount = 0;
            int totalCount = data.Sum(p => p.y);

            for (int i = 0; i < cumulativePath.Vertices.Count; i++)
            {
                currentCount += data[i].y;

                Vector2 pos = cumulativePath.ToScreenSpace(cumulativePath.Vertices[i] + new Vector2(2));
                float d = Vector2.Distance(e.ScreenSpaceMousePosition, pos);

                if (d < minDistToCursor)
                {
                    minDistToCursor = d;
                    closestPointToCursor = pos;
                    closestColourToCursor = colours.Yellow;
                    closestRatingToCursor = data[i].x;
                    closestValueToCursor = $"Cumulative: {(float)currentCount / totalCount:P1}";
                }
            }

            if (minDistToCursor == float.MaxValue)
                TooltipContent = new RatingDistributionGraphTooltipData();
            else
            {
                TooltipContent = new RatingDistributionGraphTooltipData
                {
                    Colour = closestColourToCursor,
                    Position = closestPointToCursor,
                    Rating = closestRatingToCursor,
                    Value = closestValueToCursor,
                };
            }

            hoverMarker.Position = gridContainer.ToLocalSpace(TooltipContent.Position);
            hoverMarkerFill.Colour = TooltipContent.Colour;

            return true;
        }

        public ITooltip<RatingDistributionGraphTooltipData> GetCustomTooltip() => new RatingDistributionGraphTooltip();

        public RatingDistributionGraphTooltipData TooltipContent { get; private set; } = new RatingDistributionGraphTooltipData();

        /// <summary>
        /// A simple vertical line that always remains 1px in size.
        /// </summary>
        private partial class VerticalLine : Box
        {
            protected override void Update()
            {
                base.Update();
                Width = Parent!.DrawWidth / Parent.ScreenSpaceDrawQuad.Width;
            }
        }

        /// <summary>
        /// A simple horizontal line that always remains 1px in size.
        /// </summary>
        private partial class HorizontalLine : Box
        {
            protected override void Update()
            {
                base.Update();
                Height = Parent!.DrawHeight / Parent.ScreenSpaceDrawQuad.Height;
            }
        }

        private partial class UserRatingLine : CompositeDrawable
        {
            public UserRatingLine(int rating)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 2,
                    },
                    new Circle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(8),
                    },
                    new Circle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(8),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                        Y = -4,
                        Text = $"Your rating ({rating})",
                        Font = OsuFont.Torus.With(size: 12),
                    }
                };
            }
        }

        private partial class PointPath : SmoothPath
        {
            private Vector2[] points = [];

            public Vector2[] Points
            {
                get => points;
                set
                {
                    points = value;
                    verticesCache.Invalidate();
                }
            }

            private Vector2 offset;

            public Vector2 Offset
            {
                get => offset;
                set
                {
                    offset = value;
                    verticesCache.Invalidate();
                }
            }

            private readonly LayoutValue verticesCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

            public PointPath()
            {
                AddLayout(verticesCache);
            }

            protected override void Update()
            {
                base.Update();

                if (!verticesCache.IsValid)
                {
                    updateVertices();
                    verticesCache.Validate();
                }
            }

            private void updateVertices()
            {
                ClearVertices();

                for (int i = 0; i < Points.Length; i++)
                    AddVertex(Points[i] * (Parent!.DrawSize + offset));
            }
        }

        private partial class RatingDistributionGraphTooltip : VisibilityContainer, ITooltip<RatingDistributionGraphTooltipData>
        {
            private readonly OsuSpriteText ratingText;
            private readonly Drawable valueColour;
            private readonly OsuSpriteText valueText;

            private RatingDistributionGraphTooltipData content = new RatingDistributionGraphTooltipData();
            private bool instantMove = true;

            public RatingDistributionGraphTooltip()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 3,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.7f
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding(8),
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                ratingText = new OsuSpriteText
                                {
                                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold)
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(3),
                                    Children = new[]
                                    {
                                        valueColour = new Box
                                        {
                                            Size = new Vector2(12)
                                        },
                                        valueText = new OsuSpriteText
                                        {
                                            Font = OsuFont.Torus.With(size: 12)
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            public void SetContent(RatingDistributionGraphTooltipData content)
            {
                this.content = content;

                ratingText.Text = content.Rating.ToString();
                valueColour.Colour = content.Colour;
                valueText.Text = content.Value;
            }

            public void Move(Vector2 pos)
            {
                pos = Parent!.ToLocalSpace(content.Position) - new Vector2(DrawWidth + 10, 0);

                if (instantMove)
                {
                    Position = pos;
                    instantMove = false;
                }
                else
                    this.MoveTo(pos, 200, Easing.OutQuint);
            }

            protected override void PopIn()
            {
                instantMove |= !IsPresent;
                this.FadeIn(200, Easing.OutQuint);
            }

            protected override void PopOut()
            {
                this.FadeOut(200, Easing.OutQuint);
            }
        }

        public class RatingDistributionGraphTooltipData
        {
            public Color4 Colour;
            public Vector2 Position;

            public int Rating;
            public string Value = string.Empty;
        }
    }
}
