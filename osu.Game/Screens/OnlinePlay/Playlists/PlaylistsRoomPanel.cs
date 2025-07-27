// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    /// <summary>
    /// A <see cref="RoomPanel"/> to be displayed in a playlists lobby.
    /// </summary>
    public partial class PlaylistsRoomPanel : RoomPanel
    {
        public new required Bindable<PlaylistItem?> SelectedItem
        {
            get => selectedItem.Current;
            set => selectedItem.Current = value;
        }

        private readonly BindableWithCurrent<PlaylistItem?> selectedItem = new BindableWithCurrent<PlaylistItem?>();

        public PlaylistsRoomPanel(Room room)
            : base(room)
        {
            base.SelectedItem.BindTo(SelectedItem);
        }

        protected override UpdateableBeatmapBackgroundSprite CreateBackground() => base.CreateBackground().With(d =>
        {
            d.BackgroundLoadDelay = 0;
        });
    }
}
