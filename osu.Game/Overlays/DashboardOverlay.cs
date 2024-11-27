// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Metadata;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Overlays
{
    public partial class DashboardOverlay : TabbableOnlineOverlay<DashboardOverlayHeader, DashboardOverlayTabs>
    {
        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        private IBindable<bool> metadataConnected = null!;

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
                    LoadDisplay(new CurrentlyOnlineDisplay());
                    break;

                default:
                    throw new NotImplementedException($"Display for {tab} tab is not implemented");
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            metadataConnected = metadataClient.IsConnected.GetBoundCopy();
            metadataConnected.BindValueChanged(_ => updateUserPresenceState());
            State.BindValueChanged(_ => updateUserPresenceState());
            updateUserPresenceState();
        }

        private void updateUserPresenceState()
        {
            if (!metadataConnected.Value)
                return;

            if (State.Value == Visibility.Visible)
                metadataClient.BeginWatchingUserPresence().FireAndForget();
            else
                metadataClient.EndWatchingUserPresence().FireAndForget();
        }
    }
}
