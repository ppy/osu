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

        public GroupBadgeFlow(bool combineMultiple = false)
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(2);

            User.BindValueChanged(user =>
            {
                Clear(true);

                var groups = user.NewValue?.Groups;

                if (groups != null && groups.Length > 0)
                {
                    if (combineMultiple)
                    {
                        Add(new GroupBadge(groups[0]));

                        if (groups.Length > 1)
                            Add(new CombinedGroupBadge(groups.Skip(1).ToArray()));
                    }
                    else
                    {
                        AddRange(groups.Select(g => new GroupBadge(g)));
                    }
                }
            });
        }
    }
}
