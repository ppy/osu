// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.Break;
using osu.Game.Utils;

namespace osu.Game.Screens.Play
{
    public partial class BreakOverlay : BeatSyncedContainer
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

        public BreakOverlay(ScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;
            RelativeSizeAxes = Axes.Both;

            MinimumBeatLength = 200;

            // Doesn't play well with pause/unpause.
            // This might mean that some beats don't animate if the user is running <60fps, but we'll deal with that if anyone notices.
            AllowMistimedEventFiring = false;

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

        protected override void Update()
        {
            base.Update();

            remainingTimeBox.Height = Math.Min(8, remainingTimeBox.DrawWidth);

            // Keep things simple by resetting beat synced transforms on a rewind.
            if (Clock.ElapsedFrameTime < 0)
            {
                remainingTimeBox.ClearTransforms(targetMember: nameof(Width));
                remainingTimeBox.Width = remainingTimeForCurrentPeriod;
            }
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (currentPeriod.Value == null)
                return;

            float timeBoxTargetWidth = (float)Math.Max(0, remainingTimeForCurrentPeriod - timingPoint.BeatLength / currentPeriod.Value.Value.Duration);
            remainingTimeBox.ResizeWidthTo(timeBoxTargetWidth, timingPoint.BeatLength * 3.5, Easing.OutQuint);
        }

        private void updateDisplay(ValueChangedEvent<Period?> period)
        {
            FinishTransforms(true);
            Scheduler.CancelDelayedTasks();

            if (period.NewValue == null)
                return;

            var b = period.NewValue.Value;

            using (BeginAbsoluteSequence(b.Start))
            {
                fadeContainer.FadeIn(BREAK_FADE_DURATION);
                breakArrows.Show(BREAK_FADE_DURATION);

                remainingTimeAdjustmentBox
                    .ResizeWidthTo(remaining_time_container_max_size, BREAK_FADE_DURATION, Easing.OutQuint)
                    .Delay(b.Duration - BREAK_FADE_DURATION)
                    .ResizeWidthTo(0);

                remainingTimeBox.ResizeWidthTo(remainingTimeForCurrentPeriod);

                remainingTimeCounter.CountTo(b.Duration).CountTo(0, b.Duration);

                remainingTimeCounter.MoveToX(-50)
                                    .MoveToX(0, BREAK_FADE_DURATION, Easing.OutQuint);

                info.MoveToX(50)
                    .MoveToX(0, BREAK_FADE_DURATION, Easing.OutQuint);

                using (BeginDelayedSequence(b.Duration - BREAK_FADE_DURATION))
                {
                    fadeContainer.FadeOut(BREAK_FADE_DURATION);
                    breakArrows.Hide(BREAK_FADE_DURATION);
                }
            }
        }
    }
}
