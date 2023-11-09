// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableAvatar : OsuClickableContainer, IHasCustomTooltip<ClickableAvatar.APIUserTooltipContent>
    {
        // public ITooltip<APIUser> GetCustomTooltip() => new APIUserTooltip(user!) { ShowTooltip = TooltipEnabled };
        public ITooltip<APIUserTooltipContent> GetCustomTooltip() => new APIUserTooltip(new APIUserTooltipContent(user!));

        public APIUserTooltipContent TooltipContent => content;

        private readonly APIUserTooltipContent content;
        private readonly APIUser? user;
        private bool tooltipEnabled;

        public override LocalisableString TooltipText => user!.Username;

        public bool ShowUsernameOnly
        {
            get => tooltipEnabled;
            set
            {
                tooltipEnabled = value;
                content.ShowUsernameOnly = ShowUsernameOnly;
            }
        }

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

            content = new APIUserTooltipContent(user!, ShowUsernameOnly);
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

        public partial class APIUserTooltip : VisibilityContainer, ITooltip<APIUserTooltipContent>
        {
            private OsuSpriteText text;
            private APIUserTooltipContent content;
            public APIUserTooltip(APIUserTooltipContent content)
            {
                this.content = content;
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 5;

                Child = new UserGridPanel(content.User)
                {
                    Width = 300
                };
                text = new OsuSpriteText()
                {
                    Text = this.content.User.Username
                };
            }

            protected override void PopIn()
            {
                if (content.ShowUsernameOnly)
                {
                    Child = new UserGridPanel(content.User)
                    {
                        Width = 300
                    };
                }
                else
                {
                    Alpha = 0;
                    AutoSizeAxes = Axes.Both;

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Gray,
                        },
                        text = new OsuSpriteText()
                        {
                            Font = FrameworkFont.Regular.With(size: 16),
                            Padding = new MarginPadding(5),
                            Text = content.User.Username
                        }
                    };
                }

                this.FadeIn(20, Easing.OutQuint);
            }

            protected override void PopOut() => this.FadeOut(80, Easing.OutQuint);

            public void Move(Vector2 pos) => Position = pos;

            public void SetContent(APIUserTooltipContent content)
            {
                this.content = content;
                text.Text = this.content.User.Username;
            }
        }

        public class APIUserTooltipContent
        {
            public APIUser User { get; }
            public bool ShowUsernameOnly { get; set; }

            public APIUserTooltipContent(APIUser user, bool showUsernameOnly = false)
            {
                User = user;
                ShowUsernameOnly = showUsernameOnly;
            }
        }
    }
}
