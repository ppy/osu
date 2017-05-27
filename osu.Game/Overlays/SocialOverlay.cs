// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Overlays.SearchableList;
using osu.Game.Overlays.Social;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public class SocialOverlay : SearchableListOverlay<SocialTab,SocialSortCriteria,SortDirection>
    {
        private readonly FillFlowContainer<UserPanel> panelFlow;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"5c2648");

        protected override SearchableListHeader<SocialTab> CreateHeader() => new Header();
        protected override SearchableListFilterControl<SocialSortCriteria,SortDirection> CreateFilterControl() => new FilterControl();

        private IEnumerable<User> users;
        public IEnumerable<User> Users
        {
            get { return users; }
            set
            {
                if (users?.Equals(value) ?? false) return;
                users = value;

                panelFlow.Children = users.Select(u =>
                {
                    var p = new UserPanel(u) { Width = 300 };
                    p.Status.BindTo(u.Status);
                    return p;
                });
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
                panelFlow = new FillFlowContainer<UserPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Top = 20 },
                    Spacing = new Vector2(10f),
                },
            };
        }
    }

    public enum SortDirection
    {
        Ascending,
        Descending,
    }
}
