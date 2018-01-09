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
using osu.Game.Graphics.Cursor;
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
        private readonly FillFlowContainer<UserPanel> panelFlow;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"5c2648");

        protected override SearchableListHeader<SocialTab> CreateHeader() => new Header();
        protected override SearchableListFilterControl<SocialSortCriteria, SortDirection> CreateFilterControl() => new FilterControl();

        private IEnumerable<User> users;
        private readonly LoadingAnimation loading;

        public IEnumerable<User> Users
        {
            get { return users; }
            set
            {
                if (users?.Equals(value) ?? false) return;
                users = value;

                if (users == null)
                    panelFlow.Clear();
                else
                {
                    panelFlow.ChildrenEnumerable = users.Select(u =>
                    {
                        var p = new UserPanel(u) { Width = 300 };
                        p.Status.BindTo(u.Status);
                        return p;
                    });
                }
            }
        }

        public SocialOverlay()
        {
            FirstWaveColour = OsuColour.FromHex(@"cb5fa0");
            SecondWaveColour = OsuColour.FromHex(@"b04384");
            ThirdWaveColour = OsuColour.FromHex(@"9b2b6e");
            FourthWaveColour = OsuColour.FromHex(@"6d214d");

            ScrollFlow.Children = new[]
            {
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = panelFlow = new FillFlowContainer<UserPanel>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = 20 },
                        Spacing = new Vector2(10f),
                    }
                },
            };

            Add(loading = new LoadingAnimation());
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            if (Users == null)
                reloadUsers(api);
        }

        private void reloadUsers(APIAccess api)
        {
            Users = null;

            // no this is not the correct data source, but it's something.
            var request = new GetUsersRequest();
            request.Success += res =>
            {
                Users = res.Select(e => e.User);
                loading.Hide();
            };

            api.Queue(request);
            loading.Show();
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    reloadUsers(api);
                    break;
                default:
                    Users = null;
                    break;
            }
        }
    }

    public enum SortDirection
    {
        Descending,
        Ascending,
    }
}
