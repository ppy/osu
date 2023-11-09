// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableAvatar : OsuClickableContainer, IHasCustomTooltip<APIUser?>
    {
        // public ITooltip<APIUser> GetCustomTooltip() => new APIUserTooltip(user!) { ShowTooltip = TooltipEnabled };
        public ITooltip<APIUser?> GetCustomTooltip() => new UserCardTooltip();

        public APIUser? TooltipContent { get; }

        private readonly APIUser? user;

        // TODO: reimplement.
        public bool ShowUsernameOnly { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        /// <summary>
        /// A clickable avatar for the specified user, with UI sounds included.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        public ClickableAvatar(APIUser? user = null)
        {
            if (user?.Id != APIUser.SYSTEM_USER_ID)
                Action = openProfile;

            TooltipContent = this.user = user;
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

        public partial class UserCardTooltip : VisibilityContainer, ITooltip<APIUser?>
        {
            public UserCardTooltip()
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 5;
            }

            protected override void PopIn()
            {
                this.FadeIn(20, Easing.OutQuint);
            }

            protected override void PopOut() => this.FadeOut(80, Easing.OutQuint);

            public void Move(Vector2 pos) => Position = pos;

            public void SetContent(APIUser? content) => LoadComponentAsync(new UserGridPanel(content ?? new GuestUser())
            {
                Width = 300,
            }, panel => Child = panel);
        }
    }
}
