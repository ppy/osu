// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.Break;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container
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

                // reset index in case the new breaks list is smaller than last one
                isBreakTime.Value = false;
                CurrentBreakIndex = 0;

                if (IsLoaded)
                    initializeBreaks();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public IBindable<bool> IsBreakTime => isBreakTime;

        protected int CurrentBreakIndex;

        private readonly BindableBool isBreakTime = new BindableBool();

        private readonly Container remainingTimeAdjustmentBox;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly BreakInfo info;
        private readonly BreakArrows breakArrows;

        public BreakOverlay(bool letterboxing, ScoreProcessor scoreProcessor = null)
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

            if (scoreProcessor != null) bindProcessor(scoreProcessor);
        }

        [BackgroundDependencyLoader(true)]
        private void load(GameplayClock clock)
        {
            if (clock != null) Clock = clock;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            initializeBreaks();
        }

        protected override void Update()
        {
            base.Update();
            updateBreakTimeBindable();
        }

        private void updateBreakTimeBindable()
        {
            if (breaks == null || breaks.Count == 0)
                return;

            var time = Clock.CurrentTime;

            if (time > breaks[CurrentBreakIndex].EndTime)
            {
                while (time > breaks[CurrentBreakIndex].EndTime && CurrentBreakIndex < breaks.Count - 1)
                    CurrentBreakIndex++;
            }
            else
            {
                while (time < breaks[CurrentBreakIndex].StartTime && CurrentBreakIndex > 0)
                    CurrentBreakIndex--;
            }

            var currentBreak = breaks[CurrentBreakIndex];
            isBreakTime.Value = currentBreak.HasEffect && currentBreak.Contains(time);
        }

        private void initializeBreaks()
        {
            FinishTransforms(true);
            Scheduler.CancelDelayedTasks();

            if (breaks == null) return; //we need breaks.

            foreach (var b in breaks)
            {
                if (!b.HasEffect)
                    continue;

                using (BeginAbsoluteSequence(b.StartTime, true))
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

                    using (BeginDelayedSequence(b.Duration - BREAK_FADE_DURATION, true))
                    {
                        fadeContainer.FadeOut(BREAK_FADE_DURATION);
                        breakArrows.Hide(BREAK_FADE_DURATION);
                    }
                }
            }
        }

        private void bindProcessor(ScoreProcessor processor)
        {
            info.AccuracyDisplay.Current.BindTo(processor.Accuracy);
            info.GradeDisplay.Current.BindTo(processor.Rank);
        }
    }
}
