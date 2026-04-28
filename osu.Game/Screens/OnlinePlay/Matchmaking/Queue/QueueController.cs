// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Matchmaking.Requests;
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
        public readonly Bindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private UserLookupCache users { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private BackgroundQueueNotification? backgroundNotification;

        private bool isBackgrounded = true;

        private int? lastDuelUser;
        private MatchmakingPool? lastDuelPool;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingDuelIssued += onMatchmakingDuelIssued;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;
        }

        /// <summary>
        /// Joins the matchmaking queue.
        /// </summary>
        /// <param name="pool">The pool to join.</param>
        public void JoinQueue(MatchmakingPool pool)
        {
            lastDuelUser = null;
            lastDuelPool = null;

            client.MatchmakingJoinQueue(pool.Id).FireAndForget();
        }

        /// <summary>
        /// Leaves the matchmaking queue.
        /// </summary>
        public void LeaveQueue()
        {
            lastDuelUser = null;
            lastDuelPool = null;

            client.MatchmakingLeaveQueue().FireAndForget();
        }

        public void IssueDuel(MatchmakingPool pool, int userId)
        {
            lastDuelUser = userId;
            lastDuelPool = pool;

            client.MatchmakingIssueDuel(new MatchmakingIssueDuelRequest
            {
                PoolId = pool.Id,
                UserId = userId
            }).FireAndForget();
        }

        public void AcceptDuel(MatchmakingDuelIssuedParams duel)
        {
            lastDuelUser = duel.UserId;
            lastDuelPool = duel.Pool;

            client.MatchmakingAcceptDuel(new MatchmakingAcceptDuelRequest
            {
                Id = duel.Id
            }).FireAndForget();
        }

        /// <summary>
        /// Rejoins the last joined matchmaking queue.
        /// </summary>
        public void RejoinQueue()
        {
            if (lastDuelUser != null && lastDuelPool != null)
                IssueDuel(lastDuelPool, lastDuelUser.Value);
            else if (SelectedPool.Value != null)
                JoinQueue(SelectedPool.Value);
        }

        /// <summary>
        /// Moves the matchmaking queue search to the background.
        /// </summary>
        public void SearchInBackground()
        {
            if (isBackgrounded)
                return;

            isBackgrounded = true;

            if (CurrentState.Value == ScreenQueue.MatchmakingScreenState.Queueing)
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
            if (isBackgrounded)
                postNotification();

            CurrentState.Value = ScreenQueue.MatchmakingScreenState.PendingAccept;

            backgroundNotification?.Complete(invitation);
            backgroundNotification = null;
        });

        private void onMatchmakingDuelIssued(MatchmakingDuelIssuedParams duel)
        {
            users.GetUserAsync(duel.UserId)
                 .ContinueWith(u => Scheduler.Add(() =>
                 {
                     notifications?.Post(new DuelNotification(this, u.GetResultSafely()!, duel));
                 }), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

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
                Text = MultiplayerMatchStrings.SearchingForOpponents;

                Activated = () =>
                {
                    performer?.PerformFromScreen(s =>
                    {
                        if (s is ScreenIntro || s is ScreenQueue)
                            return;

                        s.Push(new ScreenIntro(MatchmakingPoolType.RankedPlay));
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

        private partial class DuelNotification : SimpleNotification
        {
            public DuelNotification(QueueController controller, APIUser user, MatchmakingDuelIssuedParams duel)
            {
                Text = $"{user.Username} challenged you to a duel ({duel.Pool.DisplayName}). Click to accept.";

                Activated = () =>
                {
                    controller.AcceptDuel(duel);
                    return true;
                };
            }
        }
    }
}
