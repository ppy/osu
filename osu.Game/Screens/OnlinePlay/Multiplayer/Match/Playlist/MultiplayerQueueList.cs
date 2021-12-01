// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
            : base(false, false, true)
        {
        }

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new QueueFillFlowContainer
        {
            Spacing = new Vector2(0, 2)
        };

        private class QueueFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            [Resolved(typeof(Room), nameof(Room.QueueMode))]
            private Bindable<QueueMode> queueMode { get; set; }

            [Resolved(typeof(Room), nameof(Room.Playlist))]
            private BindableList<PlaylistItem> roomPlaylist { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                queueMode.BindValueChanged(_ => InvalidateLayout());
            }

            public override IEnumerable<Drawable> FlowingChildren
            {
                get
                {
                    switch (queueMode.Value)
                    {
                        default:
                            return AliveInternalChildren.Where(d => d.IsPresent)
                                                        .OfType<RearrangeableListItem<PlaylistItem>>()
                                                        .OrderBy(item => item.Model.ID);

                        case QueueMode.AllPlayersRoundRobin:
                            RearrangeableListItem<PlaylistItem>[] items = AliveInternalChildren
                                                                          .Where(d => d.IsPresent)
                                                                          .OfType<RearrangeableListItem<PlaylistItem>>()
                                                                          .OrderBy(item => item.Model.ID)
                                                                          .ToArray();

                            int totalCount = items.Length;
                            if (totalCount == 0)
                                return Enumerable.Empty<Drawable>();

                            // Count of "expired" items per user.
                            Dictionary<int, int> perUserCounts = roomPlaylist
                                                                 .Where(item => item.Expired)
                                                                 .GroupBy(item => item.OwnerID)
                                                                 .ToDictionary(group => group.Key, group => group.Count());

                            // Fill the count dictionary with zeroes for all users with no currently expired items.
                            foreach (var item in items)
                                perUserCounts.TryAdd(item.Model.OwnerID, 0);

                            List<Drawable> result = new List<Drawable>();

                            // Run a simulation...
                            // In every iteration, pick the first available item from the user with the lowest number of items in the queue to add to the result set.
                            // If multiple users have the same number of items in the queue, then the item with the lowest ID is chosen.
                            // Todo: This could probably be more efficient, likely at the cost of increased complexity.
                            while (totalCount-- > 0)
                            {
                                var candidateItem = items
                                                    .Where(item => item != null)
                                                    .OrderBy(item => perUserCounts[item.Model.OwnerID])
                                                    .First();

                                int itemIndex = Array.IndexOf(items, candidateItem);

                                result.Add(items[itemIndex]);
                                perUserCounts[candidateItem.Model.OwnerID]++;
                                items[itemIndex] = null;
                            }

                            return result;
                    }
                }
            }
        }
    }
}
