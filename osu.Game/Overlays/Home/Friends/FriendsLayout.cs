// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Social;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Home.Friends
{
    public class FriendsLayout : CompositeDrawable
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Bindable<User> localUser = new Bindable<User>();

        private GetFriendsRequest request;
        private CancellationTokenSource cancellationToken;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineStatusControl.Current.BindValueChanged(_ => recreatePanels());
            userListToolbar.DisplayStyle.BindValueChanged(_ => recreatePanels());
            userListToolbar.SortCriteria.BindValueChanged(_ => recreatePanels());

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => refetch(), true);
        }

        private void refetch()
        {
            if (itemsPlaceholder.Any())
                loading.Show();

            request?.Cancel();
            cancellationToken?.Cancel();
            onlineStatusControl.Clear();

            if (!api.IsLoggedIn)
            {
                itemsPlaceholder.Clear();
                return;
            }

            request = new GetFriendsRequest();
            request.Success += users => Schedule(() => onlineStatusControl.Populate(users));
            api.Queue(request);
        }

        private void recreatePanels()
        {
            if (itemsPlaceholder.Any())
                loading.Show();

            cancellationToken?.Cancel();

            var statefulUsers = onlineStatusControl.Current.Value?.Users ?? new List<User>();

            var sortedUsers = sortUsers(statefulUsers);

            var table = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Children = sortedUsers.Select(u => createUserPanel(u)).ToList()
            };

            LoadComponentAsync(table, loaded =>
            {
                itemsPlaceholder.Clear();
                itemsPlaceholder.Add(loaded);

                loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private SocialPanel createUserPanel(User user)
        {
            switch (userListToolbar.DisplayStyle.Value)
            {
                default:
                case OverlayPanelDisplayStyle.Card:
                    return new SocialGridPanel(user).With(panel =>
                    {
                        panel.Anchor = Anchor.TopCentre;
                        panel.Origin = Anchor.TopCentre;
                    });

                case OverlayPanelDisplayStyle.List:
                    return new SocialListPanel(user);
            }
        }

        private List<User> sortUsers(List<User> users)
        {
            switch (userListToolbar.SortCriteria.Value)
            {
                default:
                case UserSortCriteria.LastVisit:
                    return users.OrderBy(u => u.LastVisit).Reverse().ToList();

                case UserSortCriteria.Rank:
                    return users.Where(u => u.CurrentModeRank.HasValue).OrderBy(u => u.CurrentModeRank).Concat(users.Where(u => !u.CurrentModeRank.HasValue)).ToList();

                case UserSortCriteria.Username:
                    return users.OrderBy(u => u.Username).ToList();
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
