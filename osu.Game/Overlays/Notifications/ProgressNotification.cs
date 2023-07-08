// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
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
    public partial class ProgressNotification : Notification, IHasCompletionTarget
    {
        private const float loading_spinner_size = 22;

        public Func<bool>? CancelRequested { get; set; }

        /// <summary>
        /// Whether the operation represented by the <see cref="ProgressNotification"/> is still ongoing.
        /// </summary>
        public bool Ongoing => State != ProgressNotificationState.Completed && State != ProgressNotificationState.Cancelled;

        protected override bool AllowFlingDismiss => false;

        public override string PopOutSampleName => State is ProgressNotificationState.Cancelled ? base.PopOutSampleName : "";

        /// <summary>
        /// The function to post completion notifications back to.
        /// </summary>
        public Action<Notification>? CompletionTarget { get; set; }

        /// <summary>
        /// An action to complete when the completion notification is clicked. Return true to close.
        /// </summary>
        public Func<bool>? CompletionClickAction { get; set; }

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

        public LocalisableString CompletionText { get; set; } = "Task has completed!";

        private float progress;

        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                Scheduler.AddOnce(p => progressBar.Progress = p, progress);
            }
        }

        protected override IconUsage CloseButtonIcon => FontAwesome.Solid.Times;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // we may have received changes before we were displayed.
            Scheduler.AddOnce(updateState);
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

                Scheduler.AddOnce(updateState);
                attemptPostCompletion();
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

                    IconContent.FadeColour(ColourInfo.GradientVertical(colourQueued, colourQueued.Lighten(0.5f)), colour_fade_duration);
                    loadingSpinner.Show();
                    break;

                case ProgressNotificationState.Active:
                    Light.Colour = colourActive;
                    Light.Pulsate = true;
                    progressBar.Active = true;

                    IconContent.FadeColour(ColourInfo.GradientVertical(colourActive, colourActive.Lighten(0.5f)), colour_fade_duration);
                    loadingSpinner.Show();
                    break;

                case ProgressNotificationState.Cancelled:
                    cancellationTokenSource.Cancel();

                    IconContent.FadeColour(ColourInfo.GradientVertical(Color4.Gray, Color4.Gray.Lighten(0.5f)), colour_fade_duration);
                    cancelSample?.Play();
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
                    attemptPostCompletion();
                    break;
            }
        }

        private int completionSent;

        /// <summary>
        /// Attempt to post a completion notification.
        /// </summary>
        private void attemptPostCompletion()
        {
            if (state != ProgressNotificationState.Completed) return;

            // This notification may not have been posted yet (and thus may not have a target to post the completion to).
            // Completion posting will be re-attempted in a scheduled invocation.
            if (CompletionTarget == null)
                return;

            // Thread-safe barrier, as this may be called by a web request and also scheduled to the update thread at the same time.
            if (Interlocked.Exchange(ref completionSent, 1) == 1)
                return;

            CompletionTarget.Invoke(CreateCompletionNotification());

            Close(false);
        }

        private ProgressNotificationState state;

        protected virtual Notification CreateCompletionNotification() => new ProgressCompletionNotification
        {
            Activated = CompletionClickAction,
            Text = CompletionText
        };

        public override bool DisplayOnTop => false;

        public override bool IsImportant => false;

        private readonly ProgressBar progressBar;
        private Color4 colourQueued;
        private Color4 colourActive;
        private Color4 colourCancelled;

        private LoadingSpinner loadingSpinner = null!;

        private Sample? cancelSample;

        private readonly TextFlowContainer textDrawable;

        public ProgressNotification()
        {
            Content.Add(textDrawable = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 14, weight: FontWeight.Medium))
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            MainContent.Add(progressBar = new ProgressBar
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
        private void load(OsuColour colours, AudioManager audioManager)
        {
            colourQueued = colours.YellowDark;
            colourActive = colours.Blue;
            colourCancelled = colours.Red;

            IconContent.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                    Depth = float.MaxValue,
                },
                loadingSpinner = new LoadingSpinner
                {
                    Size = new Vector2(loading_spinner_size),
                }
            });

            cancelSample = audioManager.Samples.Get(@"UI/notification-cancel");
        }

        public override void Close(bool runFlingAnimation)
        {
            switch (State)
            {
                case ProgressNotificationState.Completed:
                case ProgressNotificationState.Cancelled:
                    base.Close(runFlingAnimation);
                    break;

                case ProgressNotificationState.Active:
                case ProgressNotificationState.Queued:
                    if (CancelRequested?.Invoke() != false)
                        State = ProgressNotificationState.Cancelled;
                    break;
            }
        }

        private partial class ProgressBar : Container
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
