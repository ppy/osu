// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
using osu.Game.Localisation;
using osu.Game.Online.Matchmaking;
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
    public partial class QueueController : Component
    {
        public readonly Bindable<ScreenQueue.MatchmakingScreenState> CurrentState = new Bindable<ScreenQueue.MatchmakingScreenState>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private BackgroundQueueNotification? backgroundNotification;
        private bool isBackgrounded;
        public MatchmakingPool? LastJoinedPool { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
        }

        /// <summary>
        /// Joins the matchmaking queue.
        /// </summary>
        /// <param name="pool">The pool to join.</param>
        public void JoinQueue(MatchmakingPool pool)
        {
            client.MatchmakingJoinQueue(pool.Id).FireAndForget();
            LastJoinedPool = pool;
        }

        /// <summary>
        /// Leaves the matchmaking queue.
        /// </summary>
        public void LeaveQueue()
        {
            client.MatchmakingLeaveQueue().FireAndForget();
        }

        /// <summary>
        /// Rejoins the last joined matchmaking queue.
        /// </summary>
        public void RejoinQueue()
        {
            if (LastJoinedPool != null)
                JoinQueue(LastJoinedPool);
        }

        /// <summary>
        /// Moves the matchmaking queue search to the background.
        /// </summary>
        public void SearchInBackground()
        {
            if (isBackgrounded)
                return;

            isBackgrounded = true;
            postNotification();
        }

        /// <summary>
        /// Moves the matchmaking queue search to the foreground.
        /// </summary>
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

        private void onMatchmakingRoomInvited(MatchmakingRoomInvitationParams invitation) => Scheduler.Add(() =>
        {
            CurrentState.Value = ScreenQueue.MatchmakingScreenState.PendingAccept;

            backgroundNotification?.Complete(invitation);
            backgroundNotification = null;
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

            Debug.Assert(LastJoinedPool != null);
            notifications?.Post(backgroundNotification = new BackgroundQueueNotification(this, LastJoinedPool.Type));
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
            private readonly MatchmakingPoolType poolType;

            private Notification? foundNotification;
            private Sample? matchFoundSample;

            public BackgroundQueueNotification(QueueController controller, MatchmakingPoolType poolType)
            {
                this.controller = controller;
                this.poolType = poolType;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                Text = MultiplayerMatchStrings.SearchingForOpponents;

                Activated = () =>
                {
                    performer?.PerformFromScreen(s =>
                    {
                        if (s is ScreenIntro || s is ScreenQueue)
                            return;

                        s.Push(new ScreenIntro(poolType));
                    }, [typeof(ScreenIntro), typeof(ScreenQueue)]);

                    // Closed when appropriate by SearchInForeground().
                    return false;
                };

                CancelRequested = () =>
                {
                    client.MatchmakingLeaveQueue().FireAndForget();
                    return true;
                };

                matchFoundSample = audio.Samples.Get(@"Multiplayer/Matchmaking/match-found");
            }

            public void Complete(MatchmakingRoomInvitationParams invitation)
            {
                CompletionClickAction = () =>
                {
                    client.MatchmakingAcceptInvitation().FireAndForget();
                    controller.CurrentState.Value = ScreenQueue.MatchmakingScreenState.AcceptedWaitingForRoom;

                    performer?.PerformFromScreen(s => s.Push(new ScreenIntro(invitation.Type)));

                    Close(false);
                    return true;
                };

                State = ProgressNotificationState.Completed;
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
                    Text = MultiplayerMatchStrings.MatchIsReady,
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
