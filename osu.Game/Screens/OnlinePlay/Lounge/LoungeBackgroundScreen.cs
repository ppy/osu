// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using PlaylistItem = osu.Game.Online.Rooms.PlaylistItem;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    public class LoungeBackgroundScreen : OnlinePlayBackgroundScreen
    {
        public readonly Bindable<Room> SelectedRoom = new Bindable<Room>();
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();

        public LoungeBackgroundScreen()
        {
            SelectedRoom.BindValueChanged(onSelectedRoomChanged);
            playlist.BindCollectionChanged((_, __) => PlaylistItem = playlist.GetCurrentItem());
        }

        private void onSelectedRoomChanged(ValueChangedEvent<Room> room)
        {
            if (room.OldValue != null)
                playlist.UnbindFrom(room.OldValue.Playlist);

            if (room.NewValue != null)
                playlist.BindTo(room.NewValue.Playlist);
            else
                playlist.Clear();
        }

        public override bool OnExiting(IScreen next)
        {
            // This screen never exits.
            return true;
        }
    }
}
