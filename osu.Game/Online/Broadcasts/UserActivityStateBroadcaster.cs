// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Users;

namespace osu.Game.Online.Broadcasts
{
    public partial class UserActivityStateBroadcaster : GameStateBroadcaster<string>
    {
        public override string Type => @"Activity";
        public override string Message => activity?.Value?.GetType().Name;

        private IBindable<UserActivity> activity;

        [BackgroundDependencyLoader]
        private void load(SessionStatics session)
        {
            activity = session.GetBindable<UserActivity?>(Static.UserOnlineActivity);
            activity.ValueChanged += _ => Broadcast();
        }
    }
}
