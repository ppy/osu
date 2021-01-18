// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Overlays
{
    public class DashboardOverlay : OnlineOverlay<DashboardOverlayHeader>
    {
        private CancellationTokenSource cancellationToken;

        public DashboardOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Header.Current.BindValueChanged(onTabChanged);
        }

        protected override DashboardOverlayHeader CreateHeader() => new DashboardOverlayHeader();

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
            ScrollFlow.ScrollToStart();

            LoadComponentAsync(display, loaded =>
            {
                if (API.IsLoggedIn)
                    Loading.Hide();

                Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private void onTabChanged(ValueChangedEvent<DashboardOverlayTabs> tab)
        {
            cancellationToken?.Cancel();
            Loading.Show();

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
