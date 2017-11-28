// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Notifications;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public class DrawableProgressNotification : DrawableNotification
    {
        private readonly ProgressNotification progressNotification;

        public override bool DisplayOnTop => false;

        private readonly ProgressBar progressBar;
        private Color4 colourQueued;
        private Color4 colourActive;
        private Color4 colourCancelled;

        private readonly TextFlowContainer textDrawable;
        private readonly SpriteIcon iconDrawable;

        private readonly Box iconBackgound;

        public DrawableProgressNotification(ProgressNotification progressNotification)
        {
            if (progressNotification == null)
                throw new ArgumentNullException(nameof(progressNotification));

            this.progressNotification = progressNotification;
            progressNotification.StateBinding.ValueChanged += stateChanged;
            progressNotification.ProgressBinding.ValueChanged += value => Schedule(() => progressBar.Progress = value);
            progressNotification.BackgroundColourBinding.ValueChanged += value => Schedule(() => Colour = value);
            progressNotification.TextBinding.ValueChanged += value => Schedule(() => textDrawable.Text = value);
            progressNotification.NotificationIconBinding.ValueChanged += value => Schedule(() =>
            {
                iconDrawable.Icon = value.Icon;
                iconBackgound.Colour = value.BackgroundColour;
            });

            IconContent.AddRange(new Drawable[]
            {
                iconBackgound = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = this.progressNotification.NotificationIcon.BackgroundColour
                },
                iconDrawable = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = this.progressNotification.NotificationIcon.Icon,
                    Size = new Vector2(20),
                }
            });
            Content.Add(textDrawable = new TextFlowContainer(t =>
            {
                t.TextSize = 16;
            })
            {
                Text = progressNotification.Text,
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            });

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

        private void stateChanged(ProgressNotificationState newValue)
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
            switch (progressNotification.State)
            {
                case ProgressNotificationState.Cancelled:
                    base.Close();
                    break;
                case ProgressNotificationState.Active:
                case ProgressNotificationState.Queued:
                    progressNotification.RequestCancel();
                    break;
            }
        }

        private class ProgressBar : Container
        {
            private readonly Box box;

            private Color4 colourActive;
            private Color4 colourInactive;

            private float progress;
            public float Progress
            {
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
