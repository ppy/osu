// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        /// <summary>
        /// Beatmap sets contain difficulties as related panels. This dictionary holds the relationships between set-difficulties to allow expanding them on selection.
        /// </summary>
        public IDictionary<BeatmapSetInfo, HashSet<CarouselItem>> SetItems => setItems;

        /// <summary>
        /// Groups contain children which are group-selectable. This dictionary holds the relationships between groups-panels to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupDefinition, HashSet<CarouselItem>> GroupItems => groupItems;

        private readonly Dictionary<BeatmapSetInfo, HashSet<CarouselItem>> setItems = new Dictionary<BeatmapSetInfo, HashSet<CarouselItem>>();
        private readonly Dictionary<GroupDefinition, HashSet<CarouselItem>> groupItems = new Dictionary<GroupDefinition, HashSet<CarouselItem>>();

        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterGrouping(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            bool groupSetsTogether;

            setItems.Clear();
            groupItems.Clear();

            var criteria = getCriteria();
            var newItems = new List<CarouselItem>(items.Count());

            // Add criteria groups.
            switch (criteria.Group)
            {
                default:
                    groupSetsTogether = true;
                    newItems.AddRange(items);
                    break;

                case GroupMode.Difficulty:
                    groupSetsTogether = false;
                    int starGroup = int.MinValue;

                    foreach (var item in items)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var b = (BeatmapInfo)item.Model;

                        if (b.StarRating > starGroup)
                        {
                            starGroup = (int)Math.Floor(b.StarRating);
                            newItems.Add(new CarouselItem(new GroupDefinition($"{starGroup} - {++starGroup} *")) { DrawHeight = GroupPanel.HEIGHT });
                        }

                        newItems.Add(item);
                    }

                    break;
            }

            // Add set headers wherever required.
            CarouselItem? lastItem = null;

            if (groupSetsTogether)
            {
                for (int i = 0; i < newItems.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var item = newItems[i];

                    if (item.Model is BeatmapInfo beatmap)
                    {
                        if (groupSetsTogether)
                        {
                            bool newBeatmapSet = lastItem == null || (lastItem.Model is BeatmapInfo lastBeatmap && lastBeatmap.BeatmapSet!.ID != beatmap.BeatmapSet!.ID);

                            if (newBeatmapSet)
                            {
                                newItems.Insert(i, new CarouselItem(beatmap.BeatmapSet!) { DrawHeight = BeatmapSetPanel.HEIGHT });
                                i++;
                            }

                            if (!setItems.TryGetValue(beatmap.BeatmapSet!, out var related))
                                setItems[beatmap.BeatmapSet!] = related = new HashSet<CarouselItem>();

                            related.Add(item);
                            item.IsVisible = false;
                        }
                    }

                    lastItem = item;
                }
            }

            // Link group items to their headers.
            GroupDefinition? lastGroup = null;

            foreach (var item in newItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.Model is GroupDefinition group)
                {
                    lastGroup = group;
                    continue;
                }

                if (lastGroup != null)
                {
                    if (!groupItems.TryGetValue(lastGroup, out var groupRelated))
                        groupItems[lastGroup] = groupRelated = new HashSet<CarouselItem>();
                    groupRelated.Add(item);

                    item.IsVisible = false;
                }
            }

            return newItems;
        }, cancellationToken).ConfigureAwait(false);
    }
}
