// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Lounge
{
    public partial class LoungeBackgroundScreen : OnlinePlayBackgroundScreen
    {
        public readonly Bindable<Room?> SelectedRoom = new Bindable<Room?>();
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();

        public LoungeBackgroundScreen()
        {
            SelectedRoom.BindValueChanged(onSelectedRoomChanged);
            playlist.BindCollectionChanged((_, _) => PlaylistItem = playlist.GetCurrentItem());
        }

        private void onSelectedRoomChanged(ValueChangedEvent<Room?> room)
        {
            if (room.OldValue != null)
                playlist.UnbindFrom(room.OldValue.Playlist);

            if (room.NewValue != null)
                playlist.BindTo(room.NewValue.Playlist);
            else
                playlist.Clear();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // This screen never exits.
            return true;
        }
    }
}
