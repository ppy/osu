// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Home;
using osu.Game.Overlays.Home.Friends;

namespace osu.Game.Overlays
{
    public class HomeOverlay : FullscreenOverlay
    {
        private readonly Box background;
        private readonly HomeOverlayHeader header;
        private readonly Container content;

        public HomeOverlay()
            : base(OverlayColourScheme.Purple)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new HomeOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue
                            },
                            content = new Container()
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            header.Current.BindValueChanged(onTabChanged);
        }

        private void onTabChanged(ValueChangedEvent<HomeOverlayTabs> tab)
        {
            switch (tab.NewValue)
            {
                default:
                    loadLayout(null);
                    return;

                case HomeOverlayTabs.Friends:
                    loadLayout(new FriendsLayout());
                    return;
            }
        }

        private void loadLayout(Drawable layout)
        {
            content.Clear();

            if (layout != null)
                content.Add(layout);
        }
    }
}
