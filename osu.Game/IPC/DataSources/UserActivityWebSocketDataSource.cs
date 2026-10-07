// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
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

            userActivity.BindValueChanged(_ => broadcastUserActivity());
        }

        public override Task OnClientConnected(int clientId)
        {
            if (userActivity.Value == null)
                return Task.CompletedTask;

            var msg = new UserActivityWebSocketMessage
            {
                Status = userActivity.Value.GetType().Name,
                Data = getUserActivityData(userActivity.Value),
            };

            SendMessage(clientId, msg);

            return Task.CompletedTask;
        }

        private void broadcastUserActivity()
        {
            if (userActivity.Value == null)
                return;

            var msg = new UserActivityWebSocketMessage
            {
                Status = userActivity.Value.GetType().Name,
                Data = getUserActivityData(userActivity.Value),
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
