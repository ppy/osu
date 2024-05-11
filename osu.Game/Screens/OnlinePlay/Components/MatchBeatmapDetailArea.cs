// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class MatchBeatmapDetailArea : BeatmapDetailArea
    {
        public Action CreateNewItem;

        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        private readonly GridContainer playlistArea;
        private readonly DrawableRoomPlaylist playlist;

        public MatchBeatmapDetailArea()
        {
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

            playlist.Items.BindTo(Playlist);
            playlist.SelectedItem.BindTo(SelectedItem);
        }

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
    }
}
