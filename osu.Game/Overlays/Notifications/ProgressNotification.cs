// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public class ProgressNotification : Notification, IHasCompletionTarget
    {
        private const float loading_spinner_size = 22;

        private LocalisableString text;

        public override LocalisableString Text
        {
            get => text;
            set
            {
                text = value;
                Schedule(() => textDrawable.Text = text);
            }
        }

        public string CompletionText { get; set; } = "Task has completed!";

        private float progress;

        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                Scheduler.AddOnce(updateProgress, progress);
            }
        }

        private void updateProgress(float progress) => progressBar.Progress = progress;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // we may have received changes before we were displayed.
            updateState();
        }

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public ProgressNotificationState State
        {
            get => state;
            set
            {
                if (state == value) return;

                state = value;

                if (IsLoaded)
                    Schedule(updateState);
            }
        }

        private void updateState()
        {
            const double colour_fade_duration = 200;

            switch (state)
            {
                case ProgressNotificationState.Queued:
                    Light.Colour = colourQueued;
                    Light.Pulsate = false;
                    progressBar.Active = false;

                    iconBackground.FadeColour(ColourInfo.GradientVertical(colourQueued, colourQueued.Lighten(0.5f)), colour_fade_duration);
                    loadingSpinner.Show();
                    break;

                case ProgressNotificationState.Active:
                    Light.Colour = colourActive;
                    Light.Pulsate = true;
                    progressBar.Active = true;

                    iconBackground.FadeColour(ColourInfo.GradientVertical(colourActive, colourActive.Lighten(0.5f)), colour_fade_duration);
                    loadingSpinner.Show();
                    break;

                case ProgressNotificationState.Cancelled:
                    cancellationTokenSource.Cancel();

                    iconBackground.FadeColour(ColourInfo.GradientVertical(Color4.Gray, Color4.Gray.Lighten(0.5f)), colour_fade_duration);
                    loadingSpinner.Hide();

                    var icon = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Ban,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(loading_spinner_size),
                    };

                    IconContent.Add(icon);

                    icon.FadeInFromZero(200, Easing.OutQuint);

                    Light.Colour = colourCancelled;
                    Light.Pulsate = false;
                    progressBar.Active = false;
                    break;

                case ProgressNotificationState.Completed:
                    loadingSpinner.Hide();
                    NotificationContent.MoveToY(-DrawSize.Y / 2, 200, Easing.OutQuint);
                    this.FadeOut(200).Finally(d => Completed());
                    break;
            }
        }

        private ProgressNotificationState state;

        protected virtual Notification CreateCompletionNotification() => new ProgressCompletionNotification
        {
            Activated = CompletionClickAction,
            Text = CompletionText
        };

        protected virtual void Completed()
        {
            CompletionTarget?.Invoke(CreateCompletionNotification());
            base.Close();
        }

        public override bool DisplayOnTop => false;

        private readonly ProgressBar progressBar;
        private Color4 colourQueued;
        private Color4 colourActive;
        private Color4 colourCancelled;

        private Box iconBackground;
        private LoadingSpinner loadingSpinner;

        private readonly TextFlowContainer textDrawable;

        public ProgressNotification()
        {
            Content.Add(textDrawable = new OsuTextFlowContainer
            {
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            NotificationContent.Add(progressBar = new ProgressBar
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
            });

            // make some extra space for the progress bar.
            IconContent.Margin = new MarginPadding { Bottom = 5 };

            State = ProgressNotificationState.Queued;

            // don't close on click by default.
            Activated = () => false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            colourQueued = colours.YellowDark;
            colourActive = colours.Blue;
            colourCancelled = colours.Red;

            IconContent.AddRange(new Drawable[]
            {
                iconBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                },
                loadingSpinner = new LoadingSpinner
                {
                    Size = new Vector2(loading_spinner_size),
                }
            });
        }

        public override void Close()
        {
            switch (State)
            {
                case ProgressNotificationState.Cancelled:
                    base.Close();
                    break;

                case ProgressNotificationState.Active:
                case ProgressNotificationState.Queued:
                    if (CancelRequested?.Invoke() != false)
                        State = ProgressNotificationState.Cancelled;
                    break;
            }
        }

        public Func<bool> CancelRequested { get; set; }

        /// <summary>
        /// The function to post completion notifications back to.
        /// </summary>
        public Action<Notification> CompletionTarget { get; set; }

        /// <summary>
        /// An action to complete when the completion notification is clicked. Return true to close.
        /// </summary>
        public Func<bool> CompletionClickAction;

        private class ProgressBar : Container
        {
            private readonly Box box;

            private Color4 colourActive;
            private Color4 colourInactive;

            private float progress;

            public float Progress
            {
                get => progress;
                set
                {
                    if (progress == value) return;

                    progress = value;
                    box.ResizeTo(new Vector2(progress, 1), 100, Easing.OutQuad);
                }
            }

            private bool active;

            public bool Active
            {
                get => active;
                set
                {
                    active = value;
                    this.FadeColour(active ? colourActive : colourInactive, 100);
                }
            }

            public ProgressBar()
            {
                Children = new[]
                {
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                colourActive = colours.Blue;
                Colour = colourInactive = OsuColour.Gray(0.5f);
                Height = 5;
            }
        }
    }

    public enum ProgressNotificationState
    {
        Queued,
        Active,
        Completed,
        Cancelled
    }
}
