// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    /// <summary>
    /// A historically-ordered list of <see cref="DrawableRoomPlaylistItem"/>s.
    /// </summary>
    public partial class MultiplayerHistoryList : DrawableRoomPlaylist
    {
        public new Bindable<PlaylistItem?> SelectedItem => throw new NotSupportedException();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private bool firstPopulation = true;

        public MultiplayerHistoryList()
        {
            ShowItemOwners = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.ItemAdded += onItemAdded;
            client.ItemRemoved += onItemRemoved;
            client.ItemChanged += onItemChanged;

            onRoomUpdated();
        }

        private void onRoomUpdated()
        {
            if (client.Room == null)
            {
                Items.Clear();
                firstPopulation = true;
            }
            else if (firstPopulation)
            {
                foreach (var item in client.Room.Playlist)
                    onItemAdded(item);
                firstPopulation = false;
            }

            base.SelectedItem.Value = client.Room?.CurrentPlaylistItem == null ? null : new PlaylistItem(client.Room.CurrentPlaylistItem);
        }

        private void onItemAdded(MultiplayerPlaylistItem item)
        {
            if (item.Expired)
                Items.Add(new PlaylistItem(item));
        }

        private void onItemRemoved(long item)
        {
            Items.RemoveAll(i => i.ID == item);
        }

        private void onItemChanged(MultiplayerPlaylistItem item)
        {
            if (item.Expired)
                Items.Add(new PlaylistItem(item));
        }

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new HistoryFillFlowContainer
        {
            Spacing = new Vector2(0, 2)
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.ItemAdded -= onItemAdded;
                client.ItemRemoved -= onItemRemoved;
                client.ItemChanged -= onItemChanged;
            }
        }

        private partial class HistoryFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.OfType<RearrangeableListItem<PlaylistItem>>().OrderByDescending(item => item.Model.PlayedAt);
        }
    }
}
