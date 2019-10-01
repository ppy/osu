// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public class BarHitErrorMeter : HitErrorMeter
    {
        private readonly Anchor alignment;

        private const int arrow_move_duration = 400;

        private const int judgement_line_width = 6;

        private const int bar_height = 200;

        private const int bar_width = 2;

        private const int spacing = 2;

        private const float chevron_size = 8;

        private SpriteIcon arrow;

        private Container colourBarsEarly;
        private Container colourBarsLate;

        private Container judgementsContainer;
        private Container arrowContainer;

        private double maxHitWindow;

        public BarHitErrorMeter(HitWindows hitWindows, Anchor alignment)
            : base(hitWindows)
        {
            this.alignment = alignment;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = alignment == Anchor.x1 ? Axes.Y : Axes.X,
                Direction = alignment == Anchor.x1 ? FillDirection.Vertical : FillDirection.Horizontal,
                Spacing = alignment == Anchor.x1 ? new Vector2(0, spacing) : new Vector2(spacing, 0),
                Margin = new MarginPadding(2),
                Children = new Drawable[]
                {
                    judgementsContainer = new Container
                    {
                        Anchor = Anchor.y1 | alignment,
                        Origin = Anchor.y1 | alignment,
                        RelativeSizeAxes = alignment == Anchor.x1 ? Axes.X : Axes.Y,
                    },
                    colourBars = new Container
                    {
                        RelativeSizeAxes = alignment == Anchor.x1 ? Axes.X : Axes.Y,
                        Anchor = Anchor.y1 | alignment,
                        Origin = Anchor.y1 | alignment,
                        Children = new Drawable[]
                        {
                            colourBarsEarly = new Container
                            {
                                Anchor = alignment == Anchor.x1 ? Anchor.CentreLeft : Anchor.y1 | alignment,
                                Origin = alignment == Anchor.x1 ? Anchor.CentreRight : alignment,
                                RelativeSizeAxes = Axes.Both,
                            },
                            colourBarsLate = new Container
                            {
                                Anchor = alignment == Anchor.x1 ? Anchor.CentreRight : Anchor.y1 | alignment,
                                Origin = alignment == Anchor.x1 ? Anchor.CentreRight : alignment,
                                RelativeSizeAxes = Axes.Both,
                            },
                            new SpriteIcon
                            {
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.ShippingFast,
                                Anchor = alignment == Anchor.x1 ? Anchor.CentreLeft : Anchor.TopCentre,
                                Origin = alignment == Anchor.x1 ? Anchor.CentreLeft : Anchor.TopCentre,
                            },
                            new SpriteIcon
                            {
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.Bicycle,
                                Anchor = alignment == Anchor.x1 ? Anchor.CentreRight : Anchor.BottomCentre,
                                Origin = alignment == Anchor.x1 ? Anchor.CentreRight : Anchor.BottomCentre,
                            }
                        }
                    },
                    arrowContainer = new Container
                    {
                        Anchor = Anchor.y1 | alignment,
                        Origin = Anchor.y1 | alignment,
                        RelativeSizeAxes = alignment == Anchor.x1 ? Axes.X : Axes.Y,
                        Child = arrow = new SpriteIcon
                        {
                            Anchor = alignment == Anchor.x1 ? Anchor.BottomCentre : Anchor.TopCentre,
                            Origin = alignment == Anchor.x1 ? Anchor.BottomCentre : Anchor.Centre,
                            RelativePositionAxes = alignment == Anchor.x1 ? Axes.X : Axes.Y,
                            Icon = alignment == Anchor.x1 ? FontAwesome.Solid.ChevronUp : (alignment == Anchor.x2 ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft),
                            Size = new Vector2(chevron_size),
                        }
                    },
                }
            };

            if (alignment == Anchor.x1)
            {
                InternalChild.Width = bar_height;
                judgementsContainer.Height = judgement_line_width;
                colourBars.Height = 2;
                colourBarsEarly.Scale = new Vector2(-1, 1);
                colourBarsEarly.Width = 0.5f;
                colourBarsLate.Width = 0.5f;
                arrowContainer.Height = chevron_size;
            }
            else
            {
                InternalChild.Height = bar_height;
                judgementsContainer.Width = judgement_line_width;
                colourBars.Width = bar_width;
                colourBarsEarly.Scale = new Vector2(1, -1);
                colourBarsEarly.Height = 0.5f;
                colourBarsLate.Height = 0.5f;
                arrowContainer.Width = chevron_size;
                arrow.Y = 0.5f;
            }

            createColourBars(colours);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (alignment == Anchor.x1)
            {
                colourBars.Width = 0;
                colourBars.ResizeWidthTo(1, 800, Easing.OutQuint);
            }
            else
            {
                colourBars.Height = 0;
                colourBars.ResizeHeightTo(1, 800, Easing.OutQuint);
            }

            arrow.Alpha = 0;
            arrow.Delay(200).FadeInFromZero(600);
        }

        private void createColourBars(OsuColour colours)
        {
            var windows = HitWindows.GetAllAvailableWindows().ToArray();

            maxHitWindow = windows.First().length;

            for (var i = 0; i < windows.Length; i++)
            {
                var (result, length) = windows[i];

                colourBarsEarly.Add(createColourBar(result, (float)(length / maxHitWindow), i == 0));
                colourBarsLate.Add(createColourBar(result, (float)(length / maxHitWindow), i == 0));
            }

            // a little nub to mark the centre point.
            var centre = createColourBar(windows.Last().result, 0.01f);
            centre.Anchor = centre.Origin = alignment == Anchor.x1 ? Anchor.BottomCentre : Anchor.y1 | (alignment == Anchor.x2 ? Anchor.x0 : Anchor.x2);

            if (alignment == Anchor.x1)
                centre.Height = 2.5f;
            else
                centre.Width = 2.5f;

            colourBars.Add(centre);

            Color4 getColour(HitResult result)
            {
                switch (result)
                {
                    case HitResult.Meh:
                        return colours.Yellow;

                    case HitResult.Ok:
                        return colours.Green;

                    case HitResult.Good:
                        return colours.GreenLight;

                    case HitResult.Great:
                        return colours.Blue;

                    default:
                        return colours.BlueLight;
                }
            }

            Drawable createColourBar(HitResult result, float height, bool first = false)
            {
                var colour = getColour(result);
                Box box;

                if (first)
                {
                    // the first bar needs gradient rendering.
                    const float gradient_start = 0.8f;
                    Box gradientBox;

                    var colourBar = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            gradientBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = getColour(result),
                            },
                            box = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Colour = alignment == Anchor.x1 ? ColourInfo.GradientHorizontal(colour, colour.Opacity(0)) : ColourInfo.GradientVertical(colour, colour.Opacity(0)),
                            },
                        }
                    };

                    if (alignment == Anchor.x1)
                    {
                        box.X = gradient_start;
                        box.Width = height * (1 - gradient_start);
                        gradientBox.Width = height * gradient_start;
                    }
                    else
                    {
                        box.Y = gradient_start;
                        box.Height = height * (1 - gradient_start);
                        gradientBox.Height = height * gradient_start;
                    }

                    return colourBar;
                }

                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                };

                if (alignment == Anchor.x1)
                    box.Width = height;
                else
                    box.Height = height;

                return box;
            }
        }

        private double floatingAverage;
        private Container colourBars;

        public override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit)
                return;

            var judgementLine = new JudgementLine(alignment == Anchor.x1)
            {
                Anchor = alignment == Anchor.x1 ? Anchor.x1 : (alignment == Anchor.x2 ? Anchor.x0 : Anchor.x2),
                Origin = Anchor.y1 | (alignment == Anchor.x1 ? Anchor.x1 : (alignment == Anchor.x2 ? Anchor.x0 : Anchor.x2)),
            };

            if (alignment == Anchor.x1)
            {
                judgementLine.X = getRelativeJudgementPosition(judgement.TimeOffset);
                arrow.MoveToX(
                    getRelativeJudgementPosition(floatingAverage = floatingAverage * 0.9 + judgement.TimeOffset * 0.1),
                    arrow_move_duration, Easing.Out);
            }
            else
            {
                judgementLine.Y = getRelativeJudgementPosition(judgement.TimeOffset);
                arrow.MoveToY(
                    getRelativeJudgementPosition(floatingAverage = floatingAverage * 0.9 + judgement.TimeOffset * 0.1)
                    , arrow_move_duration, Easing.Out);
            }

            judgementsContainer.Add(judgementLine);
        }

        private float getRelativeJudgementPosition(double value) => alignment == Anchor.x1 ? (float)(value / maxHitWindow) / 2 : (float)((value / maxHitWindow) + 1) / 2;

        private class JudgementLine : CompositeDrawable
        {
            private const int judgement_fade_duration = 10000;

            private readonly bool isVertical;

            public JudgementLine(bool isVertical = false)
            {
                this.isVertical = isVertical;

                if (isVertical)
                {
                    RelativeSizeAxes = Axes.Y;
                    RelativePositionAxes = Axes.X;
                    Width = 3;
                }
                else
                {
                    RelativeSizeAxes = Axes.X;
                    RelativePositionAxes = Axes.Y;
                    Height = 3;
                }

                InternalChild = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (isVertical)
                {
                    Height = 0;
                    this.ResizeHeightTo(1, 200, Easing.OutElasticHalf);
                }
                else
                {
                    Width = 0;
                    this.ResizeWidthTo(1, 200, Easing.OutElasticHalf);
                }

                this.FadeTo(0.8f, 150).Then().FadeOut(judgement_fade_duration, Easing.OutQuint).Expire();
            }
        }
    }
}
