// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.UserSearch
{
    /// <summary>
    /// This is copy pasted to hell, but it's where we're at. Based off FriendList but without the friend overheads.
    /// </summary>
    public partial class UserPanelList : CompositeDrawable
    {
        private readonly OverlayPanelDisplayStyle style;
        private readonly APIUser[] users;

        public UserPanelList(OverlayPanelDisplayStyle style, APIUser[] users)
        {
            this.style = style;
            this.users = users;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(style == OverlayPanelDisplayStyle.Card ? 10 : 2),
                ChildrenEnumerable = users.Select(createUserPanel)
            };
        }

        private UserPanel createUserPanel(APIUser user)
        {
            UserPanel panel;

            switch (style)
            {
                default:
                case OverlayPanelDisplayStyle.Card:
                    panel = new UserGridPanel(user);
                    panel.Anchor = Anchor.TopCentre;
                    panel.Origin = Anchor.TopCentre;
                    panel.Width = 290;
                    break;

                case OverlayPanelDisplayStyle.List:
                    panel = new UserListPanel(user);
                    break;

                case OverlayPanelDisplayStyle.Brick:
                    panel = new UserBrickPanel(user);
                    break;
            }

            return panel;
        }
    }
}
