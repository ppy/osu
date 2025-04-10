// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
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

        /// <summary>
        /// Invoked when the user requests to edit an item.
        /// </summary>
        public Action<PlaylistItem>? RequestEdit;

        /// <summary>
        /// Invoked when the user requests to view the results for an item.
        /// </summary>
        public Action<PlaylistItem>? RequestResults;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private MultiplayerPlaylistTabControl playlistTabControl = null!;
        private MultiplayerQueueList queueList = null!;
        private MultiplayerHistoryList historyList = null!;
        private bool firstPopulation = true;

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
                        queueList = new MultiplayerQueueList
                        {
                            RelativeSizeAxes = Axes.Both,
                            RequestEdit = item => RequestEdit?.Invoke(item)
                        },
                        historyList = new MultiplayerHistoryList
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            RequestResults = item => RequestResults?.Invoke(item)
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

            PlaylistItem? currentItem = client.Room == null ? null : new PlaylistItem(client.Room.CurrentPlaylistItem);
            queueList.SelectedItem.Value = currentItem;
            historyList.SelectedItem.Value = currentItem;
        }

        private void playlistItemAdded(MultiplayerPlaylistItem item) => Scheduler.Add(() => addItemToLists(item));

        private void playlistItemRemoved(long item) => Scheduler.Add(() => removeItemFromLists(item));

        private void playlistItemChanged(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            if (client.Room == null)
                return;

            var existingItem = queueList.Items.SingleOrDefault(i => i.ID == item.ID);

            // Test if the only change between the two playlist items is the order.
            if (existingItem != null && existingItem.With(playlistOrder: item.PlaylistOrder).Equals(new PlaylistItem(item)))
            {
                // Set the new order directly and refresh the flow layout as an optimisation to avoid refreshing the items' visual state.
                existingItem.PlaylistOrder = item.PlaylistOrder;
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
            if (client.Room == null)
                return;

            if (item.Expired)
                historyList.Items.Add(new PlaylistItem(item));
            else
                queueList.Items.Add(new PlaylistItem(item));
        }

        private void removeItemFromLists(long itemId)
        {
            if (client.Room == null)
                return;

            queueList.Items.RemoveAll(i => i.ID == itemId);
            historyList.Items.RemoveAll(i => i.ID == itemId);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.ItemAdded -= playlistItemAdded;
                client.ItemRemoved -= playlistItemRemoved;
                client.ItemChanged -= playlistItemChanged;
                client.RoomUpdated -= onRoomUpdated;
            }
        }
    }
}
