// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Users
{
    public partial class UserSquareList : VisibilityContainer
    {
        #region Fields

        private ScheduledDelegate? popOutDelegate;
        private List<APIUser>? users;
        private bool isTargetHovered;

        private readonly UsersList usersList = new UsersList();

        #endregion

        #region Properties

        public bool IsTargetHovered
        {
            get => isTargetHovered;
            set
            {
                isTargetHovered = value;

                if (!isTargetHovered)
                {
                    schedulePopOut();
                }
            }
        }

        public List<APIUser>? Users
        {
            get => users;
            set
            {
                users = value;
                usersList.UpdateUsers(users ?? new List<APIUser>());
            }
        }

        #endregion

        #region Protected overrides

        protected override bool OnHover(HoverEvent e)
        {
            schedulePopOut();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            schedulePopOut();
            base.OnHoverLost(e);
        }

        protected override void PopIn()
        {
            ClearTransforms();
            this.FadeIn(100);

            schedulePopOut();
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // keep the scheduled event correctly timed as long as we have movement.
            schedulePopOut();
            return base.OnMouseMove(e);
        }

        #endregion

        public override void Show()
        {
            if (State.Value == Visibility.Visible)
            {
                schedulePopOut();
            }

            base.Show();
        }

        #region Private members

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;
            AutoSizeDuration = 200;
            AutoSizeEasing = Easing.OutQuint;
            Masking = true;
            CornerRadius = 5;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.9f,
                    Colour = colours.Gray3,
                },
                usersList,
            });
        }

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();
            this.Delay(1000).Schedule(() =>
            {
                if (!IsHovered && !IsTargetHovered)
                {
                    Hide();
                }
            }, out popOutDelegate);
        }

        #endregion

        private partial class UsersList : FillFlowContainer
        {
            public void UpdateUsers(IReadOnlyCollection<APIUser> users)
            {
                var avatars = users.Select(u => new HoverableUserAvatar(u).With(avatar =>
                {
                    avatar.Anchor = Anchor.TopLeft;
                    avatar.Origin = Anchor.TopLeft;
                    avatar.Size = new Vector2(30);
                    avatar.CornerRadius = 5;
                    avatar.Masking = true;
                })).Take(50).ToArray();
                var contents = new List<Drawable>();
                contents.AddRange(avatars);

                if (users.Count > 50)
                {
                    contents.Add(new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Margin = new MarginPadding
                            {
                                Right = 4
                            },
                            Text = $"+ {users.Count - 50}"
                        }
                    });
                }

                Children = contents;
                Width = users.Count <= 10 ? users.Count * 34 : 330;
            }

            public UsersList()
            {
                Anchor = Anchor.TopLeft;
                Origin = Anchor.TopLeft;
                Width = 323;
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Full;
                Spacing = new Vector2(2, 2);
                Padding = new MarginPadding(2)
                {
                    Left = 4,
                    Right = 4
                };
            }
        }

        private partial class UserGridPanelTooltip : VisibilityContainer, ITooltip<UserGridPanel>
        {
            private UserGridPanel? displayedUser;

            protected override void PopIn()
            {
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

        private partial class HoverableUserAvatar : OsuClickableContainer, IHasCustomTooltip<UserGridPanel>
        {
            public ITooltip<UserGridPanel> GetCustomTooltip() => new UserGridPanelTooltip();
            public UserGridPanel TooltipContent => new UserGridPanel(user!) { Width = 300 };

            private readonly APIUser? user;

            [Resolved]
            private OsuGame? game { get; set; }

            /// <summary>
            /// A clickable avatar for the specified user, with UI sounds included.
            /// </summary>
            /// <param name="user">The user. A null value will get a placeholder avatar.</param>
            public HoverableUserAvatar(APIUser? user)
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
}
