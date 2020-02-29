// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Home;
using osu.Game.Overlays.Home.Friends;

namespace osu.Game.Overlays
{
    public class HomeOverlay : FullscreenOverlay
    {
        private CancellationTokenSource cancellationToken;

        private readonly Box background;
        private readonly HomeOverlayHeader header;
        private readonly Container content;
        private readonly LoadingLayer loading;

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
                            content = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                },
                loading = new LoadingLayer(content),
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

            header.Current.BindValueChanged(onTabChanged, true);
        }

        private void onTabChanged(ValueChangedEvent<HomeOverlayTabs> tab)
        {
            loading.Show();
            cancellationToken?.Cancel();

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
            if (layout == null)
            {
                content.Clear();
                loading.Hide();
                return;
            }

            LoadComponentAsync(layout, loaded =>
            {
                content.Clear();
                content.Add(loaded);
                loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
