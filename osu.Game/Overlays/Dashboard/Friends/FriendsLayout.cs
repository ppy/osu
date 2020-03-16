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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendsLayout : CompositeDrawable
    {
        private List<APIFriend> users = new List<APIFriend>();

        public List<APIFriend> Users
        {
            get => users;
            set
            {
                users = value;

                usersLoaded = true;

                onlineStatusControl.Populate(value);
            }
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetFriendsRequest request;
        private CancellationTokenSource cancellationToken;

        private Drawable currentContent;

        private readonly Box background;
        private readonly Box controlBackground;
        private readonly FriendsOnlineStatusControl onlineStatusControl;
        private readonly UserListToolbar userListToolbar;
        private readonly Container itemsPlaceholder;
        private readonly LoadingLayer loading;

        public FriendsLayout()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
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
                                Child = onlineStatusControl = new FriendsOnlineStatusControl(),
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
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background4;
            controlBackground.Colour = colourProvider.Background5;
        }

        private bool usersLoaded;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineStatusControl.Current.BindValueChanged(_ => recreatePanels());
            userListToolbar.DisplayStyle.BindValueChanged(_ => recreatePanels());
            userListToolbar.SortCriteria.BindValueChanged(_ => recreatePanels());

            if (!api.IsLoggedIn)
                return;

            request = new GetFriendsRequest();
            request.Success += response => Schedule(() => Users = response);
            api.Queue(request);
        }

        private void recreatePanels()
        {
            // Don't allow any changes until we have users loaded
            if (!usersLoaded)
                return;

            cancellationToken?.Cancel();

            if (itemsPlaceholder.Any())
                loading.Show();

            var groupedUsers = onlineStatusControl.Current.Value?.Users ?? new List<APIFriend>();

            var sortedUsers = sortUsers(groupedUsers);

            LoadComponentAsync(createTable(sortedUsers), addContentToPlaceholder, (cancellationToken = new CancellationTokenSource()).Token);
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

        private FillFlowContainer createTable(List<APIFriend> users)
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

        private List<APIFriend> sortUsers(List<APIFriend> unsorted)
        {
            switch (userListToolbar.SortCriteria.Value)
            {
                default:
                case UserSortCriteria.LastVisit:
                    return unsorted.OrderBy(u => u.LastVisit).Reverse().ToList();

                case UserSortCriteria.Rank:
                    return unsorted.Where(u => u.CurrentModeRank.HasValue).OrderBy(u => u.CurrentModeRank).Concat(unsorted.Where(u => u.CurrentModeRank == null)).ToList();

                case UserSortCriteria.Username:
                    return unsorted.OrderBy(u => u.Username).ToList();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            cancellationToken?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
