// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SearchableList;
using osu.Game.Overlays.Social;
using osu.Game.Users;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Threading;

namespace osu.Game.Overlays
{
    public class SocialOverlay : SearchableListOverlay<SocialTab, SocialSortCriteria, SortDirection>
    {
        private readonly LoadingSpinner loading;
        private FillFlowContainer<SocialPanel> panels;

        protected override Color4 BackgroundColour => Color4Extensions.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => Color4Extensions.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => Color4Extensions.FromHex(@"5c2648");

        protected override SearchableListHeader<SocialTab> CreateHeader() => new Header();
        protected override SearchableListFilterControl<SocialSortCriteria, SortDirection> CreateFilterControl() => new FilterControl();

        private User[] users = Array.Empty<User>();

        public User[] Users
        {
            get => users;
            set
            {
                if (users == value)
                    return;

                users = value ?? Array.Empty<User>();

                if (LoadState >= LoadState.Ready)
                    recreatePanels();
            }
        }

        public SocialOverlay()
            : base(OverlayColourScheme.Pink)
        {
            Add(loading = new LoadingSpinner());

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
            Filter.Tabs.Current.ValueChanged += _ => onFilterUpdate();

            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += _ => recreatePanels();
            Filter.DisplayStyleControl.Dropdown.Current.ValueChanged += _ => recreatePanels();

            currentQuery.BindTo(Filter.Search.Current);
            currentQuery.ValueChanged += query =>
            {
                queryChangedDebounce?.Cancel();

                if (string.IsNullOrEmpty(query.NewValue))
                    queueUpdate();
                else
                    queryChangedDebounce = Scheduler.AddDelayed(updateSearch, 500);
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            recreatePanels();
        }

        private APIRequest getUsersRequest;

        private readonly Bindable<string> currentQuery = new Bindable<string>();

        private ScheduledDelegate queryChangedDebounce;

        private void queueUpdate() => Scheduler.AddOnce(updateSearch);

        private CancellationTokenSource loadCancellation;

        private void updateSearch()
        {
            queryChangedDebounce?.Cancel();

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
                    friendRequest.Success += users => Users = users.ToArray();
                    API.Queue(getUsersRequest = friendRequest);
                    break;

                default:
                    var userRequest = new GetUsersRequest(); // TODO filter arguments!
                    userRequest.Success += res => Users = res.Users.Select(r => r.User).ToArray();
                    API.Queue(getUsersRequest = userRequest);
                    break;
            }
        }

        private void recreatePanels()
        {
            clearPanels();

            if (Users == null)
            {
                loading.Hide();
                return;
            }

            IEnumerable<User> sortedUsers = Users;

            switch (Filter.Tabs.Current.Value)
            {
                case SocialSortCriteria.Location:
                    sortedUsers = sortedUsers.OrderBy(u => u.Country.FullName);
                    break;

                case SocialSortCriteria.Name:
                    sortedUsers = sortedUsers.OrderBy(u => u.Username);
                    break;
            }

            if (Filter.DisplayStyleControl.Dropdown.Current.Value == SortDirection.Descending)
                sortedUsers = sortedUsers.Reverse();

            var newPanels = new FillFlowContainer<SocialPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10f),
                Margin = new MarginPadding { Top = 10 },
                ChildrenEnumerable = sortedUsers.Select(u =>
                {
                    SocialPanel panel;

                    switch (Filter.DisplayStyleControl.DisplayStyle.Value)
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
                if (panels != null)
                    ScrollFlow.Remove(panels);

                loading.Hide();
                ScrollFlow.Add(panels = newPanels);
            }, (loadCancellation = new CancellationTokenSource()).Token);
        }

        private void onFilterUpdate()
        {
            if (Filter.Tabs.Current.Value == SocialSortCriteria.Rank)
            {
                queueUpdate();
                return;
            }

            recreatePanels();
        }

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
