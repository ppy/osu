// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay.Playlists;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonPlaylistV2 : ScreenFooterButton, IHasPopover
    {
        private readonly Room room;

        public FooterButtonPlaylistV2(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Text = "Playlist";
            Icon = FontAwesome.Solid.List;
            AccentColour = colour.Purple1;

            Action = this.ShowPopover;
        }

        public Popover GetPopover() => new PlaylistPopover(room);

        private partial class PlaylistPopover : OsuPopover
        {
            private readonly Room room;
            private PlaylistsRoomSettingsPlaylist playlist = null!;

            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

            public PlaylistPopover(Room room)
            {
                this.room = room;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Content.Padding = new MarginPadding(10);

                Child = playlist = new PlaylistsRoomSettingsPlaylist
                {
                    Size = new Vector2(300)
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playlist.Items.BindCollectionChanged((_, __) => room.Playlist = playlist.Items.ToArray());

                room.PropertyChanged += onRoomPropertyChanged;
                updateRoomPlaylist();
            }

            private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(Room.Playlist))
                    updateRoomPlaylist();
            }

            private void updateRoomPlaylist()
                => playlist.Items.ReplaceRange(0, playlist.Items.Count, room.Playlist);
        }
    }
}
