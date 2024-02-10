// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableAvatar : OsuClickableContainer, IHasCustomTooltip<APIUser?>
    {
        public ITooltip<APIUser?> GetCustomTooltip() => showCardOnHover ? new UserCardTooltip() : new NoCardTooltip();

        public APIUser? TooltipContent { get; }

        private readonly APIUser? user;

        private readonly bool showCardOnHover;

        [Resolved]
        private OsuGame? game { get; set; }

        /// <summary>
        /// A clickable avatar for the specified user, with UI sounds included.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        /// <param name="showCardOnHover">If set to true, the <see cref="UserGridPanel"/> will be shown for the tooltip</param>
        public ClickableAvatar(APIUser? user = null, bool showCardOnHover = false)
        {
            if (user?.Id != APIUser.SYSTEM_USER_ID)
                Action = openProfile;

            this.showCardOnHover = showCardOnHover;

            TooltipContent = this.user = user ?? new GuestUser();
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
            }

            protected override void PopIn() => this.FadeIn(150, Easing.OutQuint);
            protected override void PopOut() => this.Delay(150).FadeOut(500, Easing.OutQuint);

            public void Move(Vector2 pos) => Position = pos;

            private APIUser? user;

            public void SetContent(APIUser? content)
            {
                if (content == user && Children.Any())
                    return;

                user = content;

                if (user != null)
                {
                    LoadComponentAsync(new UserGridPanel(user)
                    {
                        Width = 300,
                    }, panel => Child = panel);
                }
                else
                {
                    var tooltip = new OsuTooltipContainer.OsuTooltip();
                    tooltip.SetContent(ContextMenuStrings.ViewProfile);
                    tooltip.Show();

                    Child = tooltip;
                }
            }
        }

        public partial class NoCardTooltip : VisibilityContainer, ITooltip<APIUser?>
        {
            private readonly OsuTooltipContainer.OsuTooltip tooltip;

            public NoCardTooltip()
            {
                tooltip = new OsuTooltipContainer.OsuTooltip();
                tooltip.SetContent(ContextMenuStrings.ViewProfile);
                Child = tooltip;
            }

            protected override void PopIn() => tooltip.Show();
            protected override void PopOut() => tooltip.Hide();

            public void Move(Vector2 pos) => Position = pos;

            public void SetContent(APIUser? content)
            {
            }
        }
    }
}
