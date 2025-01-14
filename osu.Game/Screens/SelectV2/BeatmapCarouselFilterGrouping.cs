// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            // TODO: perform grouping based on FilterCriteria

            CarouselItem? lastItem = null;

            var newItems = new List<CarouselItem>(items.Count());

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.Model is BeatmapInfo b1)
                {
                    // Add set header
                    if (lastItem == null || (lastItem.Model is BeatmapInfo b2 && b2.BeatmapSet!.OnlineID != b1.BeatmapSet!.OnlineID))
                        newItems.Add(new BeatmapCarouselItem(b1.BeatmapSet!));
                }

                newItems.Add(item);
                lastItem = item;
            }

            return newItems;
        }, cancellationToken).ConfigureAwait(false);
    }
}
