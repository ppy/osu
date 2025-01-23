// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        /// <summary>
        /// Beatmap sets contain difficulties as related panels. This dictionary holds the relationships between set-difficulties to allow expanding them on selection.
        /// </summary>
        public IDictionary<BeatmapSetInfo, HashSet<CarouselItem>> SetItems => setItems;

        private readonly Dictionary<BeatmapSetInfo, HashSet<CarouselItem>> setItems = new Dictionary<BeatmapSetInfo, HashSet<CarouselItem>>();

        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterGrouping(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            var criteria = getCriteria();

            if (criteria.SplitOutDifficulties)
            {
                foreach (var item in items)
                {
                    item.IsVisible = true;
                    item.IsGroupSelectionTarget = true;
                }

                return items;
            }

            CarouselItem? lastItem = null;

            var newItems = new List<CarouselItem>(items.Count());

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.Model is BeatmapInfo b)
                {
                    // Add set header
                    if (lastItem == null || (lastItem.Model is BeatmapInfo b2 && b2.BeatmapSet!.OnlineID != b.BeatmapSet!.OnlineID))
                    {
                        newItems.Add(new CarouselItem(b.BeatmapSet!)
                        {
                            DrawHeight = 80,
                            IsGroupSelectionTarget = true
                        });
                    }

                    if (!setItems.TryGetValue(b.BeatmapSet!, out var related))
                        setItems[b.BeatmapSet!] = related = new HashSet<CarouselItem>();

                    related.Add(item);
                }

                newItems.Add(item);
                lastItem = item;

                item.IsGroupSelectionTarget = false;
                item.IsVisible = false;
            }

            return newItems;
        }, cancellationToken).ConfigureAwait(false);
    }
}
