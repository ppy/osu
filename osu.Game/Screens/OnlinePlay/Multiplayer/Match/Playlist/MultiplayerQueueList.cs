// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
    public class MultiplayerQueueList : DrawableRoomPlaylist
    {
        public MultiplayerQueueList()
        {
            ShowItemOwners = true;
        }

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new QueueFillFlowContainer
        {
            Spacing = new Vector2(0, 2)
        };

        protected override DrawableRoomPlaylistItem CreateDrawablePlaylistItem(PlaylistItem item) => new QueuePlaylistItem(item);

        private class QueueFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            [Resolved(typeof(Room), nameof(Room.Playlist))]
            private BindableList<PlaylistItem> roomPlaylist { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                roomPlaylist.BindCollectionChanged((_, __) => InvalidateLayout());
            }

            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.OfType<RearrangeableListItem<PlaylistItem>>().OrderBy(item => item.Model.PlaylistOrder);
        }

        private class QueuePlaylistItem : DrawableRoomPlaylistItem
        {
            [Resolved]
            private IAPIProvider api { get; set; }

            [Resolved]
            private MultiplayerClient multiplayerClient { get; set; }

            public QueuePlaylistItem(PlaylistItem item)
                : base(item)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RequestDeletion = item =>
                {
                    // If the requested item is the currently selected one, we proceed to a swap because we can't delete the current item
                    if (IsSelectedItem && multiplayerClient.Room?.Playlist.Count > 1)
                    {
                        // We delete the second item which we cache first then we update the first one with the cache
                        var secondItem = multiplayerClient.Room.Playlist[1];
                        var replacingItem = new MultiplayerPlaylistItem
                        {
                            ID = item.ID,
                            BeatmapID = secondItem.BeatmapID,
                            BeatmapChecksum = secondItem.BeatmapChecksum,
                            RulesetID = secondItem.RulesetID,
                            RequiredMods = secondItem.RequiredMods,
                            AllowedMods = secondItem.AllowedMods
                        };

                        multiplayerClient.RemovePlaylistItem(secondItem.ID).FireAndForget(onSuccess: () =>
                        {
                            multiplayerClient.EditPlaylistItem(replacingItem).FireAndForget();
                        });
                        return;
                    }
                    // Normal case
                    multiplayerClient.RemovePlaylistItem(item.ID).FireAndForget();
                };

                multiplayerClient.RoomUpdated += onRoomUpdated;
                onRoomUpdated();
            }

            private void onRoomUpdated() => Scheduler.AddOnce(updateDeleteButtonVisibility);

            private void updateDeleteButtonVisibility()
            {
                if (multiplayerClient.Room == null)
                    return;

                bool isItemOwner = Item.OwnerID == api.LocalUser.Value.OnlineID || multiplayerClient.IsHost;

                AllowDeletion = isItemOwner && !Item.Expired;
                AllowEditing = isItemOwner && !Item.Expired;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (multiplayerClient != null)
                    multiplayerClient.RoomUpdated -= onRoomUpdated;
            }
        }
    }
}
