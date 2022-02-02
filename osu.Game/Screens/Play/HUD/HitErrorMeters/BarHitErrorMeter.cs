// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public class BarHitErrorMeter : HitErrorMeter
    {
        private const int arrow_move_duration = 400;

        private const int judgement_line_width = 6;

        private const int bar_height = 200;

        private const int bar_width = 2;

        private const int spacing = 2;

        private const float chevron_size = 8;

        private SpriteIcon arrow;
        private SpriteIcon iconEarly;
        private SpriteIcon iconLate;

        private Container colourBarsEarly;
        private Container colourBarsLate;

        private Container judgementsContainer;

        private double maxHitWindow;

        public BarHitErrorMeter()
        {
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
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = chevron_size,
                        RelativeSizeAxes = Axes.Y,
                        Child = arrow = new SpriteIcon
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Y,
                            Y = 0.5f,
                            Icon = FontAwesome.Solid.ChevronRight,
                            Size = new Vector2(chevron_size),
                        }
                    },
                    colourBars = new Container
                    {
                        Width = bar_width,
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Children = new Drawable[]
                        {
                            colourBarsEarly = new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                Scale = new Vector2(1, -1),
                            },
                            colourBarsLate = new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                            },
                            iconEarly = new SpriteIcon
                            {
                                Y = -10,
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.ShippingFast,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                            },
                            iconLate = new SpriteIcon
                            {
                                Y = 10,
                                Size = new Vector2(10),
                                Icon = FontAwesome.Solid.Bicycle,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                            }
                        }
                    },
                    judgementsContainer = new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = judgement_line_width,
                        RelativeSizeAxes = Axes.Y,
                    },
                }
            };

            createColourBars();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            colourBars.Height = 0;
            colourBars.ResizeHeightTo(1, 800, Easing.OutQuint);

            arrow.Alpha = 0;
            arrow.Delay(200).FadeInFromZero(600);
        }

        protected override void Update()
        {
            base.Update();

            // undo any layout rotation to display icons in the correct orientation
            iconEarly.Rotation = -Rotation;
            iconLate.Rotation = -Rotation;
        }

        private void createColourBars()
        {
            var windows = HitWindows.GetAllAvailableWindows().ToArray();

            // max to avoid div-by-zero.
            maxHitWindow = Math.Max(1, windows.First().length);

            for (int i = 0; i < windows.Length; i++)
            {
                (var result, double length) = windows[i];

                float hitWindow = (float)(length / maxHitWindow);

                colourBarsEarly.Add(createColourBar(result, hitWindow, i == 0));
                colourBarsLate.Add(createColourBar(result, hitWindow, i == 0));
            }

            // a little nub to mark the centre point.
            var centre = createColourBar(windows.Last().result, 0.01f);
            centre.Anchor = centre.Origin = Anchor.CentreLeft;
            centre.Width = 2.5f;
            colourBars.Add(centre);

            Drawable createColourBar(HitResult result, float height, bool first = false)
            {
                var colour = GetColourForHitResult(result);

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
                                Colour = colour,
                                Height = height * gradient_start
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(colour, colour.Opacity(0)),
                                Y = gradient_start,
                                Height = height * (1 - gradient_start)
                            },
                        }
                    };
                }

                return new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                    Height = height
                };
            }
        }

        private double floatingAverage;
        private Container colourBars;

        private const int max_concurrent_judgements = 50;

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit || judgement.HitObject.HitWindows?.WindowFor(HitResult.Miss) == 0)
                return;

            if (!judgement.Type.IsScorable() || judgement.Type.IsBonus())
                return;

            if (judgementsContainer.Count > max_concurrent_judgements)
            {
                const double quick_fade_time = 100;

                // check with a bit of lenience to avoid precision error in comparison.
                var old = judgementsContainer.FirstOrDefault(j => j.LifetimeEnd > Clock.CurrentTime + quick_fade_time * 1.1);

                if (old != null)
                {
                    old.ClearTransforms();
                    old.FadeOut(quick_fade_time).Expire();
                }
            }

            judgementsContainer.Add(new JudgementLine
            {
                Y = getRelativeJudgementPosition(judgement.TimeOffset),
                Origin = Anchor.CentreLeft,
            });

            arrow.MoveToY(
                getRelativeJudgementPosition(floatingAverage = floatingAverage * 0.9 + judgement.TimeOffset * 0.1)
                , arrow_move_duration, Easing.Out);
        }

        private float getRelativeJudgementPosition(double value) => Math.Clamp((float)((value / maxHitWindow) + 1) / 2, 0, 1);

        internal class JudgementLine : CompositeDrawable
        {
            private const int judgement_fade_duration = 5000;

            public JudgementLine()
            {
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;
                Height = 3;

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

                Width = 0;

                this.ResizeWidthTo(1, 200, Easing.OutElasticHalf);
                this.FadeTo(0.8f, 150).Then().FadeOut(judgement_fade_duration).Expire();
            }
        }

        public override void Clear() => judgementsContainer.Clear();
    }
}
