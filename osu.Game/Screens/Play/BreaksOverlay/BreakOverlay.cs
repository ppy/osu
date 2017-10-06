// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Scoring;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class BreakOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const float remaining_time_container_max_size = 0.3f;
        private const int vertical_margin = 25;

        private List<BreakPeriod> breaks;
        public List<BreakPeriod> Breaks
        {
            set
            {
                breaks = value;
                initializeBreaks();
            }
            get
            {
                return breaks;
            }
        }

        private readonly bool letterboxing;
        private readonly LetterboxOverlay letterboxOverlay;
        private readonly Container remainingTimeAdjustmentBox;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly InfoContainer info;
        private readonly ArrowsOverlay arrowsOverlay;

        public BreakOverlay(bool letterboxing)
        {
            this.letterboxing = letterboxing;

            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                letterboxOverlay = new LetterboxOverlay
                {
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
                info = new InfoContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = vertical_margin },
                },
                arrowsOverlay = new ArrowsOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        private void initializeBreaks()
        {
            FinishTransforms(true);
            Scheduler.CancelDelayedTasks();

            if (breaks == null)
                return;

            foreach (var b in breaks)
            {
                if (!b.HasEffect)
                    continue;

                using (BeginAbsoluteSequence(b.StartTime))
                {
                    Schedule(() => onBreakIn(b));
                    using (BeginDelayedSequence(b.Duration - fade_duration))
                        Schedule(onBreakOut);
                }
            }
        }

        private void onBreakIn(BreakPeriod b)
        {
            if (letterboxing)
                letterboxOverlay.Show();

            remainingTimeAdjustmentBox
                .ResizeWidthTo(remaining_time_container_max_size, fade_duration, Easing.OutQuint)
                .Delay(b.Duration - fade_duration)
                .ResizeWidthTo(0);

            remainingTimeBox
                .ResizeWidthTo(0, b.Duration - fade_duration)
                .Then()
                .ResizeWidthTo(1);

            remainingTimeCounter.StartCounting(b.EndTime);

            remainingTimeCounter.Show();
            info.Show();
            arrowsOverlay.Show();
        }

        private void onBreakOut()
        {
            if (letterboxing)
                letterboxOverlay.Hide();

            remainingTimeCounter.Hide();
            info.Hide();
            arrowsOverlay.Hide();
        }

        public void BindProcessor(ScoreProcessor processor)
        {
            info.AccuracyDisplay.Current.BindTo(processor.Accuracy);
            info.GradeDisplay.Current.BindTo(processor.Rank);
        }
    }
}
