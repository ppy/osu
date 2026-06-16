// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.IPC.Messages;
using osu.Game.Users;

namespace osu.Game.IPC.DataSources
{
    public partial class UserActivityWebSocketDataSource : WebSocketDataSource
    {
        private readonly Bindable<UserActivity?> userActivity = new Bindable<UserActivity?>();

        public UserActivityWebSocketDataSource(IWebSocketProvider provider, SessionStatics sessionStatics)
            : base(provider)
        {
            sessionStatics.BindWith(Static.UserOnlineActivity, userActivity);
            userActivity.BindValueChanged(onUserActivityChange);
        }

        private void onUserActivityChange(ValueChangedEvent<UserActivity?> change)
        {
            if (change.NewValue == null)
                return;

            var msg = new UserActivityWebSocketMessage
            {
                Status = change.NewValue.GetType().Name,
            };

            BroadcastMessage(msg);
        }
    }
}
