// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Overlays.Browse;
using osu.Game.Overlays.Social;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public class SocialOverlay : BrowseOverlay<SocialTab, SocialSortCriteria>
    {
        private readonly FillFlowContainer<UserPanel> panelFlow;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"5c2648");

        protected override BrowseFilterControl<SocialSortCriteria> CreateFilterControl() => new FilterControl();
        protected override BrowseHeader<SocialTab> CreateHeader() => new Header();

        private IEnumerable<User> users;
        public IEnumerable<User> Users
        {
            get { return users; }
            set
            {
                if (users == value) return;
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
            ScrollFlow.Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Vertical = 10 },
                    Children = new[]
                    {
                        new DisplayStyleControl<SortDirection>
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                    },
                },
                panelFlow = new FillFlowContainer<UserPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
