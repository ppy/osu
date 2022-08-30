// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Overlays.Notifications;
using osuTK;

namespace osu.Game.Overlays
{
    /// <summary>
    /// A tray which attaches to the left of <see cref="NotificationOverlay"/> to show temporary toasts.
    /// </summary>
    public class NotificationOverlayToastTray : CompositeDrawable
    {
        public bool IsDisplayingToasts => toastFlow.Count > 0;

        private FillFlowContainer toastFlow = null!;
        private BufferedContainer toastContentBackground = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly List<ScheduledDelegate> pendingToastOperations = new List<ScheduledDelegate>();

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
                toastFlow = new FillFlowContainer
                {
                    LayoutDuration = 150,
                    LayoutEasing = Easing.OutQuart,
                    Spacing = new Vector2(3),
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        }

        public void FlushAllToasts()
        {
            foreach (var d in pendingToastOperations.Where(d => !d.Completed))
                d.RunTask();

            pendingToastOperations.Clear();
        }

        public void Post(Notification notification, Action addPermanently)
        {
            ++runningDepth;

            int depth = notification.DisplayOnTop ? -runningDepth : runningDepth;

            toastFlow.Insert(depth, notification);

            pendingToastOperations.Add(scheduleDismissal());

            ScheduledDelegate scheduleDismissal() => Scheduler.AddDelayed(() =>
            {
                // add notification to permanent overlay unless it was already dismissed by the user.
                if (notification.WasClosed)
                    return;

                if (notification.IsHovered)
                {
                    pendingToastOperations.Add(scheduleDismissal());
                    return;
                }

                toastFlow.Remove(notification);
                AddInternal(notification);

                notification.MoveToOffset(new Vector2(400, 0), NotificationOverlay.TRANSITION_LENGTH, Easing.OutQuint);
                notification.FadeOut(NotificationOverlay.TRANSITION_LENGTH, Easing.OutQuint).OnComplete(_ =>
                {
                    RemoveInternal(notification);
                    addPermanently();

                    notification.FadeIn(300, Easing.OutQuint);
                });
            }, notification.IsImportant ? 12000 : 2500);
        }

        protected override void Update()
        {
            base.Update();

            float height = toastFlow.DrawHeight + 120;
            float alpha = IsDisplayingToasts ? MathHelper.Clamp(toastFlow.DrawHeight / 40, 0, 1) * toastFlow.Children.Max(n => n.Alpha) : 0;

            toastContentBackground.Height = (float)Interpolation.DampContinuously(toastContentBackground.Height, height, 10, Clock.ElapsedFrameTime);
            toastContentBackground.Alpha = (float)Interpolation.DampContinuously(toastContentBackground.Alpha, alpha, 10, Clock.ElapsedFrameTime);
        }
    }
}
