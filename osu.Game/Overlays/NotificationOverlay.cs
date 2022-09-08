// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using NotificationsStrings = osu.Game.Localisation.NotificationsStrings;

namespace osu.Game.Overlays
{
    public class NotificationOverlay : OsuFocusedOverlayContainer, INamedOverlayComponent, INotificationOverlay
    {
        public string IconTexture => "Icons/Hexacons/notification";
        public LocalisableString Title => NotificationsStrings.HeaderTitle;
        public LocalisableString Description => NotificationsStrings.HeaderDescription;

        public const float WIDTH = 320;

        public const float TRANSITION_LENGTH = 600;

        private FlowContainer<NotificationSection> sections = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            if (State.Value == Visibility.Visible)
                return base.ReceivePositionalInputAt(screenSpacePos);

            if (toastTray.IsDisplayingToasts)
                return toastTray.ReceivePositionalInputAt(screenSpacePos);

            return false;
        }

        public override bool PropagatePositionalInputSubTree => base.PropagatePositionalInputSubTree || toastTray.IsDisplayingToasts;

        private NotificationOverlayToastTray toastTray = null!;

        private Container mainContent = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            X = WIDTH;
            Width = WIDTH;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                toastTray = new NotificationOverlayToastTray
                {
                    ForwardNotificationToPermanentStore = addPermanently,
                    Origin = Anchor.TopRight,
                },
                mainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
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
                                        new NotificationSection(AccountsStrings.NotificationsTitle, new[] { typeof(SimpleNotification) }, "Clear All"),
                                        new NotificationSection(@"Running Tasks", new[] { typeof(ProgressNotification) }, @"Cancel All"),
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }

        private ScheduledDelegate? notificationsEnabler;

        private void updateProcessingMode()
        {
            bool enabled = OverlayActivationMode.Value == OverlayActivation.All || State.Value == Visibility.Visible;

            notificationsEnabler?.Cancel();

            if (enabled)
                // we want a slight delay before toggling notifications on to avoid the user becoming overwhelmed.
                notificationsEnabler = Scheduler.AddDelayed(() => processingPosts = true, State.Value == Visibility.Visible ? 0 : 100);
            else
                processingPosts = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.BindValueChanged(_ => updateProcessingMode());
            OverlayActivationMode.BindValueChanged(_ => updateProcessingMode(), true);
        }

        public IBindable<int> UnreadCount => unreadCount;

        public int ToastCount => toastTray.UnreadCount;

        private readonly BindableInt unreadCount = new BindableInt();

        private int runningDepth;

        private readonly Scheduler postScheduler = new Scheduler();

        public override bool IsPresent =>
            // Delegate presence as we need to consider the toast tray in addition to the main overlay.
            State.Value == Visibility.Visible || mainContent.IsPresent || toastTray.IsPresent || postScheduler.HasPendingTasks;

        private bool processingPosts = true;

        private double? lastSamplePlayback;

        public void Post(Notification notification) => postScheduler.Add(() =>
        {
            ++runningDepth;

            Logger.Log($"⚠️ {notification.Text}");

            notification.Closed += notificationClosed;

            if (notification is IHasCompletionTarget hasCompletionTarget)
                hasCompletionTarget.CompletionTarget = Post;

            playDebouncedSample(notification.PopInSampleName);

            if (State.Value == Visibility.Hidden)
                toastTray.Post(notification);
            else
                addPermanently(notification);

            updateCounts();
        });

        private void addPermanently(Notification notification)
        {
            var ourType = notification.GetType();
            int depth = notification.DisplayOnTop ? -runningDepth : runningDepth;

            var section = sections.Children.First(s => s.AcceptedNotificationTypes.Any(accept => accept.IsAssignableFrom(ourType)));

            section.Add(notification, depth);

            updateCounts();
        }

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
            mainContent.FadeTo(1, TRANSITION_LENGTH, Easing.OutQuint);

            toastTray.FlushAllToasts();
        }

        protected override void PopOut()
        {
            base.PopOut();

            markAllRead();

            this.MoveToX(WIDTH, TRANSITION_LENGTH, Easing.OutQuint);
            mainContent.FadeTo(0, TRANSITION_LENGTH, Easing.OutQuint);
        }

        private void notificationClosed()
        {
            updateCounts();

            // this debounce is currently shared between popin/popout sounds, which means one could potentially not play when the user is expecting it.
            // popout is constant across all notification types, and should therefore be handled using playback concurrency instead, but seems broken at the moment.
            playDebouncedSample("UI/overlay-pop-out");
        }

        private void playDebouncedSample(string sampleName)
        {
            if (lastSamplePlayback == null || Time.Current - lastSamplePlayback > OsuGameBase.SAMPLE_DEBOUNCE_TIME)
            {
                audio.Samples.Get(sampleName)?.Play();
                lastSamplePlayback = Time.Current;
            }
        }

        private void markAllRead()
        {
            sections.Children.ForEach(s => s.MarkAllRead());
            toastTray.MarkAllRead();
            updateCounts();
        }

        private void updateCounts()
        {
            unreadCount.Value = sections.Select(c => c.UnreadCount).Sum() + toastTray.UnreadCount;
        }
    }
}
