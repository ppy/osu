// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class GroupBadgeFlow : FillFlowContainer
    {
        public readonly Bindable<APIUser?> User = new Bindable<APIUser?>();

        public GroupBadgeFlow()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(2);

            User.BindValueChanged(user =>
            {
                Clear(true);

                if (user.NewValue?.Groups != null)
                    AddRange(user.NewValue.Groups.Select(g => new GroupBadge(g)));
            });
        }
    }
}
