// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            SelectedRoom.BindValueChanged(onSelectedRoomChanged, true);
        }

        private void onSelectedRoomChanged(ValueChangedEvent<Room?> room)
        {
            if (room.OldValue != null)
                room.OldValue.PropertyChanged -= onRoomPropertyChanged;

            if (room.NewValue != null)
                room.NewValue.PropertyChanged += onRoomPropertyChanged;

            updateCurrentItem();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Playlist))
                updateCurrentItem();
        }

        private void updateCurrentItem()
            => PlaylistItem = SelectedRoom.Value?.Playlist.GetCurrentItem();

        public override bool OnExiting(ScreenExitEvent e)
        {
            // This screen never exits.
            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (SelectedRoom.Value != null)
                SelectedRoom.Value.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
