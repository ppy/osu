// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class UserListToolbar : CompositeDrawable
    {
        public Bindable<UserSortCriteria> SortCriteria => sortControl.Current;

        public Bindable<OverlayPanelDisplayStyle> DisplayStyle => styleControl.Current;

        private readonly UserSortTabControl sortControl;
        private readonly OverlayPanelDisplayStyleControl styleControl;

        public UserListToolbar()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    sortControl = new UserSortTabControl
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    styleControl = new OverlayPanelDisplayStyleControl
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            });
        }
    }
}
