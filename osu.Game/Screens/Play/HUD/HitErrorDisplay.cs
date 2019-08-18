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
using osu.Game.Configuration;

namespace osu.Game.Screens.Play.HUD
{
    public class HitErrorDisplay : CompositeDrawable
    {
        private const int stored_judgements_amount = 5;
        private const int bar_width = 3;
        private const int judgement_line_width = 8;
        private const int bar_height = 200;
        private const int spacing = 3;

        public HitWindows HitWindows { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly SpriteIcon arrow;
        private readonly FillFlowContainer bar;
        private readonly Container judgementsContainer;
        private readonly Queue<double> judgementOffsets = new Queue<double>();

        private readonly Bindable<ScoreMeterType> type = new Bindable<ScoreMeterType>();

        public HitErrorDisplay(bool reversed = false)
        {
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
                    bar = new FillFlowContainer
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
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ScoreMeter, type);
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
                    Height = (float)((getMehHitWindows() - HitWindows.Good) / (getMehHitWindows() * 2))
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Green,
                    Height = (float)((HitWindows.Good - HitWindows.Great) / (getMehHitWindows() * 2))
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.BlueLight,
                    Height = (float)(HitWindows.Great / getMehHitWindows())
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Green,
                    Height = (float)((HitWindows.Good - HitWindows.Great) / (getMehHitWindows() * 2))
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(colours.Yellow, colours.Yellow.Opacity(0)),
                    Height = (float)((getMehHitWindows() - HitWindows.Good) / (getMehHitWindows() * 2))
                }
            });

            type.BindValueChanged(onTypeChanged, true);
        }

        private double getMehHitWindows()
        {
            // In case if ruleset has no Meh hit windows (like Taiko)
            if (HitWindows.Meh == 0)
                return HitWindows.Good + 40;

            return HitWindows.Meh;
        }

        private void onTypeChanged(ValueChangedEvent<ScoreMeterType> type)
        {
            switch (type.NewValue)
            {
                case ScoreMeterType.None:
                    this.FadeOut(200, Easing.OutQuint);
                    break;

                case ScoreMeterType.HitError:
                    this.FadeIn(200, Easing.OutQuint);
                    break;
            }
        }

        public void OnNewJudgement(JudgementResult newJudgement)
        {
            if (!newJudgement.IsHit)
                return;

            Container judgementLine;

            judgementsContainer.Add(judgementLine = CreateJudgementLine(newJudgement));

            judgementLine.FadeOut(10000, Easing.OutQuint);
            judgementLine.Expire();

            arrow.MoveToY(calculateArrowPosition(newJudgement), 500, Easing.OutQuint);
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

        private float getRelativeJudgementPosition(double value) => (float)(value / getMehHitWindows());

        private float calculateArrowPosition(JudgementResult newJudgement)
        {
            if (judgementOffsets.Count > stored_judgements_amount)
                judgementOffsets.Dequeue();

            judgementOffsets.Enqueue(newJudgement.TimeOffset);

            return getRelativeJudgementPosition(judgementOffsets.Average());
        }
    }
}
