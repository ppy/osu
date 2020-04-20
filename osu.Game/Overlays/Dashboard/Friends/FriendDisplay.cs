// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendDisplay : OverlayView<List<User>>
    {
        private List<User> users = new List<User>();

        public List<User> Users
        {
            get => users;
            set
            {
                users = value;
                onlineStreamControl.Populate(value);
            }
        }

        private CancellationTokenSource cancellationToken;

        private Drawable currentContent;

        private FriendOnlineStreamControl onlineStreamControl;
        private Box background;
        private Box controlBackground;
        private UserListToolbar userListToolbar;
        private Container itemsPlaceholder;
        private LoadingLayer loading;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            controlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Top = 20,
                                    Horizontal = 45
                                },
                                Child = onlineStreamControl = new FriendOnlineStreamControl(),
                            }
                        }
                    },
                    new Container
                    {
                        Name = "User List",
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            background = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Margin = new MarginPadding { Bottom = 20 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = 40,
                                            Vertical = 20
                                        },
                                        Child = userListToolbar = new UserListToolbar
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                        }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            itemsPlaceholder = new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Horizontal = 50 }
                                            },
                                            loading = new LoadingLayer(itemsPlaceholder)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            background.Colour = colourProvider.Background4;
            controlBackground.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineStreamControl.Current.BindValueChanged(_ => recreatePanels());
            userListToolbar.DisplayStyle.BindValueChanged(_ => recreatePanels());
            userListToolbar.SortCriteria.BindValueChanged(_ => recreatePanels());
        }

        protected override APIRequest<List<User>> CreateRequest() => new GetFriendsRequest();

        protected override void OnSuccess(List<User> response)
        {
            Users = response;
        }

        private void recreatePanels()
        {
            if (!users.Any())
                return;

            cancellationToken?.Cancel();

            if (itemsPlaceholder.Any())
                loading.Show();

            var sortedUsers = sortUsers(getUsersInCurrentGroup());

            LoadComponentAsync(createTable(sortedUsers), addContentToPlaceholder, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private List<User> getUsersInCurrentGroup()
        {
            switch (onlineStreamControl.Current.Value?.Status)
            {
                default:
                case OnlineStatus.All:
                    return users;

                case OnlineStatus.Offline:
                    return users.Where(u => !u.IsOnline).ToList();

                case OnlineStatus.Online:
                    return users.Where(u => u.IsOnline).ToList();
            }
        }

        private void addContentToPlaceholder(Drawable content)
        {
            loading.Hide();

            var lastContent = currentContent;

            if (lastContent != null)
            {
                lastContent.FadeOut(100, Easing.OutQuint).Expire();
                lastContent.Delay(25).Schedule(() => lastContent.BypassAutoSizeAxes = Axes.Y);
            }

            itemsPlaceholder.Add(currentContent = content);
            currentContent.FadeIn(200, Easing.OutQuint);
        }

        private FillFlowContainer createTable(List<User> users)
        {
            var style = userListToolbar.DisplayStyle.Value;

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(style == OverlayPanelDisplayStyle.Card ? 10 : 2),
                Children = users.Select(u => createUserPanel(u, style)).ToList()
            };
        }

        private UserPanel createUserPanel(User user, OverlayPanelDisplayStyle style)
        {
            switch (style)
            {
                default:
                case OverlayPanelDisplayStyle.Card:
                    return new UserGridPanel(user).With(panel =>
                    {
                        panel.Anchor = Anchor.TopCentre;
                        panel.Origin = Anchor.TopCentre;
                        panel.Width = 290;
                    });

                case OverlayPanelDisplayStyle.List:
                    return new UserListPanel(user);
            }
        }

        private List<User> sortUsers(List<User> unsorted)
        {
            switch (userListToolbar.SortCriteria.Value)
            {
                default:
                case UserSortCriteria.LastVisit:
                    return unsorted.OrderByDescending(u => u.LastVisit).ToList();

                case UserSortCriteria.Rank:
                    return unsorted.OrderByDescending(u => u.CurrentModeRank.HasValue).ThenBy(u => u.CurrentModeRank ?? 0).ToList();

                case UserSortCriteria.Username:
                    return unsorted.OrderBy(u => u.Username).ToList();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
