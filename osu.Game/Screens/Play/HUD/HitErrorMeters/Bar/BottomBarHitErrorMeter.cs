// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters.Bar
{
    public class BottomBarHitErrorMeter : BarHitErrorMeter
    {
        private const int arrow_move_duration = 400;

        private const int judgement_line_height = 6;

        private const int bar_width = 200;

        private const int hit_bar_height = 2;

        private const int spacing = 2;

        private const float chevron_size = 8;

        private double floatingAverage;

        public BottomBarHitErrorMeter(HitWindows hitWindows)
            : base(hitWindows)
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            InternalChild = new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = bar_width,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, spacing),
                        Children = new Drawable[]
                        {
                            JudgmentsContainer = new Container
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Height = judgement_line_height,
                                RelativeSizeAxes = Axes.X,
                            },
                            ColourBars = new Container
                            {
                                Height = hit_bar_height,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Children = new Drawable[]
                                {
                                    ColourBarsEarly = new Container
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        RelativePositionAxes = Axes.X,
                                        X = 0.5f,
                                        Width = 0.5f,
                                        Scale = new Vector2(-1, 1),
                                    },
                                    ColourBarsLate = new Container
                                    {
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomRight,
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 0.5f,
                                    },
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Height = chevron_size,
                                Child = Arrow = new SpriteIcon
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativePositionAxes = Axes.X,
                                    Icon = FontAwesome.Solid.ChevronDown,
                                    Size = new Vector2(chevron_size)
                                }
                            }
                        }
                    },
                    new SpriteIcon
                    {
                        X = -10,
                        Size = new Vector2(10),
                        Icon = FontAwesome.Solid.ShippingFast,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    new SpriteIcon
                    {
                        X = 10,
                        Size = new Vector2(10),
                        Icon = FontAwesome.Solid.Bicycle,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                    }
                }
            };

            CreateColourBars();

            PerfectHit.Anchor = Anchor.BottomCentre;
            PerfectHit.RelativePositionAxes = Axes.X;
            PerfectHit.Height = 2.5f;
            PerfectHit.Width = 0.01f;

            ColourBars.Add(PerfectHit);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild.Width = 0;
            InternalChild.ResizeWidthTo(bar_width, 800, Easing.OutQuint);
        }

        protected override Drawable CreateColourBar(Color4 color, (HitResult, double) window, bool isFirst)
        {
            var length = (float)(window.Item2 / MaxHitWindow);

            if (isFirst)
            {
                const float gradient_start = 0.8f;

                return new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = color,
                            Width = length * gradient_start
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.X,
                            Colour = ColourInfo.GradientHorizontal(color, color.Opacity(0)),
                            X = gradient_start,
                            Width = length * (1 - gradient_start)
                        }
                    }
                };
            }

            return new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = color,
                Width = length
            };
        }

        private float calculatePositionForJudgement(double value) => (float)(value / MaxHitWindow) / 2;

        protected override JudgementLine CreateJudgement(JudgementResult result) => new JudgementLine(true)
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Y,
            Width = 3,
            RelativePositionAxes = Axes.X,
            X = calculatePositionForJudgement(result.TimeOffset)
        };

        protected override void MoveArrow(JudgementResult res)
        {
            Arrow.MoveToX(calculatePositionForJudgement(floatingAverage = floatingAverage * 0.9 + res.TimeOffset * 0.1),
                arrow_move_duration, Easing.Out);
        }
    }
}
