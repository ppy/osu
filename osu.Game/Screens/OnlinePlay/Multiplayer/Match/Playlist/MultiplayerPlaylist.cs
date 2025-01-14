// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    /// <summary>
    /// The multiplayer playlist, containing lists to show the items from a <see cref="MultiplayerRoom"/> in both gameplay-order and historical-order.
    /// </summary>
    public partial class MultiplayerPlaylist : CompositeDrawable
    {
        public readonly Bindable<MultiplayerPlaylistDisplayMode> DisplayMode = new Bindable<MultiplayerPlaylistDisplayMode>();

        public required Bindable<PlaylistItem?> SelectedItem
        {
            get => selectedItem;
            set => selectedItem.Current = value;
        }

        /// <summary>
        /// Invoked when an item requests to be edited.
        /// </summary>
        public Action<PlaylistItem>? RequestEdit;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly Room room;
        private readonly BindableWithCurrent<PlaylistItem?> selectedItem = new BindableWithCurrent<PlaylistItem?>();
        private MultiplayerPlaylistTabControl playlistTabControl = null!;
        private MultiplayerQueueList queueList = null!;
        private MultiplayerHistoryList historyList = null!;
        private bool firstPopulation = true;

        public MultiplayerPlaylist(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float tab_control_height = 25;

            InternalChildren = new Drawable[]
            {
                playlistTabControl = new MultiplayerPlaylistTabControl
                {
                    RelativeSizeAxes = Axes.X,
                    Height = tab_control_height,
                    Current = { BindTarget = DisplayMode }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = tab_control_height + 5 },
                    Masking = true,
                    Children = new Drawable[]
                    {
                        queueList = new MultiplayerQueueList(room)
                        {
                            RelativeSizeAxes = Axes.Both,
                            SelectedItem = { BindTarget = selectedItem },
                            RequestEdit = item => RequestEdit?.Invoke(item)
                        },
                        historyList = new MultiplayerHistoryList
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            SelectedItem = { BindTarget = selectedItem }
                        }
                    }
                }
            };

            playlistTabControl.QueueItems.BindTarget = queueList.Items;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayMode.BindValueChanged(onDisplayModeChanged, true);
            client.ItemAdded += playlistItemAdded;
            client.ItemRemoved += playlistItemRemoved;
            client.ItemChanged += playlistItemChanged;
            client.RoomUpdated += onRoomUpdated;
            updateState();
        }

        private void onDisplayModeChanged(ValueChangedEvent<MultiplayerPlaylistDisplayMode> mode)
        {
            historyList.FadeTo(mode.NewValue == MultiplayerPlaylistDisplayMode.History ? 1 : 0, 100);
            queueList.FadeTo(mode.NewValue == MultiplayerPlaylistDisplayMode.Queue ? 1 : 0, 100);
        }

        private void onRoomUpdated() => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            if (client.Room == null)
            {
                historyList.Items.Clear();
                queueList.Items.Clear();
                firstPopulation = true;
                return;
            }

            if (firstPopulation)
            {
                foreach (var item in client.Room.Playlist)
                    addItemToLists(item);

                firstPopulation = false;
            }
        }

        private void playlistItemAdded(MultiplayerPlaylistItem item) => Schedule(() => addItemToLists(item));

        private void playlistItemRemoved(long item) => Schedule(() => removeItemFromLists(item));

        private void playlistItemChanged(MultiplayerPlaylistItem item) => Schedule(() =>
        {
            if (client.Room == null)
                return;

            var newApiItem = new PlaylistItem(item);
            var existingApiItemInQueue = queueList.Items.SingleOrDefault(i => i.ID == item.ID);

            // Test if the only change between the two playlist items is the order.
            if (existingApiItemInQueue != null && existingApiItemInQueue.With(playlistOrder: newApiItem.PlaylistOrder).Equals(newApiItem))
            {
                // Set the new playlist order directly without refreshing the DrawablePlaylistItem.
                existingApiItemInQueue.PlaylistOrder = newApiItem.PlaylistOrder;

                // The following isn't really required, but is here for safety and explicitness.
                // MultiplayerQueueList internally binds to changes in Playlist to invalidate its own layout, which is mutated on every playlist operation.
                queueList.Invalidate();
            }
            else
            {
                removeItemFromLists(item.ID);
                addItemToLists(item);
            }
        });

        private void addItemToLists(MultiplayerPlaylistItem item)
        {
            var apiItem = client.Room?.Playlist.SingleOrDefault(i => i.ID == item.ID);

            // Item could have been removed from the playlist while the local player was in gameplay.
            if (apiItem == null)
                return;

            if (item.Expired)
                historyList.Items.Add(new PlaylistItem(apiItem));
            else
                queueList.Items.Add(new PlaylistItem(apiItem));
        }

        private void removeItemFromLists(long item)
        {
            queueList.Items.RemoveAll(i => i.ID == item);
            historyList.Items.RemoveAll(i => i.ID == item);
        }
    }
}
