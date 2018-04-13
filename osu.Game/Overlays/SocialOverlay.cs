// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SearchableList;
using osu.Game.Overlays.Social;
using osu.Game.Users;
using osu.Framework.Configuration;
using osu.Framework.Threading;

namespace osu.Game.Overlays
{
    public class SocialOverlay : SearchableListOverlay<SocialTab, SocialSortCriteria, SortDirection>, IOnlineComponent
    {
        private APIAccess api;
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
            get { return users; }
            set
            {
                if (users?.Equals(value) ?? false)
                    return;

                users = value?.ToList();
            }
        }

        public SocialOverlay()
        {
            FirstWaveColour = OsuColour.FromHex(@"cb5fa0");
            SecondWaveColour = OsuColour.FromHex(@"b04384");
            ThirdWaveColour = OsuColour.FromHex(@"9b2b6e");
            FourthWaveColour = OsuColour.FromHex(@"6d214d");

            Add(loading = new LoadingAnimation());

            Filter.Search.Current.ValueChanged += text =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    // force searching in players until searching for friends is supported
                    Header.Tabs.Current.Value = SocialTab.AllPlayers;

                    if (Filter.Tabs.Current.Value != SocialSortCriteria.Rank)
                        Filter.Tabs.Current.Value = SocialSortCriteria.Rank;
                }
            };

            Header.Tabs.Current.ValueChanged += tab => Scheduler.AddOnce(updateSearch);

            Filter.Tabs.Current.ValueChanged += sortCriteria => Scheduler.AddOnce(updateSearch);

            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += recreatePanels;
            Filter.DisplayStyleControl.Dropdown.Current.ValueChanged += sortOrder => Scheduler.AddOnce(updateSearch);

            currentQuery.ValueChanged += query =>
            {
                queryChangedDebounce?.Cancel();

                if (string.IsNullOrEmpty(query))
                    Scheduler.AddOnce(updateSearch);
                else
                    queryChangedDebounce = Scheduler.AddDelayed(updateSearch, 500);
            };

            currentQuery.BindTo(Filter.Search.Current);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
            api.Register(this);
        }

        private void recreatePanels(PanelDisplayStyle displayStyle)
        {
            clearPanels();

            if (Users == null)
                return;

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
                    return panel;
                })
            };

            LoadComponentAsync(newPanels, f =>
            {
                if(panels != null)
                    ScrollFlow.Remove(panels);

                ScrollFlow.Add(panels = newPanels);
            });
        }

        private APIRequest getUsersRequest;

        private readonly Bindable<string> currentQuery = new Bindable<string>();

        private ScheduledDelegate queryChangedDebounce;

        private void updateSearch()
        {
            queryChangedDebounce?.Cancel();

            if (!IsLoaded)
                return;

            Users = null;
            clearPanels();
            loading.Hide();
            getUsersRequest?.Cancel();

            if (api?.IsLoggedIn != true)
                return;

            switch (Header.Tabs.Current.Value)
            {
                case SocialTab.Friends:
                    var friendRequest = new GetFriendsRequest(); // TODO filter arguments?
                    friendRequest.Success += updateUsers;
                    api.Queue(getUsersRequest = friendRequest);
                    break;
                default:
                    var userRequest = new GetUsersRequest(); // TODO filter arguments!
                    userRequest.Success += response => updateUsers(response.Select(r => r.User));
                    api.Queue(getUsersRequest = userRequest);
                    break;
            }
            loading.Show();
        }

        private void updateUsers(IEnumerable<User> newUsers)
        {
            Users = newUsers;
            loading.Hide();
            recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
        }

        private void clearPanels()
        {
            if (panels != null)
            {
                panels.Expire();
                panels = null;
            }
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    Scheduler.AddOnce(updateSearch);
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
