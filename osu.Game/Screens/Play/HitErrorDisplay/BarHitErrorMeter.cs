// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Play.HitErrorDisplay
{
    public class BarHitErrorMeter : HitErrorMeter
    {
        private readonly bool rightAligned;

        private const int judgement_fade_duration = 10000;

        private const int arrow_move_duration = 400;

        private const int judgement_line_width = 8;

        private const int bar_height = 200;

        private const int bar_width = 3;

        private const int spacing = 3;

        private readonly SpriteIcon arrow;

        private readonly FillFlowContainer<Box> bar;

        private readonly Container judgementsContainer;

        private readonly double maxHitWindow;

        public BarHitErrorMeter(HitWindows hitWindows, bool rightAligned = false)
            : base(hitWindows)
        {
            this.rightAligned = rightAligned;
            maxHitWindow = Math.Max(Math.Max(HitWindows.Meh, HitWindows.Ok), HitWindows.Good);

            AutoSizeAxes = Axes.Both;

            AddInternal(new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                Height = bar_height,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(spacing, 0),
                Children = new Drawable[]
                {
                    judgementsContainer = new Container
                    {
                        Anchor = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                        Width = judgement_line_width,
                        RelativeSizeAxes = Axes.Y,
                    },
                    bar = new FillFlowContainer<Box>
                    {
                        Anchor = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                        Width = bar_width,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    },
                    new Container
                    {
                        Anchor = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Child = arrow = new SpriteIcon
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Y,
                            Y = 0.5f,
                            Icon = rightAligned ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
                            Size = new Vector2(8),
                        }
                    },
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (HitWindows.Meh != 0)
            {
                bar.AddRange(new[]
                {
                    createColoredPiece(ColourInfo.GradientVertical(colours.Yellow.Opacity(0), colours.Yellow), (maxHitWindow - HitWindows.Good) / (maxHitWindow * 2)),
                    createColoredPiece(colours.Green, (HitWindows.Good - HitWindows.Great) / (maxHitWindow * 2)),
                    createColoredPiece(colours.BlueLight, HitWindows.Great / maxHitWindow),
                    createColoredPiece(colours.Green, (HitWindows.Good - HitWindows.Great) / (maxHitWindow * 2)),
                    createColoredPiece(ColourInfo.GradientVertical(colours.Yellow, colours.Yellow.Opacity(0)), (maxHitWindow - HitWindows.Good) / (maxHitWindow * 2))
                });
            }
            else
            {
                bar.AddRange(new[]
                {
                    createColoredPiece(ColourInfo.GradientVertical(colours.Green.Opacity(0), colours.Green), (HitWindows.Good - HitWindows.Great) / (maxHitWindow * 2)),
                    createColoredPiece(colours.BlueLight, HitWindows.Great / maxHitWindow),
                    createColoredPiece(ColourInfo.GradientVertical(colours.Green, colours.Green.Opacity(0)), (HitWindows.Good - HitWindows.Great) / (maxHitWindow * 2)),
                });
            }
        }

        private Box createColoredPiece(ColourInfo colour, double height) => new Box
        {
            RelativeSizeAxes = Axes.Both,
            Colour = colour,
            Height = (float)height
        };

        private double floatingAverage;

        public override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit)
                return;

            judgementsContainer.Add(new JudgementLine
            {
                Y = getRelativeJudgementPosition(judgement.TimeOffset),
                Anchor = rightAligned ? Anchor.TopLeft : Anchor.TopRight,
                Origin = rightAligned ? Anchor.TopLeft : Anchor.TopRight,
            });

            arrow.MoveToY(getRelativeJudgementPosition(floatingAverage = floatingAverage * 0.9 + judgement.TimeOffset * 0.1)
                , arrow_move_duration, Easing.Out);
        }

        private float getRelativeJudgementPosition(double value) => (float)((value / maxHitWindow) + 1) / 2;

        public class JudgementLine : CompositeDrawable
        {
            public JudgementLine()
            {
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;
                Height = 2;

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
                this.ResizeWidthTo(1, 150, Easing.OutElasticHalf);
                this.FadeTo(0.8f, 150).Then().FadeOut(judgement_fade_duration, Easing.OutQuint).Expire();
            }
        }
    }
}
