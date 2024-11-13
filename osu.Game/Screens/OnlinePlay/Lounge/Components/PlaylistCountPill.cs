// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// A pill that displays the playlist item count.
    /// </summary>
    public partial class PlaylistCountPill : OnlinePlayPill
    {
        private readonly Room room;

        public PlaylistCountPill(Room room)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged((_, _) => updateCount());

            room.PropertyChanged += onRoomPropertyChanged;
            updateCount();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.PlaylistItemStats))
                updateCount();
        }

        private void updateCount()
        {
            int activeItems = Playlist.Count > 0 || room.PlaylistItemStats == null
                // For now, use the playlist as the source of truth if it has any items.
                // This allows the count to display correctly on the room screen (after joining a room).
                ? Playlist.Count(i => !i.Expired)
                : room.PlaylistItemStats.CountActive;

            TextFlow.Clear();
            TextFlow.AddText(activeItems.ToLocalisableString(), s => s.Font = s.Font.With(weight: FontWeight.Bold));
            TextFlow.AddText(" ");
            TextFlow.AddText("Beatmap".ToQuantity(activeItems, ShowQuantityAs.None));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
