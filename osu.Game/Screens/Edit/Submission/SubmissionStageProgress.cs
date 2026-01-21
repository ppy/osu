// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class SubmissionStageProgress : CompositeDrawable
    {
        public LocalisableString StageDescription { get; init; }

        public int StageIndex { get; init; }

        private Bindable<StageStatusType> status { get; } = new Bindable<StageStatusType>();

        private Bindable<float?> progress { get; } = new Bindable<float?>();

        private Container progressBarContainer = null!;
        private Box progressBar = null!;
        private Container iconContainer = null!;
        private OsuTextFlowContainer errorMessage = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Sample? progressSample;

        private const int stage_done_sample_count = 4;
        private Sample? stageDoneSample;

        private Sample? errorSample;
        private Sample? cancelSample;

        private SampleChannel? progressSampleChannel;

        private const int fadeout_duration = 100;
        private ScheduledDelegate? progressSampleFadeDelegate;
        private ScheduledDelegate? progressSampleStopDelegate;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, AudioManager audio)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = StageDescription,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        iconContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children =
                            [
                                progressBarContainer = new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Width = 150,
                                    Height = 10,
                                    CornerRadius = 5,
                                    Masking = true,
                                    Children = new[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background6,
                                        },
                                        progressBar = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Width = 0,
                                            Colour = colourProvider.Highlight1,
                                        }
                                    }
                                },
                                errorMessage = new OsuTextFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    // should really be `CentreRight` too, but that's broken due to a framework bug
                                    // (https://github.com/ppy/osu-framework/issues/5084)
                                    TextAnchor = Anchor.BottomRight,
                                    Width = 450,
                                    AutoSizeAxes = Axes.Y,
                                    Alpha = 0,
                                    Colour = colours.Red1,
                                }
                            ]
                        }
                    }
                }
            };

            errorSample = audio.Samples.Get(@"UI/generic-error");
            cancelSample = audio.Samples.Get(@"UI/notification-cancel");
            progressSample = audio.Samples.Get(@"UI/bss-progress");

            int stageSample = Math.Min(stage_done_sample_count - 1, StageIndex);
            stageDoneSample = audio.Samples.Get(@$"UI/bss-stage-{stageSample}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            status.BindValueChanged(_ => Scheduler.AddOnce(updateStatus), true);
            progress.BindValueChanged(_ => Scheduler.AddOnce(updateProgress), true);

            progressSampleChannel = progressSample?.GetChannel();
            if (progressSampleChannel != null)
                progressSampleChannel.ManualFree = true;
        }

        public void SetNotStarted() => status.Value = StageStatusType.NotStarted;

        public void SetInProgress(float? progress = null)
        {
            this.progress.Value = progress;
            status.Value = StageStatusType.InProgress;

            if (progressSampleChannel == null)
                return;

            progressSampleChannel.Frequency.Value = 0.5f;
            progressSampleChannel.Volume.Value = 0.25f;
            progressSampleChannel.Looping = true;
        }

        public void SetCompleted() => status.Value = StageStatusType.Completed;

        public void SetFailed(string reason)
        {
            status.Value = StageStatusType.Failed;
            errorMessage.Text = reason;
        }

        public void SetCanceled() => status.Value = StageStatusType.Canceled;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            progressSampleChannel?.Stop();
            progressSampleChannel?.Dispose();
        }

        private const float transition_duration = 200;
        private const Easing transition_easing = Easing.OutQuint;

        private void updateProgress()
        {
            progressSampleFadeDelegate?.Cancel();
            progressSampleStopDelegate?.Cancel();

            progressBarContainer.FadeTo(status.Value == StageStatusType.InProgress && progress.Value != null ? 1 : 0, transition_duration, transition_easing);

            if (progress.Value is float progressValue)
            {
                progressBar.ResizeWidthTo(progressValue, transition_duration, transition_easing);

                if (progressSampleChannel == null || Precision.AlmostEquals(progressValue, 0f))
                    return;

                // Don't restart the looping sample if already playing
                if (!progressSampleChannel.Playing)
                    progressSampleChannel.Play();

                this.TransformBindableTo(progressSampleChannel.Frequency, 0.5f + (progressValue * 1.5f), transition_duration, transition_easing);
                this.TransformBindableTo(progressSampleChannel.Volume, 0.25f + (progressValue * .75f), transition_duration, transition_easing);

                progressSampleFadeDelegate = Scheduler.AddDelayed(() =>
                {
                    // Perform a fade-out before stopping the sample to prevent clicking.
                    this.TransformBindableTo(progressSampleChannel.Volume, 0, fadeout_duration);
                    progressSampleStopDelegate = Scheduler.AddDelayed(() => { progressSampleChannel.Stop(); }, fadeout_duration);
                }, transition_duration - fadeout_duration);
            }
        }

        private void updateStatus()
        {
            progressBarContainer.FadeTo(status.Value == StageStatusType.InProgress && progress.Value != null ? 1 : 0, transition_duration, Easing.OutQuint);
            errorMessage.FadeTo(status.Value == StageStatusType.Failed ? 1 : 0, transition_duration, Easing.OutQuint);

            iconContainer.Clear();
            iconContainer.ClearTransforms();

            switch (status.Value)
            {
                case StageStatusType.InProgress:
                    iconContainer.Child = new LoadingSpinner
                    {
                        Size = new Vector2(16),
                        State = { Value = Visibility.Visible, },
                    };
                    iconContainer.Colour = colours.Orange1;
                    break;

                case StageStatusType.Completed:
                    iconContainer.Child = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.CheckCircle,
                        Size = new Vector2(16),
                    };
                    iconContainer.Colour = colours.Green1;
                    iconContainer.FlashColour(Colour4.White, 1000, Easing.OutQuint);

                    // manually set progress value, as to trigger sample playback for the final section
                    progress.Value = 1;

                    stageDoneSample?.Play();

                    break;

                case StageStatusType.Failed:
                    iconContainer.Child = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.ExclamationCircle,
                        Size = new Vector2(16),
                    };
                    iconContainer.Colour = colours.Red1;
                    iconContainer.FlashColour(Colour4.White, 1000, Easing.OutQuint);
                    errorSample?.Play();
                    progressSampleChannel?.Stop();
                    break;

                case StageStatusType.Canceled:
                    iconContainer.Child = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Ban,
                        Size = new Vector2(16),
                    };
                    iconContainer.Colour = colours.Gray8;
                    iconContainer.FlashColour(Colour4.White, 1000, Easing.OutQuint);
                    cancelSample?.Play();
                    progressSampleChannel?.Stop();
                    break;
            }
        }

        public enum StageStatusType
        {
            NotStarted,
            InProgress,
            Completed,
            Failed,
            Canceled,
        }
    }
}
