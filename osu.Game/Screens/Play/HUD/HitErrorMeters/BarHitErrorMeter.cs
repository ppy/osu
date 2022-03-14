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

namespace osu.Game.Screens.Play.HUD.HitErrorMeters
{
    public class BarHitErrorMeter : HitErrorMeter
    {
        private const int judgement_line_width = 14;
        private const int judgement_line_height = 4;

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
            const int centre_marker_size = 8;
            const int bar_height = 200;
            const int bar_width = 2;
            const float chevron_size = 8;
            const float icon_size = 14;

            var hitWindows = HitWindows.GetAllAvailableWindows().ToArray();

            InternalChild = new Container
            {
                AutoSizeAxes = Axes.X,
                Height = bar_height,
                Margin = new MarginPadding(2),
                Children = new Drawable[]
                {
                    colourBars = new Container
                    {
                        Name = "colour axis",
                        X = chevron_size,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = judgement_line_width,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            iconEarly = new SpriteIcon
                            {
                                Y = -10,
                                Size = new Vector2(icon_size),
                                Icon = FontAwesome.Solid.ShippingFast,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                            },
                            iconLate = new SpriteIcon
                            {
                                Y = 10,
                                Size = new Vector2(icon_size),
                                Icon = FontAwesome.Solid.Bicycle,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                            },
                            colourBarsEarly = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.TopCentre,
                                Width = bar_width,
                                RelativeSizeAxes = Axes.Y,
                                Height = 0.5f,
                                Scale = new Vector2(1, -1),
                            },
                            colourBarsLate = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.TopCentre,
                                Width = bar_width,
                                RelativeSizeAxes = Axes.Y,
                                Height = 0.5f,
                            },
                            new Circle
                            {
                                Name = "middle marker behind",
                                Colour = GetColourForHitResult(hitWindows.Last().result),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(centre_marker_size),
                            },
                            judgementsContainer = new Container
                            {
                                Name = "judgements",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                                Width = judgement_line_width,
                            },
                            new Circle
                            {
                                Name = "middle marker in front",
                                Colour = GetColourForHitResult(hitWindows.Last().result).Darken(0.3f),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(centre_marker_size),
                                Scale = new Vector2(0.5f),
                            },
                        }
                    },
                    new Container
                    {
                        Name = "average chevron",
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
                }
            };

            createColourBars(hitWindows);
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

        private void createColourBars((HitResult result, double length)[] windows)
        {
            // max to avoid div-by-zero.
            maxHitWindow = Math.Max(1, windows.First().length);

            for (int i = 0; i < windows.Length; i++)
            {
                (var result, double length) = windows[i];

                float hitWindow = (float)(length / maxHitWindow);

                colourBarsEarly.Add(createColourBar(result, hitWindow, i == 0));
                colourBarsLate.Add(createColourBar(result, hitWindow, i == 0));
            }

            Drawable createColourBar(HitResult result, float height, bool requireGradient = false)
            {
                var colour = GetColourForHitResult(result);

                if (requireGradient)
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
            const int arrow_move_duration = 800;

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
                Colour = GetColourForHitResult(judgement.Type),
            });

            arrow.MoveToY(
                getRelativeJudgementPosition(floatingAverage = floatingAverage * 0.9 + judgement.TimeOffset * 0.1)
                , arrow_move_duration, Easing.OutQuint);
        }

        private float getRelativeJudgementPosition(double value) => Math.Clamp((float)((value / maxHitWindow) + 1) / 2, 0, 1);

        internal class JudgementLine : CompositeDrawable
        {
            public JudgementLine()
            {
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;
                Height = judgement_line_height;

                Blending = BlendingParameters.Additive;

                Origin = Anchor.Centre;
                Anchor = Anchor.TopCentre;

                InternalChild = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            protected override void LoadComplete()
            {
                const int judgement_fade_in_duration = 100;
                const int judgement_fade_out_duration = 5000;

                base.LoadComplete();

                Alpha = 0;
                Width = 0;

                this
                    .FadeTo(0.6f, judgement_fade_in_duration, Easing.OutQuint)
                    .ResizeWidthTo(1, judgement_fade_in_duration, Easing.OutQuint)
                    .Then()
                    .FadeOut(judgement_fade_out_duration)
                    .ResizeWidthTo(0, judgement_fade_out_duration, Easing.InQuint)
                    .Expire();
            }
        }

        public override void Clear() => judgementsContainer.Clear();
    }
}
