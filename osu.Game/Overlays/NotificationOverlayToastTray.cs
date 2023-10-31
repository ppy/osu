// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Overlays.Notifications;
using osuTK;

namespace osu.Game.Overlays
{
    /// <summary>
    /// A tray which attaches to the left of <see cref="NotificationOverlay"/> to show temporary toasts.
    /// </summary>
    public partial class NotificationOverlayToastTray : CompositeDrawable
    {
        public override bool IsPresent => toastContentBackground.Height > 0 || toastFlow.Count > 0;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => toastFlow.ReceivePositionalInputAt(screenSpacePos);

        /// <summary>
        /// All notifications currently being displayed by the toast tray.
        /// </summary>
        public IEnumerable<Notification> Notifications => toastFlow;

        public bool IsDisplayingToasts => toastFlow.Count > 0;

        private FillFlowContainer<Notification> toastFlow = null!;
        private BufferedContainer toastContentBackground = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public Action<Notification>? ForwardNotificationToPermanentStore { get; set; }

        public int UnreadCount => allDisplayedNotifications.Count(n => !n.WasClosed && !n.Read);

        /// <summary>
        /// Notifications contained in the toast flow, or in a detached state while they animate during forwarding to the main overlay.
        /// </summary>
        private IEnumerable<Notification> allDisplayedNotifications => toastFlow.Concat(InternalChildren.OfType<Notification>());

        private int runningDepth;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding(20);

            InternalChildren = new Drawable[]
            {
                toastContentBackground = (new Box
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = ColourInfo.GradientVertical(
                        colourProvider.Background6.Opacity(0.7f),
                        colourProvider.Background6.Opacity(0.5f)),
                    RelativeSizeAxes = Axes.Both,
                    Height = 0,
                }.WithEffect(new BlurEffect
                {
                    PadExtent = true,
                    Sigma = new Vector2(20),
                }).With(postEffectDrawable =>
                {
                    postEffectDrawable.Scale = new Vector2(1.5f, 1);
                    postEffectDrawable.Position += new Vector2(70, -50);
                    postEffectDrawable.AutoSizeAxes = Axes.None;
                    postEffectDrawable.RelativeSizeAxes = Axes.X;
                })),
                toastFlow = new FillFlowContainer<Notification>
                {
                    LayoutDuration = 150,
                    LayoutEasing = Easing.OutQuart,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        }

        public void MarkAllRead()
        {
            toastFlow.Children.ForEach(n => n.Read = true);
            InternalChildren.OfType<Notification>().ForEach(n => n.Read = true);
        }

        public void FlushAllToasts()
        {
            foreach (var notification in toastFlow.ToArray())
                forwardNotification(notification);
        }

        public void Post(Notification notification)
        {
            ++runningDepth;

            notification.ForwardToOverlay = () => forwardNotification(notification);

            int depth = notification.DisplayOnTop ? -runningDepth : runningDepth;

            toastFlow.Insert(depth, notification);

            scheduleDismissal();

            void scheduleDismissal() => Scheduler.AddDelayed(() =>
            {
                // Notification dismissed by user.
                if (notification.WasClosed)
                    return;

                // Notification forwarded away.
                if (notification.Parent != toastFlow)
                    return;

                // Notification hovered; delay dismissal.
                if (notification.IsHovered || notification.IsDragged)
                {
                    scheduleDismissal();
                    return;
                }

                // All looks good, forward away!
                forwardNotification(notification);
            }, notification.IsImportant ? 12000 : 2500);
        }

        private void forwardNotification(Notification notification)
        {
            if (!notification.IsInToastTray)
                return;

            Debug.Assert(notification.Parent == toastFlow);

            // Temporarily remove from flow so we can animate the position off to the right.
            toastFlow.Remove(notification, false);
            AddInternal(notification);

            notification.MoveToOffset(new Vector2(400, 0), NotificationOverlay.TRANSITION_LENGTH, Easing.OutQuint);
            notification.FadeOut(NotificationOverlay.TRANSITION_LENGTH, Easing.OutQuint).OnComplete(_ =>
            {
                RemoveInternal(notification, false);
                ForwardNotificationToPermanentStore?.Invoke(notification);

                notification.FadeIn(300, Easing.OutQuint);
            });
        }

        protected override void Update()
        {
            base.Update();

            float height = toastFlow.Count > 0 ? toastFlow.DrawHeight + 120 : 0;
            float alpha = toastFlow.Count > 0 ? MathHelper.Clamp(toastFlow.DrawHeight / 41, 0, 1) * toastFlow.Children.Max(n => n.Alpha) : 0;

            toastContentBackground.Height = (float)Interpolation.DampContinuously(toastContentBackground.Height, height, 10, Clock.ElapsedFrameTime);
            toastContentBackground.Alpha = (float)Interpolation.DampContinuously(toastContentBackground.Alpha, alpha, 10, Clock.ElapsedFrameTime);
        }
    }
}
