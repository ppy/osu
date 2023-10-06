// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableAvatar : OsuClickableContainer
    {
        public override LocalisableString TooltipText
        {
            get
            {
                if (!Enabled.Value)
                    return string.Empty;

                return ShowUsernameTooltip ? (user?.Username ?? string.Empty) : ContextMenuStrings.ViewProfile;
            }
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// By default, the tooltip will show "view profile" as avatars are usually displayed next to a username.
        /// Setting this to <c>true</c> exposes the username via tooltip for special cases where this is not true.
        /// </summary>
        public bool ShowUsernameTooltip { get; set; }

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
    }
}
