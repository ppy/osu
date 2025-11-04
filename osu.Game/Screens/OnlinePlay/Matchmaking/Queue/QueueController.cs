// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.OnlinePlay.Matchmaking.Intro;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    /// <summary>
    /// A component which acts as a bridge between the online component (ie <see cref="MultiplayerClient"/>)
    /// and the visual representations and flow of queueing for matchmaking.
    ///
    /// Includes support for deferring to background.
    /// </summary>
    /// <remarks>
    /// This is initialised and cached in the <see cref="ScreenQueue"/> but can be used throughout the system via DI.</remarks>
    public partial class QueueController : Component
    {
        public readonly Bindable<ScreenQueue.MatchmakingScreenState> CurrentState = new Bindable<ScreenQueue.MatchmakingScreenState>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private BackgroundQueueNotification? backgroundNotification;
        private bool isBackgrounded;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
        }

        public void SearchInBackground()
        {
            if (isBackgrounded)
                return;

            isBackgrounded = true;
            postNotification();
        }

        public void SearchInForeground()
        {
            if (!isBackgrounded)
                return;

            isBackgrounded = false;
            closeNotifications();
        }

        private void onRoomUpdated() => Scheduler.Add(() =>
        {
            if (client.Room == null)
                CurrentState.Value = ScreenQueue.MatchmakingScreenState.Idle;
        });

        private void onMatchmakingQueueJoined() => Scheduler.Add(() =>
        {
            CurrentState.Value = ScreenQueue.MatchmakingScreenState.Queueing;

            if (isBackgrounded)
            {
                closeNotifications();
                postNotification();
            }
        });

        private void onMatchmakingQueueLeft() => Scheduler.Add(() =>
        {
            if (CurrentState.Value != ScreenQueue.MatchmakingScreenState.InRoom)
                CurrentState.Value = ScreenQueue.MatchmakingScreenState.Idle;

            closeNotifications();
        });

        private void onMatchmakingRoomInvited() => Scheduler.Add(() =>
        {
            CurrentState.Value = ScreenQueue.MatchmakingScreenState.PendingAccept;

            if (backgroundNotification != null)
            {
                backgroundNotification.State = ProgressNotificationState.Completed;
                backgroundNotification = null;
            }
        });

        private void onMatchmakingRoomReady(long roomId, string password) => Scheduler.Add(() =>
        {
            client.JoinRoom(new Room { RoomID = roomId }, password)
                  .FireAndForget(() => Scheduler.Add(() =>
                  {
                      CurrentState.Value = ScreenQueue.MatchmakingScreenState.InRoom;
                  }));
        });

        private void postNotification()
        {
            if (backgroundNotification != null)
                return;

            notifications?.Post(backgroundNotification = new BackgroundQueueNotification(this));
        }

        private void closeNotifications()
        {
            if (backgroundNotification != null)
            {
                backgroundNotification.State = ProgressNotificationState.Cancelled;
                backgroundNotification.CloseAll();
                backgroundNotification = null;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.MatchmakingQueueJoined -= onMatchmakingQueueJoined;
                client.MatchmakingQueueLeft -= onMatchmakingQueueLeft;
                client.MatchmakingRoomInvited -= onMatchmakingRoomInvited;
                client.MatchmakingRoomReady -= onMatchmakingRoomReady;
            }
        }

        private partial class BackgroundQueueNotification : ProgressNotification
        {
            [Resolved]
            private IPerformFromScreenRunner? performer { get; set; }

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private readonly QueueController controller;

            private Notification? foundNotification;
            private Sample? matchFoundSample;

            public BackgroundQueueNotification(QueueController controller)
            {
                this.controller = controller;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                Text = "Searching for opponents...";

                CompletionClickAction = () =>
                {
                    client.MatchmakingAcceptInvitation().FireAndForget();
                    controller.CurrentState.Value = ScreenQueue.MatchmakingScreenState.AcceptedWaitingForRoom;

                    performer?.PerformFromScreen(s => s.Push(new ScreenIntro()));

                    Close(false);
                    return true;
                };

                CancelRequested = () =>
                {
                    client.MatchmakingLeaveQueue().FireAndForget();
                    return true;
                };

                matchFoundSample = audio.Samples.Get(@"Multiplayer/Matchmaking/match-found");
            }

            protected override Notification CreateCompletionNotification()
            {
                // Playing here means it will play even if notification overlay is hidden.
                //
                // If we add support for the completion notification to be processed during gameplay,
                // this can be moved inside the `MatchFoundNotification` implementation.
                matchFoundSample?.Play();

                return foundNotification = new MatchFoundNotification
                {
                    Activated = CompletionClickAction,
                    Text = "Your match is ready! Click to join.",
                };
            }

            public void CloseAll()
            {
                foundNotification?.Close(false);
                Close(false);
            }

            public partial class MatchFoundNotification : ProgressCompletionNotification
            {
                protected override IconUsage CloseButtonIcon => FontAwesome.Solid.Times;

                public MatchFoundNotification()
                {
                    IsCritical = true;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Icon = FontAwesome.Solid.Bolt;
                    IconContent.Colour = ColourInfo.GradientVertical(colours.YellowDark, colours.YellowLight);
                }
            }
        }
    }
}
