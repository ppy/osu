// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
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

        private Bindable<StageStatusType> status { get; } = new Bindable<StageStatusType>();

        private Bindable<float?> progress { get; } = new Bindable<float?>();

        private Container progressBarContainer = null!;
        private Box progressBar = null!;
        private Container iconContainer = null!;
        private OsuTextFlowContainer errorMessage = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            status.BindValueChanged(_ => Scheduler.AddOnce(updateStatus), true);
            progress.BindValueChanged(_ => Scheduler.AddOnce(updateProgress), true);
        }

        public void SetNotStarted() => status.Value = StageStatusType.NotStarted;

        public void SetInProgress(float? progress = null)
        {
            this.progress.Value = progress;
            status.Value = StageStatusType.InProgress;
        }

        public void SetCompleted() => status.Value = StageStatusType.Completed;

        public void SetFailed(string reason)
        {
            status.Value = StageStatusType.Failed;
            errorMessage.Text = reason;
        }

        public void SetCanceled() => status.Value = StageStatusType.Canceled;

        private const float transition_duration = 200;

        private void updateProgress()
        {
            if (progress.Value != null)
                progressBar.ResizeWidthTo(progress.Value.Value, transition_duration, Easing.OutQuint);

            progressBarContainer.FadeTo(status.Value == StageStatusType.InProgress && progress.Value != null ? 1 : 0, transition_duration, Easing.OutQuint);
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
                    break;

                case StageStatusType.Failed:
                    iconContainer.Child = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.ExclamationCircle,
                        Size = new Vector2(16),
                    };
                    iconContainer.Colour = colours.Red1;
                    iconContainer.FlashColour(Colour4.White, 1000, Easing.OutQuint);
                    break;

                case StageStatusType.Canceled:
                    iconContainer.Child = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Ban,
                        Size = new Vector2(16),
                    };
                    iconContainer.Colour = colours.Gray8;
                    iconContainer.FlashColour(Colour4.White, 1000, Easing.OutQuint);
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
