// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerRoomBackgroundScreen : OnlinePlayBackgroundScreen
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private long lastPlaylistItemId;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.SettingsChanged += onSettingsChanged;
            client.ItemChanged += onPlaylistItemChanged;

            updateBackground();
        }

        private void onPlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            if (item.ID == client.Room?.Settings.PlaylistItemId)
                updateBackground();
        }

        private void onSettingsChanged(MultiplayerRoomSettings settings)
        {
            if (settings.PlaylistItemId != lastPlaylistItemId)
            {
                updateBackground();
                lastPlaylistItemId = settings.PlaylistItemId;
            }
        }

        private void updateBackground()
        {
            if (client.Room == null)
                return;

            PlaylistItem = new PlaylistItem(client.Room.Playlist.Single(i => i.ID == client.Room.Settings.PlaylistItemId));
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
