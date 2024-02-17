// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmapSet : CarouselGroupEagerSelect
    {
        public override float TotalHeight
        {
            get
            {
                switch (State.Value)
                {
                    case CarouselItemState.Selected:
                        return DrawableCarouselBeatmapSet.HEIGHT + Items.Count(c => c.Visible) * DrawableCarouselBeatmap.HEIGHT;

                    default:
                        return DrawableCarouselBeatmapSet.HEIGHT;
                }
            }
        }

        public IEnumerable<CarouselBeatmap> Beatmaps => Items.OfType<CarouselBeatmap>();

        public BeatmapSetInfo BeatmapSet;

        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo?>? GetRecommendedBeatmap;

        public CarouselBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            BeatmapSet = beatmapSet ?? throw new ArgumentNullException(nameof(beatmapSet));

            beatmapSet.Beatmaps
                      .Where(b => !b.Hidden)
                      .OrderBy(b => b.Ruleset)
                      .ThenBy(b => b.StarRating)
                      .Select(b => new CarouselBeatmap(b))
                      .ForEach(AddItem);
        }

        public override CarouselItem? GetNextToSelect()
        {
            if (LastSelected == null || LastSelected.Filtered.Value)
            {
                if (GetRecommendedBeatmap?.Invoke(Items.OfType<CarouselBeatmap>().Where(b => !b.Filtered.Value).Select(b => b.BeatmapInfo)) is BeatmapInfo recommended)
                    return Items.OfType<CarouselBeatmap>().First(b => b.BeatmapInfo.Equals(recommended));
            }

            return base.GetNextToSelect();
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmapSet otherSet))
                return base.CompareTo(criteria, other);

            int comparison;

            switch (criteria.Sort)
            {
                default:
                case SortMode.Artist:
                    comparison = string.Compare(BeatmapSet.Metadata.Artist, otherSet.BeatmapSet.Metadata.Artist, StringComparison.Ordinal);
                    break;

                case SortMode.Title:
                    comparison = string.Compare(BeatmapSet.Metadata.Title, otherSet.BeatmapSet.Metadata.Title, StringComparison.Ordinal);
                    break;

                case SortMode.Author:
                    comparison = string.Compare(BeatmapSet.Metadata.Author.Username, otherSet.BeatmapSet.Metadata.Author.Username, StringComparison.Ordinal);
                    break;

                case SortMode.Source:
                    comparison = string.Compare(BeatmapSet.Metadata.Source, otherSet.BeatmapSet.Metadata.Source, StringComparison.Ordinal);
                    break;

                case SortMode.DateAdded:
                    comparison = otherSet.BeatmapSet.DateAdded.CompareTo(BeatmapSet.DateAdded);
                    break;

                case SortMode.DateRanked:
                    comparison = Nullable.Compare(otherSet.BeatmapSet.DateRanked, BeatmapSet.DateRanked);
                    break;

                case SortMode.LastPlayed:
                    comparison = -compareUsingAggregateMax(otherSet, static b => (b.LastPlayed ?? DateTimeOffset.MinValue).ToUnixTimeSeconds());
                    break;

                case SortMode.BPM:
                    comparison = compareUsingAggregateMax(otherSet, static b => b.BPM);
                    break;

                case SortMode.Length:
                    comparison = compareUsingAggregateMax(otherSet, static b => b.Length);
                    break;

                case SortMode.Difficulty:
                    comparison = compareUsingAggregateMax(otherSet, static b => b.StarRating);
                    break;

                case SortMode.DateSubmitted:
                    comparison = Nullable.Compare(otherSet.BeatmapSet.DateSubmitted, BeatmapSet.DateSubmitted);
                    break;
            }

            if (comparison != 0) return comparison;

            // If the initial sort could not differentiate, attempt to use DateAdded to order sets in a stable fashion.
            // The directionality of this matches the current SortMode.DateAdded, but we may want to reconsider if that becomes a user decision (ie. asc / desc).
            comparison = otherSet.BeatmapSet.DateAdded.CompareTo(BeatmapSet.DateAdded);

            if (comparison != 0) return comparison;

            // If DateAdded fails to break the tie, fallback to our internal GUID for stability.
            // This basically means it's a stable random sort.
            return otherSet.BeatmapSet.ID.CompareTo(BeatmapSet.ID);
        }

        /// <summary>
        /// All beatmaps which are not filtered and valid for display.
        /// </summary>
        protected IEnumerable<BeatmapInfo> ValidBeatmaps
        {
            get
            {
                foreach (var item in Items) // iterating over Items directly to not allocate 2 enumerators
                {
                    if (item is CarouselBeatmap b && (!b.Filtered.Value || b.State.Value == CarouselItemState.Selected))
                        yield return b.BeatmapInfo;
                }
            }
        }

        /// <summary>
        /// Whether there are available beatmaps which are not filtered and valid for display.
        /// Cheaper alternative to <see cref="ValidBeatmaps"/>.Any()
        /// </summary>
        public bool HasValidBeatmaps
        {
            get
            {
                foreach (var item in Items) // iterating over Items directly to not allocate 2 enumerators
                {
                    if (item is CarouselBeatmap b && (!b.Filtered.Value || b.State.Value == CarouselItemState.Selected))
                        return true;
                }

                return false;
            }
        }

        private int compareUsingAggregateMax(CarouselBeatmapSet other, Func<BeatmapInfo, double> func)
        {
            bool ourBeatmaps = HasValidBeatmaps;
            bool otherBeatmaps = other.HasValidBeatmaps;

            if (!ourBeatmaps && !otherBeatmaps) return 0;
            if (!ourBeatmaps) return -1;
            if (!otherBeatmaps) return 1;

            return ValidBeatmaps.Max(func).CompareTo(other.ValidBeatmaps.Max(func));
        }

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            Filtered.Value = Items.All(i => i.Filtered.Value);
        }

        public override string ToString() => BeatmapSet.ToString();
    }
}
