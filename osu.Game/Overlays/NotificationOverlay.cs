// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays
{
    public class NotificationOverlay : OsuFocusedOverlayContainer, INamedOverlayComponent
    {
        public string IconTexture => "Icons/Hexacons/notification";
        public LocalisableString Title => NotificationsStrings.HeaderTitle;
        public LocalisableString Description => NotificationsStrings.HeaderDescription;

        public const float WIDTH = 320;

        public const float TRANSITION_LENGTH = 600;

        private FlowContainer<NotificationSection> sections;

        [Resolved]
        private AudioManager audio { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            X = WIDTH;
            Width = WIDTH;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.05f),
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
                                new NotificationSection(@"Notifications", @"Clear All")
                                {
                                    AcceptTypes = new[] { typeof(SimpleNotification) }
                                },
                                new NotificationSection(@"Running Tasks", @"Cancel All")
                                {
                                    AcceptTypes = new[] { typeof(ProgressNotification) }
                                }
                            }
                        }
                    }
                }
            };
        }

        private ScheduledDelegate notificationsEnabler;

        private void updateProcessingMode()
        {
            bool enabled = OverlayActivationMode.Value == OverlayActivation.All || State.Value == Visibility.Visible;

            notificationsEnabler?.Cancel();

            if (enabled)
                // we want a slight delay before toggling notifications on to avoid the user becoming overwhelmed.
                notificationsEnabler = Scheduler.AddDelayed(() => processingPosts = true, State.Value == Visibility.Visible ? 0 : 1000);
            else
                processingPosts = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.ValueChanged += _ => updateProcessingMode();
            OverlayActivationMode.BindValueChanged(_ => updateProcessingMode(), true);
        }

        public readonly BindableInt UnreadCount = new BindableInt();

        private int runningDepth;

        private readonly Scheduler postScheduler = new Scheduler();

        public override bool IsPresent => base.IsPresent || postScheduler.HasPendingTasks;

        private bool processingPosts = true;

        private double? lastSamplePlayback;

        /// <summary>
        /// Post a new notification for display.
        /// </summary>
        /// <param name="notification">The notification to display.</param>
        public void Post(Notification notification) => postScheduler.Add(() =>
        {
            ++runningDepth;

            Logger.Log($"⚠️ {notification.Text}");

            notification.Closed += notificationClosed;

            if (notification is IHasCompletionTarget hasCompletionTarget)
                hasCompletionTarget.CompletionTarget = Post;

            var ourType = notification.GetType();

            var section = sections.Children.FirstOrDefault(s => s.AcceptTypes.Any(accept => accept.IsAssignableFrom(ourType)));
            section?.Add(notification, notification.DisplayOnTop ? -runningDepth : runningDepth);

            if (notification.IsImportant)
                Show();

            updateCounts();
            playDebouncedSample(notification.PopInSampleName);
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

            this.MoveToX(WIDTH, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH, Easing.OutQuint);
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

        private void updateCounts()
        {
            UnreadCount.Value = sections.Select(c => c.UnreadCount).Sum();
        }

        private void markAllRead()
        {
            sections.Children.ForEach(s => s.MarkAllRead());

            updateCounts();
        }
    }
}
