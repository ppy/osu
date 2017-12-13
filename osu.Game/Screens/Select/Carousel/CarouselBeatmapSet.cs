// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmapSet : CarouselGroupEagerSelect
    {
        public readonly List<CarouselBeatmap> Beatmaps;

        public BeatmapSetInfo BeatmapSet;

        public CarouselBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet == null) throw new ArgumentNullException(nameof(beatmapSet));

            BeatmapSet = beatmapSet;

            Children = Beatmaps = beatmapSet.Beatmaps
                                            .Where(b => !b.Hidden)
                                            .OrderBy(b => b.RulesetID).ThenBy(b => b.StarDifficulty)
                                            .Select(b => new CarouselBeatmap(b))
                                            .ToList();
        }

        protected override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmapSet(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);
            Filtered.Value = Children.All(i => i.Filtered);

            /*switch (criteria.Sort)
            {
                default:
                case SortMode.Artist:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Artist, y.BeatmapSet.Metadata.Artist, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Title:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Title, y.BeatmapSet.Metadata.Title, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Author:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Author.Username, y.BeatmapSet.Metadata.Author.Username, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Difficulty:
                    groups.Sort((x, y) => x.BeatmapSet.MaxStarDifficulty.CompareTo(y.BeatmapSet.MaxStarDifficulty));
                    break;
            }*/
        }
    }
}
