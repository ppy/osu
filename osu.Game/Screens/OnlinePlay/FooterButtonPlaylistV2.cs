// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay.Playlists;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonPlaylistV2 : ScreenFooterButton, IHasPopover
    {
        public required Action? CreateNewItem { get; init; }

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

        public Popover GetPopover() => new PlaylistPopover(room)
        {
            CreateNewItem = CreateNewItem
        };

        private partial class PlaylistPopover : OsuPopover
        {
            public required Action? CreateNewItem { get; init; }

            private readonly Room room;
            private PlaylistsRoomSettingsPlaylist playlist = null!;

            public PlaylistPopover(Room room)
            {
                this.room = room;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Content.Padding = new MarginPadding(5);

                Add(new GridContainer
                {
                    Size = new Vector2(300, 300),
                    Padding = new MarginPadding { Vertical = 10 },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = 10 },
                                Child = playlist = new PlaylistsRoomSettingsPlaylist
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new RoundedButton
                            {
                                Text = "Add new playlist entry",
                                RelativeSizeAxes = Axes.Both,
                                Size = Vector2.One,
                                Action = () => CreateNewItem?.Invoke()
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 50),
                    }
                });
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
