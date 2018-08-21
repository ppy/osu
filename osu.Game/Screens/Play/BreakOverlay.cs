﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.Break;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container, IKeyBindingHandler<GlobalAction>
    {
        public IAdjustableClock AdjustableClock;

        private const double skip_required_cutoff = 3000;
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const float remaining_time_container_max_size = 0.3f;
        private const int vertical_margin = 25;

        private List<BreakPeriod> breaks;

        private readonly Container fadeContainer;

        public List<BreakPeriod> Breaks
        {
            get => breaks;
            set
            {
                breaks = value;
                initializeBreaks();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        private readonly Container remainingTimeAdjustmentBox;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly BreakInfo info;
        private readonly BreakArrows breakArrows;

        public BreakOverlay(bool letterboxing, ScoreProcessor scoreProcessor)
            : this(letterboxing)
        {
            bindProcessor(scoreProcessor);
        }

        public BreakOverlay(bool letterboxing)
        {
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
        }

        private void initializeBreaks()
        {
            if (!IsLoaded) return; // we need a clock.

            FinishTransforms(true);
            Scheduler.CancelDelayedTasks();

            if (breaks == null) return; //we need breaks.

            foreach (var b in breaks)
            {
                if (!b.HasEffect)
                    continue;

                using (BeginAbsoluteSequence(b.StartTime, true))
                {
                    fadeContainer.FadeIn(fade_duration);
                    breakArrows.Show(fade_duration);

                    remainingTimeAdjustmentBox
                        .ResizeWidthTo(remaining_time_container_max_size, fade_duration, Easing.OutQuint)
                        .Delay(b.Duration - fade_duration)
                        .ResizeWidthTo(0);

                    remainingTimeBox
                        .ResizeWidthTo(0, b.Duration - fade_duration)
                        .Then()
                        .ResizeWidthTo(1);

                    remainingTimeCounter.CountTo(b.Duration).CountTo(0, b.Duration);

                    using (BeginDelayedSequence(b.Duration - fade_duration, true))
                    {
                        fadeContainer.FadeOut(fade_duration);
                        breakArrows.Hide(fade_duration);
                    }
                }
            }
        }

        private void bindProcessor(ScoreProcessor processor)
        {
            info.AccuracyDisplay.Current.BindTo(processor.Accuracy);
            info.GradeDisplay.Current.BindTo(processor.Rank);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SkipCutscene:
                    double startTime = -1;
                    foreach (var b in breaks)
                    {
                        if(Time.Current > b.StartTime && (Time.Current + skip_required_cutoff + fade_duration) < b.EndTime)
                        {
                            startTime = b.EndTime;
                            break;
                        }
                    }
                    if (startTime == -1)
                        return false;
                    AdjustableClock?.Seek(startTime - skip_required_cutoff - fade_duration);
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;
    }
}
