// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class OverlinedPlaylistHeader : OverlinedHeader
    {
        private readonly Room room;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public OverlinedPlaylistHeader(Room room)
            : base("Playlist")
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateDuration();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Playlist))
                updateDuration();
        }

        private void updateDuration()
            => Details.Value = room.Playlist.GetTotalDuration(rulesets);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
