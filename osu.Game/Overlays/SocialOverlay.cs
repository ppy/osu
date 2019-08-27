// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SearchableList;
using osu.Game.Overlays.Social;
using osu.Game.Users;
using osu.Framework.Threading;
using System;
using System.Threading;

namespace osu.Game.Overlays
{
    public class SocialOverlay : SearchableListOverlay<SocialTab, SocialSortCriteria, SortDirection>
    {
        private readonly LoadingAnimation loading;
        private FillFlowContainer<SocialPanel> panels;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"5c2648");

        protected override SearchableListHeader<SocialTab> CreateHeader() => new Header();
        protected override SearchableListFilterControl<SocialSortCriteria, SortDirection> CreateFilterControl() => new FilterControl();

        private IEnumerable<User> users;

        public IEnumerable<User> Users
        {
            get => users;
            set
            {
                if (users?.Equals(value) ?? false)
                    return;

                users = value?.ToList();
            }
        }

        public SocialOverlay()
        {
            Waves.FirstWaveColour = OsuColour.FromHex(@"cb5fa0");
            Waves.SecondWaveColour = OsuColour.FromHex(@"b04384");
            Waves.ThirdWaveColour = OsuColour.FromHex(@"9b2b6e");
            Waves.FourthWaveColour = OsuColour.FromHex(@"6d214d");

            Add(loading = new LoadingAnimation());

            Filter.Search.Current.ValueChanged += text =>
            {
                if (!string.IsNullOrEmpty(text.NewValue))
                {
                    // force searching in players until searching for friends is supported
                    Header.Tabs.Current.Value = SocialTab.AllPlayers;

                    if (Filter.Tabs.Current.Value != SocialSortCriteria.Rank)
                        Filter.Tabs.Current.Value = SocialSortCriteria.Rank;
                }
            };

            Header.Tabs.Current.ValueChanged += _ => queueUpdate();

            Filter.Tabs.Current.ValueChanged += _ => queueUpdate();

            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += style => recreatePanels(style.NewValue);
            Filter.DisplayStyleControl.Dropdown.Current.ValueChanged += _ => updateUsers(Users);

            currentQuery.BindTo(Filter.Search.Current);
            currentQuery.ValueChanged += query =>
            {
                Scheduler.CancelDelayedTasks();

                if (string.IsNullOrEmpty(query.NewValue))
                    queueUpdate();
                else
                    Scheduler.AddDelayed(updateSearch, 500);
            };
        }

        private APIRequest getUsersRequest;

        private readonly Bindable<string> currentQuery = new Bindable<string>();

        private void queueUpdate() => Scheduler.AddOnce(updateSearch);

        private CancellationTokenSource loadCancellation;

        private void updateSearch()
        {
            Scheduler.CancelDelayedTasks();

            if (!IsLoaded)
                return;

            Users = null;
            clearPanels();
            getUsersRequest?.Cancel();

            if (API?.IsLoggedIn != true)
                return;

            switch (Header.Tabs.Current.Value)
            {
                case SocialTab.Friends:
                    var friendRequest = new GetFriendsRequest(); // TODO filter arguments?
                    friendRequest.Success += updateUsers;
                    API.Queue(getUsersRequest = friendRequest);
                    break;

                default:
                    var userRequest = new GetUsersRequest(); // TODO filter arguments!
                    userRequest.Success += response => updateUsers(response.Select(r => r.User));
                    API.Queue(getUsersRequest = userRequest);
                    break;
            }
        }

        private void recreatePanels(PanelDisplayStyle displayStyle)
        {
            clearPanels();

            if (Users == null)
            {
                loading.Hide();
                return;
            }

            loadCancellation = new CancellationTokenSource();

            var newPanels = new FillFlowContainer<SocialPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10f),
                Margin = new MarginPadding { Top = 10 },
                ChildrenEnumerable = Users.Select(u =>
                {
                    SocialPanel panel;

                    switch (displayStyle)
                    {
                        case PanelDisplayStyle.Grid:
                            panel = new SocialGridPanel(u)
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre
                            };
                            break;

                        default:
                            panel = new SocialListPanel(u);
                            break;
                    }

                    panel.Status.BindTo(u.Status);
                    panel.Activity.BindTo(u.Activity);
                    return panel;
                })
            };

            LoadComponentAsync(newPanels, f =>
            {
                loading.Hide();
                ScrollFlow.Add(panels = newPanels);
            }, loadCancellation.Token);
        }

        private void updateUsers(IEnumerable<User> newUsers)
        {
            var sortDirection = Filter.DisplayStyleControl.Dropdown.Current.Value;

            IEnumerable<User> sortedUsers = newUsers;

            if (sortedUsers.Any())
            {
                switch (Filter.Tabs.Current.Value)
                {
                    case SocialSortCriteria.Location:
                        sortedUsers = sortBy(sortedUsers, u => u.Country.FullName, sortDirection);
                        break;

                    case SocialSortCriteria.Name:
                        sortedUsers = sortBy(sortedUsers, u => u.Username, sortDirection);
                        break;
                }
            }

            Users = sortedUsers;
            recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
        }

        private IEnumerable<User> sortBy<T>(IEnumerable<User> users, Func<User, T> condition, SortDirection sortDirection) =>
            sortDirection == SortDirection.Ascending ? users.OrderBy(condition) : users.OrderByDescending(condition);

        private void clearPanels()
        {
            loading.Show();

            loadCancellation?.Cancel();

            if (panels != null)
            {
                panels.Expire();
                panels = null;
            }
        }

        public override void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    queueUpdate();
                    break;

                default:
                    Users = null;
                    clearPanels();
                    break;
            }
        }
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
