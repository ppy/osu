// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Overlays
{
    public class DashboardOverlay : FullscreenOverlay<DashboardOverlayHeader>
    {
        private CancellationTokenSource cancellationToken;

        private Container content;
        private LoadingLayer loading;
        private OverlayScrollContainer scrollFlow;

        public DashboardOverlay()
            : base(OverlayColourScheme.Purple, new DashboardOverlayHeader
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Depth = -float.MaxValue
            })
        {
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background5
                },
                scrollFlow = new OverlayScrollContainer
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
                            Header,
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Header.Current.BindValueChanged(onTabChanged);
        }

        private bool displayUpdateRequired = true;

        protected override void PopIn()
        {
            base.PopIn();

            // We don't want to create a new display on every call, only when exiting from fully closed state.
            if (displayUpdateRequired)
            {
                Header.Current.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            loadDisplay(Empty());
            displayUpdateRequired = true;
        }

        private void loadDisplay(Drawable display)
        {
            scrollFlow.ScrollToStart();

            LoadComponentAsync(display, loaded =>
            {
                if (API.IsLoggedIn)
                    loading.Hide();

                content.Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private void onTabChanged(ValueChangedEvent<DashboardOverlayTabs> tab)
        {
            cancellationToken?.Cancel();
            loading.Show();

            if (!API.IsLoggedIn)
            {
                loadDisplay(Empty());
                return;
            }

            switch (tab.NewValue)
            {
                case DashboardOverlayTabs.Friends:
                    loadDisplay(new FriendDisplay());
                    break;

                case DashboardOverlayTabs.CurrentlyPlaying:
                    loadDisplay(new CurrentlyPlayingDisplay());
                    break;

                default:
                    throw new NotImplementedException($"Display for {tab.NewValue} tab is not implemented");
            }
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            if (State.Value == Visibility.Hidden)
                return;

            Header.Current.TriggerChange();
        });

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
