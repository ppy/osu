// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters.Bar
{
    public class SidebarHitErrorMeter : BarHitErrorMeter
    {
        private readonly Anchor alignment;

        private const int arrow_move_duration = 400;

        private const int judgement_line_width = 6;

        private const int bar_height = 200;

        private const int bar_width = 2;

        private const int spacing = 2;

        private const float chevron_size = 8;

        public SidebarHitErrorMeter(HitWindows hitWindows, bool rightAligned = false)
            : base(hitWindows)
        {
            alignment = rightAligned ? Anchor.x0 : Anchor.x2;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                Height = bar_height,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(spacing, 0),
                Margin = new MarginPadding(2),
                Children = new Drawable[]
                {
                    JudgmentsContainer = new Container
                    {
                        Anchor = Anchor.y1 | alignment,
                        Origin = Anchor.y1 | alignment,
                        Width = judgement_line_width,
                        RelativeSizeAxes = Axes.Y,
                    },
                    ColourBars = new Container
                    {
                        Width = bar_width,
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.y1 | alignment,
                        Origin = Anchor.y1 | alignment,
                        Children = new Drawable[]
                        {
                            ColourBarsEarly = new Container
                            {
                                Anchor = Anchor.y1 | alignment,
                                Origin = alignment,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                Scale = new Vector2(1, -1),
                            },
                            ColourBarsLate = new Container
                            {
                                Anchor = Anchor.y1 | alignment,
                                Origin = alignment,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                            },
                            new SpriteIcon
                            {
                                Y = -10,
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.ShippingFast,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new SpriteIcon
                            {
                                Y = 10,
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.Bicycle,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.y1 | alignment,
                        Origin = Anchor.y1 | alignment,
                        Width = chevron_size,
                        RelativeSizeAxes = Axes.Y,
                        Child = Arrow = new SpriteIcon
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Y,
                            Y = 0.5f,
                            Icon = alignment == Anchor.x2 ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
                            Size = new Vector2(chevron_size),
                        }
                    },
                }
            };

            CreateColourBars();

            PerfectHit.Anchor = PerfectHit.Origin = Anchor.y1 | (alignment == Anchor.x2 ? Anchor.x0 : Anchor.x2);
            PerfectHit.Width = 2.5f;
            PerfectHit.Height = 0.01f;

            ColourBars.Add(PerfectHit);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ColourBars.Height = 0;
            ColourBars.ResizeHeightTo(1, 800, Easing.OutQuint);
        }

        private double floatingAverage;

        private float getRelativeJudgementPosition(double value) => (float)((value / MaxHitWindow) + 1) / 2;

        protected override Drawable CreateColourBar(Color4 color, (HitResult, double) window, bool isFirst)
        {
            var length = (float)(window.Item2 / MaxHitWindow);

            if (isFirst)
            {
                // the first bar needs gradient rendering.
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
                            Height = length * gradient_start
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(color, color.Opacity(0)),
                            Y = gradient_start,
                            Height = length * (1 - gradient_start)
                        },
                    }
                };
            }

            return new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = color,
                Height = length
            };
        }

        protected override JudgementLine CreateJudgement(JudgementResult result) => new JudgementLine(false)
        {
            Y = getRelativeJudgementPosition(result.TimeOffset),
            Anchor = alignment == Anchor.x2 ? Anchor.x0 : Anchor.x2,
            Origin = Anchor.y1 | (alignment == Anchor.x2 ? Anchor.x0 : Anchor.x2),
            RelativeSizeAxes = Axes.X,
            RelativePositionAxes = Axes.Y,
            Height = 3,
        };

        protected override void MoveArrow(JudgementResult res) => Arrow.MoveToY(
            getRelativeJudgementPosition(floatingAverage = floatingAverage * 0.9 + res.TimeOffset * 0.1)
            , arrow_move_duration, Easing.Out);
    }
}
