// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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

        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo> GetRecommendedBeatmap;

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

        protected override CarouselItem GetNextToSelect()
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

            int i = 0;

            switch (criteria.Sort)
            {
                default:
                case SortMode.Artist:
                    i = string.Compare(BeatmapSet.Metadata.Artist, otherSet.BeatmapSet.Metadata.Artist, StringComparison.OrdinalIgnoreCase);

                    if (i != 0) return i;
                    goto case SortMode.Title;

                case SortMode.Title:
                    i = string.Compare(BeatmapSet.Metadata.Title, otherSet.BeatmapSet.Metadata.Title, StringComparison.OrdinalIgnoreCase);

                    if (i != 0) return i;
                    goto case SortMode.Author;

                case SortMode.Author:
                    i = string.Compare(BeatmapSet.Metadata.Author.Username, otherSet.BeatmapSet.Metadata.Author.Username, StringComparison.OrdinalIgnoreCase);

                    if (i != 0) return i;
                    goto case SortMode.Source;

                case SortMode.Source:
                    i = string.Compare(BeatmapSet.Metadata.Source, otherSet.BeatmapSet.Metadata.Source, StringComparison.OrdinalIgnoreCase);

                    if (i != 0) return i;
                    goto case SortMode.DateAdded;

                case SortMode.DateAdded:
                    i = otherSet.BeatmapSet.DateAdded.CompareTo(BeatmapSet.DateAdded);

                    if (i != 0) return i;
                    goto case SortMode.DateRanked;

                case SortMode.DateRanked:
                    // Beatmaps which have no ranked date should already be filtered away in this mode.
                    if (BeatmapSet.DateRanked == null || otherSet.BeatmapSet.DateRanked == null)
                        goto case SortMode.LastPlayed;

                    i = otherSet.BeatmapSet.DateRanked.Value.CompareTo(BeatmapSet.DateRanked.Value);

                    if (i != 0) return i;
                    goto case SortMode.LastPlayed;

                case SortMode.LastPlayed:
                    i = -compareUsingAggregateMax(otherSet, b => (b.LastPlayed ?? DateTimeOffset.MinValue).ToUnixTimeSeconds());

                    if (i != 0) return i;
                    goto case SortMode.BPM;

                case SortMode.BPM:
                    i = compareUsingAggregateMax(otherSet, b => b.BPM);

                    if (i != 0) return i;
                    goto case SortMode.Length;

                case SortMode.Length:
                    i = compareUsingAggregateMax(otherSet, b => b.Length);

                    if (i != 0) return i;
                    goto case SortMode.Difficulty;

                case SortMode.Difficulty:
                    i = compareUsingAggregateMax(otherSet, b => b.StarRating);

                    if (i != 0) return i;
                    goto case SortMode.DateSubmitted;

                case SortMode.DateSubmitted:
                    // Beatmaps which have no submitted date should already be filtered away in this mode.
                    if (BeatmapSet.DateSubmitted == null || otherSet.BeatmapSet.DateSubmitted == null)
                        return 0;

                    return otherSet.BeatmapSet.DateSubmitted.Value.CompareTo(BeatmapSet.DateSubmitted.Value);
            }
        }

        /// <summary>
        /// All beatmaps which are not filtered and valid for display.
        /// </summary>
        protected IEnumerable<BeatmapInfo> ValidBeatmaps => Beatmaps.Where(b => !b.Filtered.Value || b.State.Value == CarouselItemState.Selected).Select(b => b.BeatmapInfo);

        private int compareUsingAggregateMax(CarouselBeatmapSet other, Func<BeatmapInfo, double> func)
        {
            bool ourBeatmaps = ValidBeatmaps.Any();
            bool otherBeatmaps = other.ValidBeatmaps.Any();

            if (!ourBeatmaps && !otherBeatmaps) return 0;
            if (!ourBeatmaps) return -1;
            if (!otherBeatmaps) return 1;

            return ValidBeatmaps.Max(func).CompareTo(other.ValidBeatmaps.Max(func));
        }

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            bool filtered = Items.All(i => i.Filtered.Value);

            filtered |= criteria.Sort == SortMode.DateRanked && BeatmapSet?.DateRanked == null;
            filtered |= criteria.Sort == SortMode.DateSubmitted && BeatmapSet?.DateSubmitted == null;

            Filtered.Value = filtered;
        }

        public override string ToString() => BeatmapSet.ToString();
    }
}
