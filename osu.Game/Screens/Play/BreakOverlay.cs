// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.Break;

namespace osu.Game.Screens.Play
{
    public partial class BreakOverlay : Container
    {
        /// <summary>
        /// The duration of the break overlay fading.
        /// </summary>
        public const double BREAK_FADE_DURATION = BreakPeriod.MIN_BREAK_DURATION / 2;

        private const float remaining_time_container_max_size = 0.3f;
        private const int vertical_margin = 25;

        private readonly Container fadeContainer;

        private IReadOnlyList<BreakPeriod> breaks;

        public IReadOnlyList<BreakPeriod> Breaks
        {
            get => breaks;
            set
            {
                breaks = value;

                if (IsLoaded)
                    initializeBreaks();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        private readonly Container remainingTimeAdjustmentBox;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly BreakArrows breakArrows;
        private readonly ScoreProcessor scoreProcessor;
        private readonly BreakInfo info;

        public BreakOverlay(bool letterboxing, ScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;
            RelativeSizeAxes = Axes.Both;

            Child = fadeContainer = new Container
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new LetterboxOverlay
                    {
                        Alpha = letterboxing ? 1 : 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    remainingTimeAdjustmentBox = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Width = 0,
                        Child = remainingTimeBox = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            Height = 8,
                            CornerRadius = 4,
                            Masking = true,
                            Child = new Box { RelativeSizeAxes = Axes.Both }
                        }
                    },
                    remainingTimeCounter = new RemainingTimeCounter
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding { Bottom = vertical_margin },
                    },
                    info = new BreakInfo
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = vertical_margin },
                    },
                    breakArrows = new BreakArrows
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            initializeBreaks();

            if (scoreProcessor != null)
            {
                info.AccuracyDisplay.Current.BindTo(scoreProcessor.Accuracy);
                info.GradeDisplay.Current.BindTo(scoreProcessor.Rank);
            }
        }

        private void initializeBreaks()
        {
            FinishTransforms(true);
            Scheduler.CancelDelayedTasks();

            if (breaks == null) return; // we need breaks.

            foreach (var b in breaks)
            {
                if (!b.HasEffect)
                    continue;

                using (BeginAbsoluteSequence(b.StartTime))
                {
                    fadeContainer.FadeIn(BREAK_FADE_DURATION);
                    breakArrows.Show(BREAK_FADE_DURATION);

                    remainingTimeAdjustmentBox
                        .ResizeWidthTo(remaining_time_container_max_size, BREAK_FADE_DURATION, Easing.OutQuint)
                        .Delay(b.Duration - BREAK_FADE_DURATION)
                        .ResizeWidthTo(0);

                    remainingTimeBox
                        .ResizeWidthTo(0, b.Duration - BREAK_FADE_DURATION)
                        .Then()
                        .ResizeWidthTo(1);

                    remainingTimeCounter.CountTo(b.Duration).CountTo(0, b.Duration);

                    using (BeginDelayedSequence(b.Duration - BREAK_FADE_DURATION))
                    {
                        fadeContainer.FadeOut(BREAK_FADE_DURATION);
                        breakArrows.Hide(BREAK_FADE_DURATION);
                    }
                }
            }
        }
    }
}
