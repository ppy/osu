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
                        return DrawableCarouselBeatmapSet.HEIGHT + Children.Count(c => c.Visible) * DrawableCarouselBeatmap.HEIGHT;

                    default:
                        return DrawableCarouselBeatmapSet.HEIGHT;
                }
            }
        }

        public IEnumerable<CarouselBeatmap> Beatmaps => InternalChildren.OfType<CarouselBeatmap>();

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
                      .ForEach(AddChild);
        }

        protected override CarouselItem GetNextToSelect()
        {
            if (LastSelected == null || LastSelected.Filtered.Value)
            {
                if (GetRecommendedBeatmap?.Invoke(Children.OfType<CarouselBeatmap>().Where(b => !b.Filtered.Value).Select(b => b.BeatmapInfo)) is BeatmapInfo recommended)
                    return Children.OfType<CarouselBeatmap>().First(b => b.BeatmapInfo.Equals(recommended));
            }

            return base.GetNextToSelect();
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmapSet otherSet))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Artist:
                    return string.Compare(BeatmapSet.Metadata.Artist, otherSet.BeatmapSet.Metadata.Artist, StringComparison.OrdinalIgnoreCase);

                case SortMode.Title:
                    return string.Compare(BeatmapSet.Metadata.Title, otherSet.BeatmapSet.Metadata.Title, StringComparison.OrdinalIgnoreCase);

                case SortMode.Author:
                    return string.Compare(BeatmapSet.Metadata.Author.Username, otherSet.BeatmapSet.Metadata.Author.Username, StringComparison.OrdinalIgnoreCase);

                case SortMode.Source:
                    return string.Compare(BeatmapSet.Metadata.Source, otherSet.BeatmapSet.Metadata.Source, StringComparison.OrdinalIgnoreCase);

                case SortMode.DateAdded:
                    return otherSet.BeatmapSet.DateAdded.CompareTo(BeatmapSet.DateAdded);

                case SortMode.BPM:
                    return compareUsingAggregateMax(otherSet, b => b.BPM);

                case SortMode.Length:
                    return compareUsingAggregateMax(otherSet, b => b.Length);

                case SortMode.Difficulty:
                    return compareUsingAggregateMax(otherSet, b => b.StarRating);
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
            Filtered.Value = InternalChildren.All(i => i.Filtered.Value);
        }

        public override string ToString() => BeatmapSet.ToString();
    }
}
