// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
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

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            var criteria = getCriteria();

            return items.Order(Comparer<CarouselItem>.Create((a, b) =>
            {
                var ab = (BeatmapInfo)a.Model;
                var bb = (BeatmapInfo)b.Model;

                if (ab.BeatmapSet!.Equals(bb.BeatmapSet))
                    return compareDifficulty(ab, bb);

                return compare(ab, bb, items, criteria.Sort);
            })).ToList();
        }, cancellationToken).ConfigureAwait(false);

        private static int compare(BeatmapInfo a, BeatmapInfo b, IEnumerable<CarouselItem> items, SortMode sort)
        {
            int comparison;

            switch (sort)
            {
                case SortMode.Artist:
                    comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(a.BeatmapSet!.Metadata.Artist, b.BeatmapSet!.Metadata.Artist);
                    if (comparison == 0)
                        goto case SortMode.Title;
                    break;

                case SortMode.Title:
                    comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(a.BeatmapSet!.Metadata.Title, b.BeatmapSet!.Metadata.Title);
                    break;

                case SortMode.Author:
                    comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(a.BeatmapSet!.Metadata.Author.Username, b.BeatmapSet!.Metadata.Author.Username);
                    break;

                case SortMode.Source:
                    comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(a.BeatmapSet!.Metadata.Source, b.BeatmapSet!.Metadata.Source);
                    break;

                case SortMode.Difficulty:
                    comparison = a.StarRating.CompareTo(b.StarRating);
                    break;

                case SortMode.DateAdded:
                    comparison = b.BeatmapSet!.DateAdded.CompareTo(a.BeatmapSet!.DateAdded);
                    break;

                case SortMode.DateRanked:
                    comparison = Nullable.Compare(b.BeatmapSet!.DateRanked, a.BeatmapSet!.DateRanked);
                    break;

                case SortMode.DateSubmitted:
                    comparison = Nullable.Compare(b.BeatmapSet!.DateSubmitted, a.BeatmapSet!.DateSubmitted);
                    break;

                case SortMode.LastPlayed:
                    comparison = -compareUsingAggregateMax(a, b, items, static b => (b.LastPlayed ?? DateTimeOffset.MinValue).ToUnixTimeSeconds());
                    break;

                case SortMode.BPM:
                    comparison = compareUsingAggregateMax(a, b, items, static b => b.BPM);
                    break;

                case SortMode.Length:
                    comparison = compareUsingAggregateMax(a, b, items, static b => b.Length);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If the initial sort could not differentiate, attempt to use DateAdded to order sets in a stable fashion.
            // The directionality of this matches the current SortMode.DateAdded, but we may want to reconsider if that becomes a user decision (ie. asc / desc).
            if (comparison == 0)
                comparison = b.BeatmapSet!.DateAdded.CompareTo(a.BeatmapSet!.DateAdded);

            // If DateAdded fails to break the tie, fallback to our internal GUID for stability.
            // This basically means it's a stable random sort.
            if (comparison == 0)
                comparison = b.BeatmapSet!.ID.CompareTo(a.BeatmapSet!.ID);

            return comparison;
        }

        private static int compareDifficulty(BeatmapInfo a, BeatmapInfo b)
        {
            int comparison = a.Ruleset.CompareTo(b.Ruleset);

            if (comparison == 0)
                comparison = a.StarRating.CompareTo(b.StarRating);

            return comparison;
        }

        private static int compareUsingAggregateMax(BeatmapInfo a, BeatmapInfo b, IEnumerable<CarouselItem> items, Func<BeatmapInfo, double> func)
        {
            var aMatchedBeatmaps = items.Select(i => i.Model).Cast<BeatmapInfo>().Where(beatmap => beatmap.BeatmapSet!.Equals(a.BeatmapSet));
            var bMatchedBeatmaps = items.Select(i => i.Model).Cast<BeatmapInfo>().Where(beatmap => beatmap.BeatmapSet!.Equals(b.BeatmapSet));

            bool aAny = aMatchedBeatmaps.Any();
            bool bAny = bMatchedBeatmaps.Any();

            if (!aAny && !bAny) return 0;
            if (!aAny) return -1;
            if (!bAny) return 1;

            return aMatchedBeatmaps.Max(func).CompareTo(bMatchedBeatmaps.Max(func));
        }
    }
}
