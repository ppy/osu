// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class MatchmakingController : Component
    {
        public readonly Bindable<MatchmakingQueueScreen.MatchmakingScreenState> CurrentState = new Bindable<MatchmakingQueueScreen.MatchmakingScreenState>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        private ProgressNotification? backgroundNotification;
        private Notification? readyNotification;
        private bool isBackgrounded;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.MatchmakingQueueJoined += onMatchmakingQueueJoined;
            client.MatchmakingQueueLeft += onMatchmakingQueueLeft;
            client.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            client.MatchmakingRoomReady += onMatchmakingRoomReady;

            ruleset.BindValueChanged(_ => client.MatchmakingLeaveQueue().FireAndForget());
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
                CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.Idle;
        });

        private void onMatchmakingQueueJoined() => Scheduler.Add(() =>
        {
            CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.Queueing;

            if (isBackgrounded)
            {
                closeNotifications();
                postNotification();
            }
        });

        private void onMatchmakingQueueLeft() => Scheduler.Add(() =>
        {
            if (CurrentState.Value != MatchmakingQueueScreen.MatchmakingScreenState.InRoom)
                CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.Idle;

            closeNotifications();
        });

        private void onMatchmakingRoomInvited() => Scheduler.Add(() =>
        {
            CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.PendingAccept;

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
                      CurrentState.Value = MatchmakingQueueScreen.MatchmakingScreenState.InRoom;
                  }));
        });

        private void postNotification()
        {
            if (backgroundNotification != null)
                return;

            notifications?.Post(backgroundNotification = new ProgressNotification
            {
                Text = "Searching for opponents...",
                CompletionTarget = n => notifications.Post(readyNotification = n),
                CompletionText = "Your match is ready! Click to join.",
                CompletionClickAction = () =>
                {
                    client.MatchmakingAcceptInvitation().FireAndForget();
                    performer?.PerformFromScreen(s => s.Push(new MatchmakingIntroScreen()));

                    closeNotifications();
                    return true;
                },
                CancelRequested = () =>
                {
                    client.MatchmakingLeaveQueue().FireAndForget();

                    closeNotifications();
                    return true;
                }
            });
        }

        private void closeNotifications()
        {
            if (backgroundNotification != null)
            {
                backgroundNotification.State = ProgressNotificationState.Cancelled;
                backgroundNotification.Close(false);
            }

            readyNotification?.Close(false);

            backgroundNotification = null;
            readyNotification = null;
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
    }
}
