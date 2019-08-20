// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using System.Linq;

namespace osu.Game.Screens.Play.HitErrorDisplay
{
    public class BarHitErrorDisplay : HitErrorDisplay
    {
        /// <summary>
        /// The amount of <see cref="JudgementResult"/> which will be stored to calculate arrow position.
        /// </summary>
        private const int stored_judgements_amount = 5;
        private const int judgement_fade_duration = 10000;
        private const int arrow_move_duration = 500;
        private const int judgement_line_width = 8;
        private const int bar_height = 200;
        private const int bar_width = 3;
        private const int spacing = 3;

        private readonly SpriteIcon arrow;
        private readonly FillFlowContainer<Box> bar;
        private readonly Container judgementsContainer;
        private readonly Queue<double> judgementOffsets = new Queue<double>();
        private readonly double maxHitWindows;

        public BarHitErrorDisplay(HitWindows hitWindows, bool reversed = false)
            : base(hitWindows)
        {
            maxHitWindows = HitWindows.Meh == 0 ? HitWindows.Good : HitWindows.Meh;

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
                        Anchor = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                        Width = judgement_line_width,
                        RelativeSizeAxes = Axes.Y,
                    },
                    bar = new FillFlowContainer<Box>
                    {
                        Anchor = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                        Width = bar_width,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    },
                    new Container
                    {
                        Anchor = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Child = arrow = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Y,
                            Icon = reversed ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
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
                    createColoredPiece(ColourInfo.GradientVertical(colours.Yellow.Opacity(0), colours.Yellow),
                        (maxHitWindows - HitWindows.Good) / (maxHitWindows * 2)),
                    createColoredPiece(colours.Green, (HitWindows.Good - HitWindows.Great) / (maxHitWindows * 2)),
                    createColoredPiece(colours.BlueLight, HitWindows.Great / maxHitWindows),
                    createColoredPiece(colours.Green, (HitWindows.Good - HitWindows.Great) / (maxHitWindows * 2)),
                    createColoredPiece(ColourInfo.GradientVertical(colours.Yellow, colours.Yellow.Opacity(0)),
                        (maxHitWindows - HitWindows.Good) / (maxHitWindows * 2))
                });
            }
            else
            {
                bar.AddRange(new[]
                {
                    createColoredPiece(ColourInfo.GradientVertical(colours.Green.Opacity(0), colours.Green),
                        (HitWindows.Good - HitWindows.Great) / (maxHitWindows * 2)),
                    createColoredPiece(colours.BlueLight, HitWindows.Great / maxHitWindows),
                    createColoredPiece(ColourInfo.GradientVertical(colours.Green, colours.Green.Opacity(0)),
                        (HitWindows.Good - HitWindows.Great) / (maxHitWindows * 2)),
                });
            }
        }

        private Box createColoredPiece(ColourInfo colour, double height) => new Box
        {
            RelativeSizeAxes = Axes.Both,
            Colour = colour,
            Height = (float)height
        };

        public override void OnNewJudgement(JudgementResult newJudgement)
        {
            if (!newJudgement.IsHit)
                return;

            var judgementLine = CreateJudgementLine(newJudgement);

            judgementsContainer.Add(judgementLine);

            judgementLine.FadeOut(judgement_fade_duration, Easing.OutQuint).Expire();

            arrow.MoveToY(calculateArrowPosition(newJudgement), arrow_move_duration, Easing.OutQuint);
        }

        protected virtual Container CreateJudgementLine(JudgementResult judgement) => new CircularContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Masking = true,
            RelativeSizeAxes = Axes.X,
            Height = 2,
            RelativePositionAxes = Axes.Y,
            Y = getRelativeJudgementPosition(judgement.TimeOffset),
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            }
        };

        private float getRelativeJudgementPosition(double value) => (float)(value / maxHitWindows);

        private float calculateArrowPosition(JudgementResult newJudgement)
        {
            if (judgementOffsets.Count > stored_judgements_amount)
                judgementOffsets.Dequeue();

            judgementOffsets.Enqueue(newJudgement.TimeOffset);

            return getRelativeJudgementPosition(judgementOffsets.Average());
        }
    }
}
