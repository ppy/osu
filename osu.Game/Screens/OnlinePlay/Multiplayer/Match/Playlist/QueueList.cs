// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    public class QueueList : DrawableRoomPlaylist
    {
        public readonly Bindable<QueueMode> QueueMode = new Bindable<QueueMode>();

        public QueueList(bool allowEdit, bool allowSelection, bool reverse = false)
            : base(allowEdit, allowSelection, reverse)
        {
        }

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new QueueFillFlowContainer
        {
            QueueMode = { BindTarget = QueueMode },
            Spacing = new Vector2(0, 2)
        };

        private class QueueFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            public readonly IBindable<QueueMode> QueueMode = new Bindable<QueueMode>();

            protected override void LoadComplete()
            {
                base.LoadComplete();
                QueueMode.BindValueChanged(_ => InvalidateLayout());
            }

            public override IEnumerable<Drawable> FlowingChildren
            {
                get
                {
                    switch (QueueMode.Value)
                    {
                        default:
                            return AliveInternalChildren.Where(d => d.IsPresent)
                                                        .OfType<RearrangeableListItem<PlaylistItem>>()
                                                        .OrderBy(item => item.Model.ID);

                        case Game.Online.Multiplayer.QueueMode.AllPlayersRoundRobin:
                            RearrangeableListItem<PlaylistItem>[] allItems = AliveInternalChildren
                                                                             .Where(d => d.IsPresent)
                                                                             .OfType<RearrangeableListItem<PlaylistItem>>()
                                                                             .OrderBy(item => item.Model.ID)
                                                                             .ToArray();

                            int totalCount = allItems.Length;
                            if (totalCount == 0)
                                return Enumerable.Empty<Drawable>();

                            Dictionary<int, int> perUserCounts = allItems
                                                                 .Select(item => item.Model.OwnerID)
                                                                 .Distinct()
                                                                 .ToDictionary(u => u, _ => 0);

                            List<Drawable> result = new List<Drawable>();

                            while (totalCount-- > 0)
                            {
                                var candidateItem = allItems
                                                    .Where(item => item != null)
                                                    .OrderBy(item => perUserCounts[item.Model.OwnerID])
                                                    .First();

                                int itemIndex = Array.IndexOf(allItems, candidateItem);

                                result.Add(allItems[itemIndex]);
                                perUserCounts[candidateItem.Model.OwnerID]++;
                                allItems[itemIndex] = null;
                            }

                            return result;
                    }
                }
            }
        }
    }
}
