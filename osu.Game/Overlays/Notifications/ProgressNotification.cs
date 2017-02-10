// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public class ProgressNotification : Notification, IHasCompletionTarget
    {
        private string text;

        private float progress;
        public float Progress
        {
            get { return progress; }
            set
            {
                Debug.Assert(state == ProgressNotificationState.Active);
                progress = value;
                progressBar.Progress = progress;
            }
        }

        public ProgressNotificationState State
        {
            get { return state; }
            set
            {
                state = value;
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
        }

        private ProgressNotificationState state;

        public Action Completed;

        public override bool DisplayOnTop => false;

        private ProgressBar progressBar;
        private Color4 colourQueued;
        private Color4 colourActive;
        private Color4 colourCancelled;

        public ProgressNotification(string text)
        {
            this.text = text;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            colourQueued = colours.YellowDark;
            colourActive = colours.Blue;
            colourCancelled = colours.Red;

            IconContent.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
            });

            Content.Add(new SpriteText
            {
                TextSize = 16,
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Text = text
            });

            NotificationContent.Add(progressBar = new ProgressBar
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
            });

            State = ProgressNotificationState.Queued;
        }

        public void Complete()
        {
            state = ProgressNotificationState.Completed;

            NotificationContent.MoveToY(-DrawSize.Y / 2, 200, EasingTypes.OutQuint);
            FadeTo(0.01f, 200); //don't completely fade out or our scheduled task won't run.

            Delay(100);
            Schedule(() =>
            {
                CompletionTarget?.Invoke(new ProgressCompletionNotification(this));
                base.Close();
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
                    State = ProgressNotificationState.Cancelled;
                    break;
            }
        }

        public Action<Notification> CompletionTarget { get; set; }

        class ProgressBar : Container
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