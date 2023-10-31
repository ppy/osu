// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Overlays
{
    public partial class DashboardOverlay : TabbableOnlineOverlay<DashboardOverlayHeader, DashboardOverlayTabs>
    {
        public DashboardOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        protected override DashboardOverlayHeader CreateHeader() => new DashboardOverlayHeader();

        public override bool AcceptsFocus => false;

        protected override void CreateDisplayToLoad(DashboardOverlayTabs tab)
        {
            switch (tab)
            {
                case DashboardOverlayTabs.Friends:
                    LoadDisplay(new FriendDisplay());
                    break;

                case DashboardOverlayTabs.CurrentlyPlaying:
                    LoadDisplay(new CurrentlyPlayingDisplay());
                    break;

                default:
                    throw new NotImplementedException($"Display for {tab} tab is not implemented");
            }
        }
    }
}
