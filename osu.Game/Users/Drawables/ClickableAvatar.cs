// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableAvatar : OsuClickableContainer, IHasCustomTooltip<UserGridPanel>
    {
        public ITooltip<UserGridPanel> GetCustomTooltip() => new UserGridPanelTooltip(this);

        public UserGridPanel TooltipContent => new UserGridPanel(user!)
        {
            Width = 300
        };

        public override LocalisableString TooltipText
        {
            get
            {
                if (!Enabled.Value)
                    return string.Empty;

                return !IsTooltipEnabled ? (user?.Username ?? string.Empty) : ContextMenuStrings.ViewProfile;
            }
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// By default, the tooltip will show "view profile" as avatars are usually displayed next to a username.
        /// Setting this to <c>true</c> exposes the username via tooltip for special cases where this is not true.
        /// </summary>
        // public bool ShowUsernameTooltip { get; set; }

        public bool IsTooltipEnabled { get; set; }

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

            if (user?.Id != APIUser.SYSTEM_USER_ID)
                Action = openProfile;
        }

        public void SetValue(out bool value)
        {
            value = IsTooltipEnabled;
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

        public partial class UserGridPanelTooltip : VisibilityContainer, ITooltip<UserGridPanel>
        {
            private readonly ClickableAvatar parent;
            private UserGridPanel? displayedUser;
            private bool isEnabled;

            public UserGridPanelTooltip(ClickableAvatar parent)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
                isEnabled = this.parent.IsTooltipEnabled;
            }

            protected override void PopIn()
            {
                parent.SetValue(out isEnabled);

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
