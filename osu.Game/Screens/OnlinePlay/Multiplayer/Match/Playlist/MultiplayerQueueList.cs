// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    /// <summary>
    /// A gameplay-ordered list of <see cref="DrawableRoomPlaylistItem"/>s.
    /// </summary>
    public partial class MultiplayerQueueList : DrawableRoomPlaylist
    {
        private readonly Room room;

        private QueueFillFlowContainer flow = null!;

        public MultiplayerQueueList(Room room)
        {
            this.room = room;
            ShowItemOwners = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateRoomPlaylist();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Playlist))
                updateRoomPlaylist();
        }

        private void updateRoomPlaylist()
            => flow.InvalidateLayout();

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => flow = new QueueFillFlowContainer
        {
            Spacing = new Vector2(0, 2)
        };

        protected override DrawableRoomPlaylistItem CreateDrawablePlaylistItem(PlaylistItem item) => new QueuePlaylistItem(item);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }

        private partial class QueueFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            public new void InvalidateLayout() => base.InvalidateLayout();

            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.OfType<RearrangeableListItem<PlaylistItem>>().OrderBy(item => item.Model.PlaylistOrder);
        }

        private partial class QueuePlaylistItem : DrawableRoomPlaylistItem
        {
            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            [Resolved]
            private MultiplayerClient multiplayerClient { get; set; } = null!;

            public QueuePlaylistItem(PlaylistItem item)
                : base(item)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RequestDeletion = item => multiplayerClient.RemovePlaylistItem(item.ID).FireAndForget();

                multiplayerClient.RoomUpdated += onRoomUpdated;
                onRoomUpdated();
            }

            private void onRoomUpdated() => Scheduler.AddOnce(updateDeleteButtonVisibility);

            private void updateDeleteButtonVisibility()
            {
                if (multiplayerClient.Room == null)
                    return;

                bool isItemOwner = Item.OwnerID == api.LocalUser.Value.OnlineID || multiplayerClient.IsHost;
                bool isValidItem = isItemOwner && !Item.Expired;

                AllowDeletion = isValidItem
                                && (Item.ID != multiplayerClient.Room.Settings.PlaylistItemId // This is an optimisation for the following check.
                                    || multiplayerClient.Room.Playlist.Count(i => !i.Expired) > 1);

                AllowEditing = isValidItem;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (multiplayerClient.IsNotNull())
                    multiplayerClient.RoomUpdated -= onRoomUpdated;
            }
        }
    }
}
