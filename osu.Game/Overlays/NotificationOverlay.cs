// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Notifications;

namespace osu.Game.Overlays
{
    public class NotificationOverlay : OsuFocusedOverlayContainer
    {
        private const float width = 320;

        public const float TRANSITION_LENGTH = 600;

        private ScrollContainer scrollContainer;
        private FlowContainer<NotificationSection> sections;

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = width;
            RelativeSizeAxes = Axes.Y;

            AlwaysPresent = true;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                scrollContainer = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = Toolbar.Toolbar.HEIGHT },
                    Children = new[]
                    {
                        sections = new FillFlowContainer<NotificationSection>
                        {
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Children = new[]
                            {
                                new NotificationSection
                                {
                                    Title = @"Notifications",
                                    ClearText = @"Clear All",
                                    AcceptTypes = new[] { typeof(SimpleDrawableNotification) },
                                },
                                new NotificationSection
                                {
                                    Title = @"Running Tasks",
                                    ClearText = @"Cancel All",
                                    AcceptTypes = new[] { typeof(DrawableProgressNotification) },
                                },
                            }
                        }
                    }
                }
            };
        }

        private int runningDepth;

        private void notificationClosed()
        {
            // hide ourselves if all notifications have been dismissed.
            if (sections.Select(c => c.DisplayedCount).Sum() == 0)
                State = Visibility.Hidden;
        }

        private DrawableNotification createContainerFor(Notification notification)
        {
            var progressNotification = notification as ProgressNotification;
            if (progressNotification != null)
                return new DrawableProgressNotification(progressNotification);

            return new SimpleDrawableNotification(notification);
        }

        public void Post(Notification notification)
        {
            Schedule(() =>
            {
                DrawableNotification drawableNotification = createContainerFor(notification);

                State = Visibility.Visible;

                ++runningDepth;
                drawableNotification.Depth = drawableNotification.DisplayOnTop ? runningDepth : -runningDepth;

                drawableNotification.Closed += notificationClosed;

                var n = notification as IHasFollowUpNotifications;
                if (n != null)
                {
                    n.ProgressCompleted += () => n.FollowUpNotifications.ForEach(Post);
                }

                var ourType = drawableNotification.GetType();
                sections.Children.FirstOrDefault(s => s.AcceptTypes.Any(accept => accept.IsAssignableFrom(ourType)))?.Add(drawableNotification);
            });
        }

        protected override void PopIn()
        {
            base.PopIn();

            scrollContainer.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(1, TRANSITION_LENGTH / 2);
        }

        private void markAllRead()
        {
            sections.Children.ForEach(s => s.MarkAllRead());
        }

        protected override void PopOut()
        {
            base.PopOut();

            markAllRead();

            this.MoveToX(width, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH / 2);
        }
    }
}
