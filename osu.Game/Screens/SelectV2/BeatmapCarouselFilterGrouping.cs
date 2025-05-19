// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        public bool BeatmapSetsGroupedTogether { get; private set; }

        /// <summary>
        /// Beatmap sets contain difficulties as related panels. This dictionary holds the relationships between set-difficulties to allow expanding them on selection.
        /// </summary>
        public IDictionary<BeatmapSetInfo, HashSet<CarouselItem>> SetItems => setItems;

        /// <summary>
        /// Groups contain children which are group-selectable. This dictionary holds the relationships between groups-panels to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupDefinition, HashSet<CarouselItem>> GroupItems => groupItems;

        private readonly Dictionary<BeatmapSetInfo, HashSet<CarouselItem>> setItems = new Dictionary<BeatmapSetInfo, HashSet<CarouselItem>>();
        private readonly Dictionary<GroupDefinition, HashSet<CarouselItem>> groupItems = new Dictionary<GroupDefinition, HashSet<CarouselItem>>();

        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterGrouping(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                setItems.Clear();
                groupItems.Clear();

                var criteria = getCriteria();
                var newItems = new List<CarouselItem>();

                BeatmapInfo? lastBeatmap = null;

                GroupDefinition? lastGroup = null;
                CarouselItem? lastGroupItem = null;

                HashSet<CarouselItem>? currentGroupItems = null;
                HashSet<CarouselItem>? currentSetItems = null;

                BeatmapSetsGroupedTogether = criteria.Sort != SortMode.Difficulty;

                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var beatmap = (BeatmapInfo)item.Model;

                    if (createGroupIfRequired(criteria, beatmap, lastGroup) is GroupDefinition newGroup)
                    {
                        // When reaching a new group, ensure we reset any beatmap set tracking.
                        currentSetItems = null;
                        lastBeatmap = null;

                        groupItems[newGroup] = currentGroupItems = new HashSet<CarouselItem>();
                        lastGroup = newGroup;

                        addItem(lastGroupItem = new CarouselItem(newGroup)
                        {
                            DrawHeight = PanelGroup.HEIGHT,
                            DepthLayer = -2,
                        });
                    }

                    if (BeatmapSetsGroupedTogether)
                    {
                        bool newBeatmapSet = lastBeatmap?.BeatmapSet!.ID != beatmap.BeatmapSet!.ID;

                        if (newBeatmapSet)
                        {
                            setItems[beatmap.BeatmapSet!] = currentSetItems = new HashSet<CarouselItem>();

                            if (lastGroupItem != null)
                                lastGroupItem.NestedItemCount++;

                            addItem(new CarouselItem(beatmap.BeatmapSet!)
                            {
                                DrawHeight = PanelBeatmapSet.HEIGHT,
                                DepthLayer = -1
                            });
                        }
                    }
                    else
                    {
                        if (lastGroupItem != null)
                            lastGroupItem.NestedItemCount++;

                        item.DrawHeight = PanelBeatmapStandalone.HEIGHT;
                    }

                    addItem(item);
                    lastBeatmap = beatmap;

                    void addItem(CarouselItem i)
                    {
                        newItems.Add(i);

                        currentGroupItems?.Add(i);
                        currentSetItems?.Add(i);

                        i.IsVisible = i.Model is GroupDefinition || (lastGroup == null && (i.Model is BeatmapSetInfo || currentSetItems == null));
                    }
                }

                return newItems;
            }, cancellationToken).ConfigureAwait(false);
        }

        private GroupDefinition? createGroupIfRequired(FilterCriteria criteria, BeatmapInfo beatmap, GroupDefinition? lastGroup)
        {
            switch (criteria.Group)
            {
                case GroupMode.Artist:
                    char groupChar = lastGroup?.Data as char? ?? (char)0;
                    char beatmapFirstChar = char.ToUpperInvariant(beatmap.Metadata.Artist[0]);

                    if (beatmapFirstChar > groupChar)
                        return new GroupDefinition(beatmapFirstChar, $"{beatmapFirstChar}");

                    break;

                case GroupMode.Difficulty:
                    var starGroup = lastGroup?.Data as StarDifficulty? ?? new StarDifficulty(-1, 0);
                    double beatmapStarRating = Math.Round(beatmap.StarRating, 2);

                    if (beatmapStarRating >= starGroup.Stars + 1)
                    {
                        starGroup = new StarDifficulty((int)Math.Floor(beatmapStarRating), 0);

                        if (starGroup.Stars == 0)
                            return new GroupDefinition(starGroup, "Below 1 Star");

                        if (starGroup.Stars == 1)
                            return new GroupDefinition(starGroup, "1 Star");

                        return new GroupDefinition(starGroup, $"{starGroup.Stars} Stars");
                    }

                    break;
            }

            return null;
        }
    }
}
