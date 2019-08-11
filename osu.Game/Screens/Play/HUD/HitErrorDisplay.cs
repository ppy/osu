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
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Play.HUD
{
    public class HitErrorDisplay : Container
    {
        private const int bar_width = 4;
        private const int bar_height = 250;
        private const int spacing = 3;

        public HitWindows HitWindows { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

        private readonly bool mirrored;
        private readonly SpriteIcon arrow;
        private readonly List<double> judgementOffsets = new List<double>();

        public HitErrorDisplay(bool mirrored = false)
        {
            this.mirrored = mirrored;

            Size = new Vector2(bar_width, bar_height);

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Orange),
                            Height = 0.3f
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Green,
                            Height = 0.15f
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Blue,
                            Height = 0.1f
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Green,
                            Height = 0.15f
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Orange, Color4.Black.Opacity(0)),
                            Height = 0.3f
                        }
                    }
                },
                arrow = new SpriteIcon
                {
                    Anchor = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
                    Origin = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
                    X = mirrored ? -spacing : spacing,
                    RelativePositionAxes = Axes.Y,
                    Icon = mirrored ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
                    Size = new Vector2(10),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            HitWindows.SetDifficulty(beatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty);
        }

        public void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit)
                return;

            Container judgementLine;

            Add(judgementLine = CreateJudgementLine(judgement));

            judgementLine.FadeOut(5000, Easing.OutQuint);
            judgementLine.Expire();

            arrow.MoveToY(getRelativeJudgementPosition(calculateArrowPosition(judgement)), 500, Easing.OutQuint);
        }

        protected virtual Container CreateJudgementLine(JudgementResult judgement) => new CircularContainer
        {
            Anchor = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
            Origin = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
            Masking = true,
            Size = new Vector2(10, 2),
            RelativePositionAxes = Axes.Y,
            Y = getRelativeJudgementPosition(judgement.TimeOffset),
            X = mirrored ? spacing : -spacing,
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            }
        };

        private float getRelativeJudgementPosition(double value) => (float)(value / HitWindows.Miss);

        private double calculateArrowPosition(JudgementResult judgement)
        {
            if (judgementOffsets.Count > 5)
                judgementOffsets.RemoveAt(0);

            judgementOffsets.Add(judgement.TimeOffset);

            double offsets = 0;

            foreach (var offset in judgementOffsets)
                offsets += offset;

            return offsets / judgementOffsets.Count;
        }
    }
}
