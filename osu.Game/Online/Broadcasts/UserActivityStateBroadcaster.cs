// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Online.Broadcasts
{
    public class UserActivityStateBroadcaster : GameStateBroadcaster<string>
    {
        public override string Type => @"Activity";
        public override string Message => activity?.Value?.GetType().Name;

        private IBindable<UserActivity> activity;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            activity = api.Activity.GetBoundCopy();
            activity.ValueChanged += _ => Broadcast();
        }
    }
}
