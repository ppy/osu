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
using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using System.Linq;

namespace osu.Game.Screens.Play.HUD
{
    public class HitErrorDisplay : Container
    {
        private const int bar_width = 3;
        private const int bar_height = 200;
        private const int spacing = 3;

        public HitWindows HitWindows { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly bool mirrored;
        private readonly SpriteIcon arrow;
        private readonly FillFlowContainer bar;
        private readonly List<double> judgementOffsets = new List<double>();

        public HitErrorDisplay(bool mirrored = false)
        {
            this.mirrored = mirrored;

            Size = new Vector2(bar_width, bar_height);

            Children = new Drawable[]
            {
                bar = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                },
                arrow = new SpriteIcon
                {
                    Anchor = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
                    Origin = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
                    X = mirrored ? -spacing : spacing,
                    RelativePositionAxes = Axes.Y,
                    Icon = mirrored ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
                    Size = new Vector2(8),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            HitWindows.SetDifficulty(beatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            bar.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(colours.Yellow.Opacity(0), colours.Yellow),
                    Height = (float)((HitWindows.Meh - HitWindows.Good) / (HitWindows.Meh * 2))
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Green,
                    Height = (float)((HitWindows.Good - HitWindows.Great) / (HitWindows.Meh * 2))
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.BlueLight,
                    Height = (float)(HitWindows.Great / HitWindows.Meh)
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Green,
                    Height = (float)((HitWindows.Good - HitWindows.Great) / (HitWindows.Meh * 2))
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(colours.Yellow, colours.Yellow.Opacity(0)),
                    Height = (float)((HitWindows.Meh - HitWindows.Good) / (HitWindows.Meh * 2))
                }
            });
        }

        public void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit)
                return;

            Container judgementLine;

            Add(judgementLine = CreateJudgementLine(judgement));

            judgementLine.FadeOut(10000, Easing.OutQuint);
            judgementLine.Expire();

            arrow.MoveToY(getRelativeJudgementPosition(calculateArrowPosition(judgement)), 500, Easing.OutQuint);
        }

        protected virtual Container CreateJudgementLine(JudgementResult judgement) => new CircularContainer
        {
            Anchor = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
            Origin = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
            Masking = true,
            Size = new Vector2(8, 2),
            RelativePositionAxes = Axes.Y,
            Y = getRelativeJudgementPosition(judgement.TimeOffset),
            X = mirrored ? spacing : -spacing,
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            }
        };

        private float getRelativeJudgementPosition(double value) => (float)(value / HitWindows.Meh);

        private double calculateArrowPosition(JudgementResult judgement)
        {
            if (judgementOffsets.Count > 5)
                judgementOffsets.RemoveAt(0);

            judgementOffsets.Add(judgement.TimeOffset);

            return judgementOffsets.Average();
        }
    }
}
