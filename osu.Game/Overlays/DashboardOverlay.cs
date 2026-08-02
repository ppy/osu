// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.CurrentlyOnline;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Overlays.Dashboard.UserSearch;

namespace osu.Game.Overlays
{
    public partial class DashboardOverlay : TabbableOnlineOverlay<DashboardOverlayHeader, DashboardOverlayTabs>
    {
        private readonly BindableBool loading = new BindableBool();

        public DashboardOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loading.BindValueChanged(loading =>
            {
                if (loading.NewValue)
                    Loading.Show();
                else
                    Loading.Hide();
            }, true);
        }

        protected override DashboardOverlayHeader CreateHeader() => new DashboardOverlayHeader();

        public override bool AcceptsFocus => false;

        protected override void CreateDisplayToLoad(DashboardOverlayTabs tab)
        {
            switch (tab)
            {
                case DashboardOverlayTabs.Friends:
                    LoadDisplay(new FriendDisplay
                    {
                        Loading = { BindTarget = loading },
                    });
                    break;

                case DashboardOverlayTabs.CurrentlyPlaying:
                    LoadDisplay(new CurrentlyOnlineDisplay
                    {
                        Loading = { BindTarget = loading },
                        OverlayState = { BindTarget = State }
                    });
                    break;

                case DashboardOverlayTabs.UserSearch:
                    LoadDisplay(new UserSearchDisplay
                    {
                        Loading = { BindTarget = loading },
                    });
                    break;

                default:
                    throw new NotImplementedException($"Display for {tab} tab is not implemented");
            }
        }
    }
}
