// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Notifications;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public class ProgressNotificationContainer : NotificationContainer, IHasNotification
    {
        public ProgressNotification ProgressNotification { get; }
        public Notification Notification => ProgressNotification;

        public override bool DisplayOnTop => false;

        private readonly ProgressBar progressBar;
        private Color4 colourQueued;
        private Color4 colourActive;
        private Color4 colourCancelled;

        private readonly TextFlowContainer textDrawable;
        private readonly SpriteIcon iconDrawable;

        protected Box IconBackgound;

        public ProgressNotificationContainer(ProgressNotification progressNotification)
        {
            ProgressNotification = progressNotification;
            ProgressNotification.StateBinding.ValueChanged += stateOnValueChanged;

            ProgressNotification.ProgressBinding.ValueChanged += value => Schedule(() => progressBar.Progress = value);
            ProgressNotification.IconBinding.ValueChanged += value => Schedule(() => iconDrawable.Icon = value);
            ProgressNotification.TextBinding.ValueChanged += value => Schedule(() => textDrawable.Text = value);

            IconContent.AddRange(new Drawable[]
            {
                IconBackgound = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.6f))
                },
                iconDrawable = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = ProgressNotification.Icon,
                    Size = new Vector2(20),
                }
            });

            Content.Add(textDrawable = new TextFlowContainer(t =>
            {
                t.TextSize = 16;

            })
            {
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

            Schedule(() => textDrawable.Text = ProgressNotification.Text);

            NotificationContent.Add(progressBar = new ProgressBar
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Progress = 0
            });

            // don't close on click by default.
            Activated = () => false;
        }

        private void stateOnValueChanged(ProgressNotificationState newValue)
        {
            if (IsLoaded)
            {
                switch (newValue)
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
                    case ProgressNotificationState.Completed:
                        NotificationContent.MoveToY(-DrawSize.Y / 2, 200, Easing.OutQuint);
                        this.FadeOut(200).Finally(d => Expire());
                        break;
                }
            }
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
            switch (ProgressNotification.State)
            {
                case ProgressNotificationState.Cancelled:
                    base.Close();
                    break;
                case ProgressNotificationState.Active:
                case ProgressNotificationState.Queued:
                    ProgressNotification.RequestCancel();
                    break;
            }
        }

        public Func<bool> CancelRequested { get; set; }


        /// <summary>
        /// An action to complete when the completion notificationContainer is clicked.
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


}
