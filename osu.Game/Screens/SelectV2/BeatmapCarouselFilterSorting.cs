// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterSorting : ICarouselFilter
    {
        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            return items.OrderDescending(Comparer<CarouselItem>.Create((a, b) =>
            {
                if (a.Model is BeatmapInfo ab && b.Model is BeatmapInfo bb)
                    return ab.OnlineID.CompareTo(bb.OnlineID);

                if (a is BeatmapCarouselItem aItem && b is BeatmapCarouselItem bItem)
                    return aItem.ID.CompareTo(bItem.ID);

                return 0;
            }));
        }, cancellationToken).ConfigureAwait(false);
    }
}
