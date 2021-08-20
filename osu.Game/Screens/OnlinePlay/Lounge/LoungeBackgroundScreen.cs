// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using PlaylistItem = osu.Game.Online.Rooms.PlaylistItem;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    public class LoungeBackgroundScreen : OnlinePlayBackgroundScreen
    {
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();

        public LoungeBackgroundScreen()
        {
            playlist.BindCollectionChanged((_, __) => PlaylistItem = playlist.FirstOrDefault());
        }

        private Room? selectedRoom;

        public Room? SelectedRoom
        {
            get => selectedRoom;
            set
            {
                if (selectedRoom == value)
                    return;

                if (selectedRoom != null)
                    playlist.UnbindFrom(selectedRoom.Playlist);

                selectedRoom = value;

                if (selectedRoom != null)
                    playlist.BindTo(selectedRoom.Playlist);
                else
                    playlist.Clear();
            }
        }
    }
}
