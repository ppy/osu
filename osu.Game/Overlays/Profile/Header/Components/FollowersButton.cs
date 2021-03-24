// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class FollowersButton : ProfileHeaderStatisticsButton
    {
        public readonly Bindable<User> User = new Bindable<User>();

        public override string TooltipText => "followers";

        protected override IconUsage Icon => FontAwesome.Solid.User;

        [BackgroundDependencyLoader]
        private void load()
        {
            // todo: when friending/unfriending is implemented, the APIAccess.Friends list should be updated accordingly.
            User.BindValueChanged(user => SetValue(user.NewValue?.FollowerCount ?? 0), true);
        }
    }
}
