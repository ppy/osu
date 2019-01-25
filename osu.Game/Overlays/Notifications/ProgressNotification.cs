﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public class ProgressNotification : Notification, IHasCompletionTarget
    {
        public string Text
        {
            set
            {
                Schedule(() => textDrawable.Text = value);
            }
        }

        public string CompletionText { get; set; } = "Task has completed!";

        public float Progress
        {
            get { return progressBar.Progress; }
            set { Schedule(() => progressBar.Progress = value); }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //we may have received changes before we were displayed.
            State = state;
        }

        public virtual ProgressNotificationState State
        {
            get { return state; }
            set
            {
                Schedule(() =>
                {
                    bool stateChanged = state != value;
                    state = value;

                    if (IsLoaded)
                    {
                        switch (state)
                        {
                            case ProgressNotificationState.Queued:
                                Light.Colour = colourQueued;
                                Light.Pulsate = false;
                                progressBar.Active = false;
                                break;
                            case ProgressNotificationState.Active:
                                Light.Colour = colourActive;
                                Light.Pulsate = true;
                                progressBar.Active = true;
                                break;
                            case ProgressNotificationState.Cancelled:
                                Light.Colour = colourCancelled;
                                Light.Pulsate = false;
                                progressBar.Active = false;
                                break;
                        }
                    }

                    if (stateChanged)
                    {
                        switch (state)
                        {
                            case ProgressNotificationState.Completed:
                                NotificationContent.MoveToY(-DrawSize.Y / 2, 200, Easing.OutQuint);
                                this.FadeOut(200).Finally(d => Completed());
                                break;
                        }
                    }
                });
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

        private readonly TextFlowContainer textDrawable;

        public ProgressNotification()
        {
            IconContent.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
            });

            Content.Add(textDrawable = new OsuTextFlowContainer(t =>
            {
                t.TextSize = 16;
            })
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
                get { return progress; }
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
                get { return active; }
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
