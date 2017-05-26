// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Browse;
using osu.Game.Overlays.Social;

using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays
{
    public class SocialOverlay : BrowseOverlay<SocialTab, SocialSortCriteria>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"5c2648");

        protected override BrowseFilterControl<SocialSortCriteria> CreateFilterControl() => new FilterControl();
        protected override BrowseHeader<SocialTab> CreateHeader() => new Header();

        public SocialOverlay()
        {
            ScrollFlow.Children = new[]
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
            };
        }
    }

    public enum SortDirection
    {
        Ascending,
        Descending,
    }
}
