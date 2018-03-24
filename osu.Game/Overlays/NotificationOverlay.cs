﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Threading;

namespace osu.Game.Overlays
{
    public class NotificationOverlay : OsuFocusedOverlayContainer
    {
        private const float width = 320;

        public const float TRANSITION_LENGTH = 600;

        /// <summary>
        /// Whether posted notifications should be processed.
        /// </summary>
        public readonly BindableBool Enabled = new BindableBool(true);

        private FlowContainer<NotificationSection> sections;

        /// <summary>
        /// Provide a source for the toolbar height.
        /// </summary>
        public Func<float> GetToolbarHeight;

        public NotificationOverlay()
        {
            ScheduledDelegate notificationsEnabler = null;
            Enabled.ValueChanged += v =>
            {
                if (!IsLoaded)
                {
                    processingPosts = v;
                    return;
                }

                notificationsEnabler?.Cancel();

                if (v)
                    // we want a slight delay before toggling notifications on to avoid the user becoming overwhelmed.
                    notificationsEnabler = Scheduler.AddDelayed(() => processingPosts = true, 1000);
                else
                    processingPosts = false;
            };
        }

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
                    Alpha = 0.6f
                },
                new OsuScrollContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
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
                                    AcceptTypes = new[] { typeof(SimpleNotification) }
                                },
                                new NotificationSection
                                {
                                    Title = @"Running Tasks",
                                    ClearText = @"Cancel All",
                                    AcceptTypes = new[] { typeof(ProgressNotification) }
                                }
                            }
                        }
                    }
                }
            };
        }

        private int totalCount => sections.Select(c => c.DisplayedCount).Sum();
        private int unreadCount => sections.Select(c => c.UnreadCount).Sum();

        public readonly BindableInt UnreadCount = new BindableInt();

        private int runningDepth;

        private void notificationClosed() => updateCounts();

        private readonly Scheduler postScheduler = new Scheduler();

        private bool processingPosts = true;

        public void Post(Notification notification) => postScheduler.Add(() =>
        {
            ++runningDepth;

            notification.Closed += notificationClosed;

            var hasCompletionTarget = notification as IHasCompletionTarget;
            if (hasCompletionTarget != null)
                hasCompletionTarget.CompletionTarget = Post;

            var ourType = notification.GetType();

            var section = sections.Children.FirstOrDefault(s => s.AcceptTypes.Any(accept => accept.IsAssignableFrom(ourType)));
            section?.Add(notification, notification.DisplayOnTop ? -runningDepth : runningDepth);

            State = Visibility.Visible;

            updateCounts();
        });

        protected override void Update()
        {
            base.Update();
            if (processingPosts)
                postScheduler.Update();
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(1, TRANSITION_LENGTH, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            markAllRead();

            this.MoveToX(width, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH, Easing.OutQuint);
        }

        private void updateCounts()
        {
            UnreadCount.Value = unreadCount;
        }

        private void markAllRead()
        {
            sections.Children.ForEach(s => s.MarkAllRead());

            updateCounts();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Padding = new MarginPadding { Top = GetToolbarHeight?.Invoke() ?? 0 };
        }
    }
}
