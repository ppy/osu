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
using osu.Game.Utils;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterSorting : ICarouselFilter
    {
        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterSorting(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            var criteria = getCriteria();

            return items.Order(Comparer<CarouselItem>.Create((a, b) =>
            {
                int comparison;

                var ab = (BeatmapInfo)a.Model;
                var bb = (BeatmapInfo)b.Model;

                switch (criteria.Sort)
                {
                    case SortMode.Artist:
                        comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(ab.Metadata.Artist, bb.Metadata.Artist);
                        if (comparison == 0)
                            goto case SortMode.Title;
                        break;

                    case SortMode.Difficulty:
                        comparison = ab.StarRating.CompareTo(bb.StarRating);
                        break;

                    case SortMode.Title:
                        comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(ab.Metadata.Title, bb.Metadata.Title);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return comparison;
            }));
        }, cancellationToken).ConfigureAwait(false);
    }
}
