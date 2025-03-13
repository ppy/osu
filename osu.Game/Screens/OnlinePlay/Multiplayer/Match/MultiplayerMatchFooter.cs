// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerMatchFooter : CompositeDrawable
    {
        private const float ready_button_width = 600;
        private const float spectate_button_width = 200;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        // Todo: This bindable shouldn't exist - consider moving this class' logic into each component.
        private readonly Bindable<PlaylistItem?> selectedItem = new Bindable<PlaylistItem?>();

        private long lastPlaylistItemId;

        public MultiplayerMatchFooter()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable?[]
                    {
                        null,
                        new MultiplayerSpectateButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            SelectedItem = selectedItem
                        },
                        null,
                        new MatchStartControl
                        {
                            RelativeSizeAxes = Axes.Both,
                            SelectedItem = selectedItem
                        },
                        null
                    }
                },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(maxSize: spectate_button_width),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(maxSize: ready_button_width),
                    new Dimension()
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.SettingsChanged += onSettingsChanged;
            client.ItemChanged += onPlaylistItemChanged;

            updateSelectedItem();
        }

        private void onPlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            if (item.ID == client.Room?.Settings.PlaylistItemId)
                updateSelectedItem();
        }

        private void onSettingsChanged(MultiplayerRoomSettings settings)
        {
            if (settings.PlaylistItemId != lastPlaylistItemId)
            {
                updateSelectedItem();
                lastPlaylistItemId = settings.PlaylistItemId;
            }
        }

        private void updateSelectedItem()
        {
            if (client.Room == null)
                return;

            selectedItem.Value = new PlaylistItem(client.Room.Playlist.Single(i => i.ID == client.Room.Settings.PlaylistItemId));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.SettingsChanged -= onSettingsChanged;
                client.ItemChanged -= onPlaylistItemChanged;
            }
        }
    }
}
