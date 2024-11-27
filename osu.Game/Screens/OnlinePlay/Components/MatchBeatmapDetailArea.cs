// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Select;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class MatchBeatmapDetailArea : BeatmapDetailArea
    {
        public Action? CreateNewItem;

        private readonly Room room;
        private readonly GridContainer playlistArea;
        private readonly DrawableRoomPlaylist playlist;

        public MatchBeatmapDetailArea(Room room)
        {
            this.room = room;

            Add(playlistArea = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
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

        protected override void OnTabChanged(BeatmapDetailAreaTabItem tab, bool selectedMods)
        {
            base.OnTabChanged(tab, selectedMods);

            switch (tab)
            {
                case BeatmapDetailAreaPlaylistTabItem:
                    playlistArea.Show();
                    break;

                default:
                    playlistArea.Hide();
                    break;
            }
        }

        protected override BeatmapDetailAreaTabItem[] CreateTabItems() => base.CreateTabItems().Prepend(new BeatmapDetailAreaPlaylistTabItem()).ToArray();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
