// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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
                remainingTimeBox = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0, 8),
                    CornerRadius = 4,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
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

                using (BeginAbsoluteSequence(b.StartTime, true))
                {
                    onBreakIn(b);

                    using (BeginDelayedSequence(b.Duration - fade_duration, true))
                        onBreakOut();
                }
            }
        }

        private void onBreakIn(BreakPeriod b)
        {
            if (letterboxing)
                letterboxOverlay.Show();

            remainingTimeBox
                .ResizeWidthTo(remaining_time_container_max_size, fade_duration, Easing.OutQuint)
                .Then()
                .ResizeWidthTo(0, b.Duration - fade_duration);

            Scheduler.AddDelayed(() => remainingTimeCounter.StartCounting(b.EndTime), b.StartTime - Clock.CurrentTime);

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
