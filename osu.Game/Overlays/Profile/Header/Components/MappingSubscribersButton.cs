// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class MappingSubscribersButton : ProfileHeaderStatisticsButton
    {
        public readonly Bindable<User> User = new Bindable<User>();

        public override string TooltipText => "mapping subscribers";

        protected override IconUsage Icon => FontAwesome.Solid.Bell;

        [BackgroundDependencyLoader]
        private void load()
        {
            User.BindValueChanged(user => SetValue(user.NewValue?.MappingFollowerCount ?? 0), true);
        }
    }
}
