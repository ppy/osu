// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Users;

namespace osu.Game.Online.Broadcasts
{
    public partial class UserActivityBroadcaster : Broadcaster<string>
    {
        private Bindable<UserActivity?>? activity;

        public UserActivityBroadcaster()
            : base(@"activity")
        {
        }

        [BackgroundDependencyLoader]
        private void load(SessionStatics session)
        {
            activity = session.GetBindable<UserActivity?>(Static.UserOnlineActivity);
            activity.BindValueChanged(value => Broadcast(value.NewValue?.GetType().Name ?? no_activity), true);
        }

        private const string no_activity = @"None";
    }
}
