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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters.Bar
{
    public class BottomBarHitErrorMeter : HitErrorMeter
    {
        private const int arrow_move_duration = 400;

        private const int judgement_line_height = 6;

        private const int bar_width = 200;

        private const int hit_bar_height = 2;

        private const int spacing = 2;

        private const float chevron_size = 8;

        private SpriteIcon arrow;

        private Container colourBarsEarly;
        private Container colourBarsLate;

        private Container colourBars;

        private Container judgementsContainer;

        private double maxHitWindow;
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
                            judgementsContainer = new Container
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Height = judgement_line_height,
                                RelativeSizeAxes = Axes.X,
                            },
                            colourBars = new Container
                            {
                                Height = hit_bar_height,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.X,
                                Children = new Drawable[]
                                {
                                    colourBarsEarly = new Container
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        RelativePositionAxes = Axes.X,
                                        X = 0.5f,
                                        Width = 0.5f,
                                        Scale = new Vector2(-1, 1),
                                    },
                                    colourBarsLate = new Container
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
                                Child = arrow = new SpriteIcon
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
            createColourBars(colors);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild.Width = 0;
            InternalChild.ResizeWidthTo(bar_width, 800, Easing.OutQuint);

            arrow.Alpha = 0;
            arrow.Delay(200).FadeInFromZero(600);
        }

        private void createColourBars(OsuColour colours)
        {
            var windows = HitWindows.GetAllAvailableWindows().ToArray();

            maxHitWindow = windows.First().length;

            for (int i = 0; i < windows.Length; i++)
            {
                var (result, length) = windows[i];

                colourBarsEarly.Add(createColourBar(result, (float)(length / maxHitWindow), i == 0));
                colourBarsLate.Add(createColourBar(result, (float)(length / maxHitWindow), i == 0));
            }

            var centre = createColourBar(windows.Last().result, 0.01f);
            centre.Anchor = centre.Origin = Anchor.BottomCentre;
            centre.RelativePositionAxes = Axes.X;
            centre.Height = 2.5f;
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

            Drawable createColourBar(HitResult result, float width, bool first = false)
            {
                var colour = getColour(result);

                if (first)
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
                                Colour = getColour(result),
                                Width = width * gradient_start
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                Colour = ColourInfo.GradientHorizontal(colour, colour.Opacity(0)),
                                X = gradient_start,
                                Width = width * (1 - gradient_start)
                            },
                        }
                    };
                }

                return new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                    Width = width
                };
            }
        }

        public override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit)
                return;

            judgementsContainer.Add(new JudgementLine
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                X = calculatePositionForJudgement(judgement.TimeOffset),
            });

            arrow.MoveToX(calculatePositionForJudgement(floatingAverage = floatingAverage * 0.9 + judgement.TimeOffset * 0.1),
                arrow_move_duration, Easing.Out);
        }

        private float calculatePositionForJudgement(double value) => (float)(value / maxHitWindow) / 2;

        private class JudgementLine : CompositeDrawable
        {
            private const int judgement_fade_duration = 10000;

            public JudgementLine()
            {
                RelativeSizeAxes = Axes.Y;
                RelativePositionAxes = Axes.X;
                Width = 3;

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

                Height = 0;

                this.ResizeHeightTo(1, 200, Easing.OutElasticHalf);
                this.FadeTo(0.8f, 150).Then().FadeOut(judgement_fade_duration, Easing.OutQuint).Expire();
            }
        }
    }
}
