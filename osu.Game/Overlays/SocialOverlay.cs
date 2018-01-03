// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

namespace osu.Game.Overlays
{
    public class SocialOverlay : SearchableListOverlay<SocialTab, SocialSortCriteria, SortDirection>, IOnlineComponent
    {
        private APIAccess api;
        private readonly LoadingAnimation loading;
        private FillFlowContainer<UserPanel> panels;

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
                if (text != string.Empty)
                {
                    Header.Tabs.Current.Value = SocialTab.Search;

                    if (Filter.Tabs.Current.Value == SocialSortCriteria.Rank)
                        Filter.Tabs.Current.Value = SocialSortCriteria.Relevance;
                }
                else
                {
                    Header.Tabs.Current.Value = SocialTab.OnlinePlayers;

                    if (Filter.Tabs.Current.Value == SocialSortCriteria.Relevance)
                        Filter.Tabs.Current.Value = SocialSortCriteria.Rank;
                }
            };

            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += recreatePanels;

            // TODO sort our list in some way (either locally or with API call)
            //Filter.DisplayStyleControl.Dropdown.Current.ValueChanged += sortOrder => Scheduler.AddOnce(updateSearch);

            Header.Tabs.Current.ValueChanged += tab =>
            {
                if (tab != SocialTab.Search)
                {
                    //currentQuery.Value = string.Empty;
                    Filter.Tabs.Current.Value = (SocialSortCriteria)Header.Tabs.Current.Value;
                    Scheduler.AddOnce(updateSearch);
                }
            };

            //currentQuery.ValueChanged += v =>
            //{
            //    queryChangedDebounce?.Cancel();

            //    if (string.IsNullOrEmpty(v))
            //        Scheduler.AddOnce(updateSearch);
            //    else
            //        queryChangedDebounce = Scheduler.AddDelayed(updateSearch, 500);
            //};

            //currentQuery.BindTo(Filter.Search.Current);

            Filter.Tabs.Current.ValueChanged += sortCriteria =>
            {
                if (Header.Tabs.Current.Value != SocialTab.Search && sortCriteria != (SocialSortCriteria)Header.Tabs.Current.Value)
                    Header.Tabs.Current.Value = SocialTab.Search;

                Scheduler.AddOnce(updateSearch);
            };
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

            var newPanels = new FillFlowContainer<UserPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10f),
                Margin = new MarginPadding { Top = 10 },
                ChildrenEnumerable = Users.Select(u =>
                {
                    UserPanel panel = new UserPanel(u)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    };
                    switch (displayStyle)
                    {
                        case PanelDisplayStyle.Grid:
                            panel.Width = 300;
                            break;
                        default:
                            panel.RelativeSizeAxes = Axes.X;
                            break;
                    }
                    panel.Status.BindTo(u.Status);
                    return panel;
                })
            };

            LoadComponentAsync(newPanels, p => ScrollFlow.Add(panels = newPanels));
        }

        private void clearPanels()
        {
            if (panels != null)
            {
                ScrollFlow.Remove(panels);
                panels.Expire();
                panels = null;
            }
        }

        private APIRequest getUsersRequest;

        //private readonly Bindable<string> currentQuery = new Bindable<string>();

        //private ScheduledDelegate queryChangedDebounce;

        private void updateSearch()
        {
            //queryChangedDebounce?.Cancel();

            if (!IsLoaded)
                return;

            Users = null;
            clearPanels();
            loading.Hide();
            getUsersRequest?.Cancel();

            if (api?.IsLoggedIn == false)
                return;

            switch (Header.Tabs.Current.Value)
            {
                case SocialTab.OnlineFriends:
                    var friendRequest = new GetFriendsRequest(); // TODO filter???
                    friendRequest.Success += updateUsers;
                    api.Queue(getUsersRequest = friendRequest);
                    break;
                default:
                    var userRequest = new GetUsersRequest(); // TODO filter???
                    userRequest.Success += response => updateUsers(response.Select(r => r.User));
                    api.Queue(getUsersRequest = userRequest);
                    break;
            }
            loading.Show();
        }

        private void updateUsers(IEnumerable<User> newUsers)
        {
            Schedule(() =>
            {
                Users = newUsers;
                loading.Hide();
                recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
            });
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
                    recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
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
