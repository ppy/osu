// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public class ProgressNotification : Notification, IHasCompletionTarget
    {
        public string Text
        {
            get { return textDrawable.Text; }
            set
            {
                textDrawable.Text = value;
            }
        }

        public float Progress
        {
            get { return progressBar.Progress; }
            set
            {
                progressBar.Progress = value;
            }
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
                            NotificationContent.MoveToY(-DrawSize.Y / 2, 200, EasingTypes.OutQuint);
                            FadeTo(0.01f, 200); //don't completely fade out or our scheduled task won't run.

                            Delay(100);
                            Schedule(Completed);
                            break;
                    }
                }
            }
        }

        private ProgressNotificationState state;

        protected virtual Notification CreateCompletionNotification() => new ProgressCompletionNotification()
        {
            Activated = CompletionClickAction,
            Text = $"Task \"{Text}\" has completed!"
        };

        protected virtual void Completed()
        {
            Expire();
            CompletionTarget?.Invoke(CreateCompletionNotification());
        }

        public override bool DisplayOnTop => false;

        private readonly ProgressBar progressBar;
        private Color4 colourQueued;
        private Color4 colourActive;
        private Color4 colourCancelled;

        private readonly SpriteText textDrawable;

        public ProgressNotification()
        {
            IconContent.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
            });

            Content.Add(textDrawable = new OsuSpriteText
            {
                TextSize = 16,
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
                    State = ProgressNotificationState.Cancelled;
                    break;
            }
        }

        /// <summary>
        /// The function to post completion notifications back to.
        /// </summary>
        public Action<Notification> CompletionTarget { get; set; }

        /// <summary>
        /// An action to complete when the completion notification is clicked.
        /// </summary>
        public Func<bool> CompletionClickAction;

        private class ProgressBar : Container
        {
            private Box box;

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
                    box.ResizeTo(new Vector2(progress, 1), 100, EasingTypes.OutQuad);
                }
            }

            private bool active;

            public bool Active
            {
                get { return active; }
                set
                {
                    active = value;
                    FadeColour(active ? colourActive : colourInactive, 100);
                }
            }


            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                colourActive = colours.Blue;
                Colour = colourInactive = OsuColour.Gray(0.5f);

                Height = 5;

                Children = new[]
                {
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0,
                    }
                };
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