// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableAvatar : OsuClickableContainer, IHasCustomTooltip<UserGridPanel>
    {
        public ITooltip<UserGridPanel> GetCustomTooltip() => new UserGridPanelTooltip(IsTooltipEnabled);

        public UserGridPanel TooltipContent => new UserGridPanel(user!)
        {
            Width = 300
        };

        public bool IsTooltipEnabled;

        private readonly APIUser? user;

        [Resolved]
        private OsuGame? game { get; set; }

        /// <summary>
        /// A clickable avatar for the specified user, with UI sounds included.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        public ClickableAvatar(APIUser? user = null)
        {
            this.user = user;
            IsTooltipEnabled = true;

            if (user?.Id != APIUser.SYSTEM_USER_ID)
                Action = openProfile;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponentAsync(new DrawableAvatar(user), Add);
        }

        private void openProfile()
        {
            if (user?.Id > 1 || !string.IsNullOrEmpty(user?.Username))
                game?.ShowUser(user);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Enabled.Value)
                return false;

            return base.OnClick(e);
        }

        private partial class UserGridPanelTooltip : VisibilityContainer, ITooltip<UserGridPanel>
        {
            private readonly bool isEnabled;
            private UserGridPanel? displayedUser;

            public UserGridPanelTooltip(bool isEnabled = true)
            {
                this.isEnabled = isEnabled;
            }

            protected override void PopIn()
            {
                if (displayedUser is null || !isEnabled)
                {
                    return;
                }

                Child = displayedUser;
                this.FadeIn(20, Easing.OutQuint);
            }

            protected override void PopOut() => this.FadeOut(80, Easing.OutQuint);

            public void Move(Vector2 pos) => Position = pos;

            public void SetContent(UserGridPanel userGridPanel)
            {
                displayedUser = userGridPanel;
            }
        }
    }
}
