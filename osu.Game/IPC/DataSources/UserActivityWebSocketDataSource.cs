// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.IPC.Messages;
using osu.Game.IPC.Models;
using osu.Game.Users;

namespace osu.Game.IPC.DataSources
{
    public partial class UserActivityWebSocketDataSource : WebSocketDataSource
    {
        private readonly Bindable<UserActivity?> userActivity = new Bindable<UserActivity?>();

        public UserActivityWebSocketDataSource(IWebSocketProvider provider)
            : base(provider) { }

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            sessionStatics.BindWith(Static.UserOnlineActivity, userActivity);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userActivity.BindValueChanged(onUserActivityChange);
        }

        private void onUserActivityChange(ValueChangedEvent<UserActivity?> change)
        {
            if (change.NewValue == null)
                return;

            var msg = new UserActivityWebSocketMessage
            {
                Status = change.NewValue.GetType().Name,
                Data = getUserActivityData(change.NewValue),
            };

            BroadcastMessage(msg);
        }

        private static object? getUserActivityData(UserActivity userActivity)
        {
            switch (userActivity)
            {
                case UserActivity.InLobby inLobby:
                    return new WebSocketInLobbyUserActivityData
                    {
                        RoomId = inLobby.RoomID,
                        RoomName = inLobby.RoomName,
                    };

                case UserActivity.WatchingReplay watchingReplay:
                    return new WebSocketWatchingReplayUserActivityData
                    {
                        ScoreId = watchingReplay.ScoreID,
                        UserId = watchingReplay.UserID,
                        BeatmapId = watchingReplay.BeatmapID,
                    };

                default:
                    return null;
            }
        }
    }
}
