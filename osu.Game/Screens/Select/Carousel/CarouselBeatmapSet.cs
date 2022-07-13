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
                        return DrawableCarouselBeatmapSet.HEIGHT + Children.Count(c => c.Visible) * DrawableCarouselBeatmap.HEIGHT;

                    default:
                        return DrawableCarouselBeatmapSet.HEIGHT;
                }
            }
        }

        public List<CarouselBeatmap> Beatmaps => InternalChildren.OfType<CarouselBeatmap>().ToList();

        public BeatmapInfo OrderBy;

        public CarouselBeatmap OrderByCarouselItem => Beatmaps.First(b => ReferenceEquals(b.BeatmapInfo, OrderBy));

        public BeatmapSetInfo BeatmapSet;

        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo> GetRecommendedBeatmap;

        public CarouselBeatmapSet(BeatmapSetInfo beatmapSet, BeatmapInfo orderBy)
        {
            BeatmapSet = beatmapSet ?? throw new ArgumentNullException(nameof(beatmapSet));

            beatmapSet.Beatmaps
                      .Where(b => !b.Hidden)
                      .OrderBy(b => b.Ruleset)
                      .ThenBy(b => b.StarRating)
                      .Select(b => new CarouselBeatmap(b))
                      .ForEach(AddChild);
            OrderBy = orderBy;
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

                case SortMode.LastPlayed:
                    return compare(otherSet, b => (b.LastPlayed ?? DateTimeOffset.MinValue).ToUnixTimeSeconds());

                case SortMode.BPM:
                    return compare(otherSet, b => b.BPM);

                case SortMode.Length:
                    return compare(otherSet, b => b.Length);

                case SortMode.Difficulty:
                    return compare(otherSet, b => b.StarRating);
            }
        }

        private int compare(CarouselBeatmapSet other, Func<BeatmapInfo, double> func)
        {
            var bmLeft  = OrderByCarouselItem;
            var bmRight = other.OrderByCarouselItem;

            if (!valid(bmLeft) && !valid(bmRight)) return 0;
            if (!valid(bmLeft)) return -1;
            if (!valid(bmRight)) return 1;

            return func(bmLeft.BeatmapInfo).CompareTo(func(bmRight.BeatmapInfo));

            bool valid(CarouselItem beatmap) => !beatmap.Filtered.Value || beatmap.State.Value == CarouselItemState.Selected;
        }

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);
            Filtered.Value = InternalChildren.All(i => i.Filtered.Value);
        }

        public override string ToString() => BeatmapSet.ToString();
    }
}
