// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.Break;
using osu.Game.Utils;

namespace osu.Game.Screens.Play
{
    public partial class BreakOverlay : Container
    {
        /// <summary>
        /// The duration of the break overlay fading.
        /// </summary>
        public const double BREAK_FADE_DURATION = BreakPeriod.MIN_BREAK_DURATION / 2;

        private const float remaining_time_container_max_size = 0.3f;
        private const int vertical_margin = 15;

        private readonly Container fadeContainer;

        public override bool RemoveCompletedTransforms => false;

        public required BreakTracker BreakTracker { get; init; }

        private readonly Container remainingTimeAdjustmentBox;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly BreakArrows breakArrows;
        private readonly ScoreProcessor scoreProcessor;
        private readonly BreakInfo info;

        private readonly IBindable<Period?> currentPeriod = new Bindable<Period?>();

        /// <summary>
        /// The beatmap time at which the current break finishes fading out.
        /// </summary>
        /// <remarks>
        /// Deliberately retained after <see cref="currentPeriod"/> elapses, so that the countdown can keep ticking
        /// down to zero while the overlay is still fading out.
        /// </remarks>
        private double? breakEndTime;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        public BreakOverlay(ScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;
            RelativeSizeAxes = Axes.Both;

            Child = fadeContainer = new Container
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 80,
                        Height = 4,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Radius = 260,
                            Colour = OsuColour.Gray(0.2f).Opacity(0.8f),
                            Roundness = 12
                        },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Alpha = 0,
                                AlwaysPresent = true,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
                    remainingTimeAdjustmentBox = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Width = 0,
                        Child = remainingTimeBox = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            Height = 8,
                            Masking = true,
                        }
                    },
                    remainingTimeCounter = new RemainingTimeCounter
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.BottomCentre,
                        Y = -vertical_margin,
                    },
                    info = new BreakInfo
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.TopCentre,
                        Y = vertical_margin,
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

            info.AccuracyDisplay.Current.BindTo(scoreProcessor.Accuracy);
            ((IBindable<ScoreRank>)info.GradeDisplay.Current).BindTo(scoreProcessor.Rank);

            currentPeriod.BindTo(BreakTracker.CurrentPeriod);
            currentPeriod.BindValueChanged(updateDisplay, true);
        }

        private float remainingTimeForCurrentPeriod =>
            currentPeriod.Value == null ? 0 : (float)Math.Max(0, (currentPeriod.Value.Value.End - Time.Current - BREAK_FADE_DURATION) / currentPeriod.Value.Value.Duration);

        /// <summary>
        /// How fast beatmap time is progressing relative to real time at the current point in time, as dictated by the
        /// active mods.
        /// </summary>
        /// <remarks>
        /// Mods which vary their rate over the course of a beatmap are sampled at the current point in time rather than
        /// projected forward. For wind up / wind down the rate drifts slowly enough over a single break for this to be
        /// imperceptible, while adaptive speed is driven by the player's input and cannot be predicted at all.
        /// </remarks>
        private double gameplayRate
        {
            get
            {
                double rate = 1;

                foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                {
                    // adaptive speed's `ApplyToRate()` can only report the rate gameplay started at,
                    // so read the rate it is actually running at instead.
                    rate = mod is ModAdaptiveSpeed adaptiveSpeed
                        ? rate * adaptiveSpeed.SpeedChange.Value
                        : mod.ApplyToRate(Time.Current, rate);
                }

                return rate;
            }
        }

        protected override void Update()
        {
            base.Update();

            remainingTimeBox.Width = (float)Interpolation.DampContinuously(remainingTimeBox.Width, remainingTimeForCurrentPeriod, 40, Math.Abs(Time.Elapsed));
            remainingTimeBox.Height = Math.Min(8, remainingTimeBox.DrawWidth);

            // the countdown is displayed in real time rather than beatmap time, so that it lines up with how long the
            // break will actually last for the user when rate-changing mods are active. it is updated every frame
            // rather than transformed, as the rate can change over the course of a break.
            if (breakEndTime != null)
                remainingTimeCounter.RemainingTime = Math.Max(0, breakEndTime.Value - Time.Current) / gameplayRate;
        }

        private void updateDisplay(ValueChangedEvent<Period?> period)
        {
            Scheduler.CancelDelayedTasks();

            if (period.NewValue == null)
                return;

            var b = period.NewValue.Value;

            breakEndTime = b.End + BREAK_FADE_DURATION;

            using (BeginAbsoluteSequence(b.Start))
            {
                fadeContainer.FadeIn(BREAK_FADE_DURATION);
                breakArrows.Show(BREAK_FADE_DURATION);

                remainingTimeAdjustmentBox
                    .ResizeWidthTo(remaining_time_container_max_size, BREAK_FADE_DURATION, Easing.OutQuint)
                    .Delay(b.Duration)
                    .ResizeWidthTo(0);

                remainingTimeCounter.MoveToX(-50)
                                    .MoveToX(0, BREAK_FADE_DURATION, Easing.OutQuint);

                info.MoveToX(50)
                    .MoveToX(0, BREAK_FADE_DURATION, Easing.OutQuint);

                using (BeginDelayedSequence(b.Duration))
                {
                    fadeContainer.FadeOut(BREAK_FADE_DURATION);
                    breakArrows.Hide(BREAK_FADE_DURATION);
                }
            }
        }
    }
}
