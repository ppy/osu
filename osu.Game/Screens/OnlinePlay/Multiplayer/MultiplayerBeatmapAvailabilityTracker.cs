// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerBeatmapAvailabilityTracker : OnlinePlayBeatmapAvailabilityTracker
    {
        public new Bindable<PlaylistItem> SelectedItem => throw new NotSupportedException();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private long lastPlaylistItemId;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.SettingsChanged += onSettingsChanged;
            client.ItemChanged += onPlaylistItemChanged;

            updateSelectedItem();
        }

        private void onPlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            if (item.ID == client.Room?.Settings.PlaylistItemId)
                updateSelectedItem();
        }

        private void onSettingsChanged(MultiplayerRoomSettings settings)
        {
            if (settings.PlaylistItemId != lastPlaylistItemId)
            {
                updateSelectedItem();
                lastPlaylistItemId = settings.PlaylistItemId;
            }
        }

        private void updateSelectedItem()
        {
            if (client.Room == null)
                return;

            base.SelectedItem.Value = new PlaylistItem(client.Room.Playlist.Single(i => i.ID == client.Room.Settings.PlaylistItemId));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.SettingsChanged -= onSettingsChanged;
                client.ItemChanged -= onPlaylistItemChanged;
            }
        }
    }
}
