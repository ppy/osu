// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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

                // reset index in case the new breaks list is smaller than last one
                resetBreakIndex();

                initializeBreaks();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public IBindable<bool> IsBreakTime => isBreakTime;

        private int currentBreakIndex;
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

        private void resetBreakIndex()
        {
            isBreakTime.Value = false;
            currentBreakIndex = 0;
        }

        private void updateBreakTimeBindable()
        {
            if (breaks?.Any() != true)
            {
                resetBreakIndex();
                return;
            }

            var time = Clock.CurrentTime;

            if (currentBreakIndex < breaks.Count - 1)
            {
                // Quick check if we're not in a break with our current index.
                if (time > breaks[currentBreakIndex].EndTime && time < breaks[currentBreakIndex + 1].StartTime)
                {
                    isBreakTime.Value = false;
                    return;
                }
            }

            if (time > breaks[currentBreakIndex].EndTime)
            {
                for (int i = currentBreakIndex; i < breaks.Count; i++)
                {
                    if (time <= breaks[i].EndTime)
                    {
                        currentBreakIndex = i;
                        break;
                    }
                }
            }
            else if (time < breaks[currentBreakIndex].StartTime)
            {
                for (int i = currentBreakIndex; i >= 0; i--)
                {
                    if (time >= breaks[i].StartTime)
                    {
                        currentBreakIndex = i;
                        break;
                    }
                }
            }

            // This ensures that IsBreakTime is generally consistent with the overlay's transforms during a break.
            // If the current break doesn't have effects, IsBreakTime should be false.
            // We also assume that the overlay's fade out transform is "not break time".
            var currentBreak = breaks[currentBreakIndex];
            isBreakTime.Value = currentBreak.HasEffect && time >= currentBreak.StartTime && time <= currentBreak.EndTime - fade_duration;
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
    }
}
