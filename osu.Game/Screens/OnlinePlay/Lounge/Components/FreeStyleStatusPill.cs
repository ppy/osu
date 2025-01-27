// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class FreeStyleStatusPill : OnlinePlayPill
    {
        private readonly Room room;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override FontUsage Font => base.Font.With(weight: FontWeight.SemiBold);

        public FreeStyleStatusPill(Room room)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Pill.Background.Alpha = 1;
            Pill.Background.Colour = colours.Yellow;

            TextFlow.Text = "Freestyle";
            TextFlow.Colour = Color4.Black;

            room.PropertyChanged += onRoomPropertyChanged;
            updateFreeStyleStatus();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.CurrentPlaylistItem):
                case nameof(Room.Playlist):
                    updateFreeStyleStatus();
                    break;
            }
        }

        private void updateFreeStyleStatus()
        {
            PlaylistItem? currentItem = room.Playlist.GetCurrentItem() ?? room.CurrentPlaylistItem;
            Alpha = currentItem?.FreeStyle == true ? 1 : 0;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
