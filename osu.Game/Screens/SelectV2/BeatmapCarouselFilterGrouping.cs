// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
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
        public IDictionary<BeatmapSetInfo, HashSet<CarouselItem>> SetItems => setMap;

        /// <summary>
        /// Groups contain children which are group-selectable. This dictionary holds the relationships between groups-panels to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupDefinition, HashSet<CarouselItem>> GroupItems => groupMap;

        private readonly Dictionary<BeatmapSetInfo, HashSet<CarouselItem>> setMap = new Dictionary<BeatmapSetInfo, HashSet<CarouselItem>>();
        private readonly Dictionary<GroupDefinition, HashSet<CarouselItem>> groupMap = new Dictionary<GroupDefinition, HashSet<CarouselItem>>();

        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterGrouping(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                setMap.Clear();
                groupMap.Clear();

                var criteria = getCriteria();
                var newItems = new List<CarouselItem>();

                BeatmapSetsGroupedTogether = ShouldGroupBeatmapsTogether(criteria);

                var groups = getGroups((List<CarouselItem>)items, criteria);

                foreach (var (group, itemsInGroup) in groups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    CarouselItem? groupItem = null;
                    HashSet<CarouselItem>? currentGroupItems = null;
                    HashSet<CarouselItem>? currentSetItems = null;
                    BeatmapInfo? lastBeatmap = null;

                    if (group != null)
                    {
                        groupMap[group] = currentGroupItems = new HashSet<CarouselItem>();

                        addItem(groupItem = new CarouselItem(group)
                        {
                            DrawHeight = PanelGroup.HEIGHT,
                            DepthLayer = -2,
                        });
                    }

                    foreach (var item in itemsInGroup)
                    {
                        var beatmap = (BeatmapInfo)item.Model;

                        bool newBeatmapSet = lastBeatmap?.BeatmapSet!.ID != beatmap.BeatmapSet!.ID;

                        if (newBeatmapSet)
                        {
                            if (!setMap.TryGetValue(beatmap.BeatmapSet!, out currentSetItems))
                                setMap[beatmap.BeatmapSet!] = currentSetItems = new HashSet<CarouselItem>();
                        }

                        if (BeatmapSetsGroupedTogether)
                        {
                            if (newBeatmapSet)
                            {
                                if (groupItem != null)
                                    groupItem.NestedItemCount++;

                                addItem(new CarouselItem(beatmap.BeatmapSet!)
                                {
                                    DrawHeight = PanelBeatmapSet.HEIGHT,
                                    DepthLayer = -1
                                });
                            }
                        }
                        else
                        {
                            if (groupItem != null)
                                groupItem.NestedItemCount++;

                            item.DrawHeight = PanelBeatmapStandalone.HEIGHT;
                        }

                        addItem(item);
                        lastBeatmap = beatmap;
                    }

                    void addItem(CarouselItem i)
                    {
                        newItems.Add(i);

                        currentGroupItems?.Add(i);
                        currentSetItems?.Add(i);

                        i.IsVisible = i.Model is GroupDefinition || (group == null && (i.Model is BeatmapSetInfo || !BeatmapSetsGroupedTogether));
                    }
                }

                return newItems;
            }, cancellationToken).ConfigureAwait(false);
        }

        public static bool ShouldGroupBeatmapsTogether(FilterCriteria criteria)
        {
            // In certain cases, we intentionally split out difficulties
            // where it's more relevant or convenient to view them as individual items.
            if (criteria.Sort == SortMode.Difficulty || criteria.Group == GroupMode.Difficulty)
                return false;
            if (criteria.Sort == SortMode.LastPlayed && criteria.Group == GroupMode.LastPlayed)
                return false;

            // In the majority case we group sets together for display.
            return true;
        }

        private List<GroupMapping> getGroups(List<CarouselItem> items, FilterCriteria criteria)
        {
            switch (criteria.Group)
            {
                case GroupMode.None:
                    return new List<GroupMapping> { new GroupMapping(null, items) };

                case GroupMode.Artist:
                    return getGroupsBy(b => defineGroupAlphabetically(b.BeatmapSet!.Metadata.Artist), items);

                case GroupMode.Author:
                    return getGroupsBy(b => defineGroupAlphabetically(b.BeatmapSet!.Metadata.Author.Username), items);

                case GroupMode.Title:
                    return getGroupsBy(b => defineGroupAlphabetically(b.BeatmapSet!.Metadata.Title), items);

                case GroupMode.DateAdded:
                    return getGroupsBy(b => defineGroupByDate(b.BeatmapSet!.DateAdded), items);

                case GroupMode.DateRanked:
                    return getGroupsBy(b => defineGroupByRankedDate(b.BeatmapSet!.DateRanked), items);

                case GroupMode.LastPlayed:
                    return getGroupsBy(b =>
                    {
                        var date = b.LastPlayed;

                        if (BeatmapSetsGroupedTogether)
                            date = aggregateMax(b, static b => (b.LastPlayed ?? DateTimeOffset.MinValue));

                        if (date == null || date == DateTimeOffset.MinValue)
                            return new GroupDefinition(int.MaxValue, "Never");

                        return defineGroupByDate(date.Value);
                    }, items);

                case GroupMode.RankedStatus:
                    return getGroupsBy(b => defineGroupByStatus(b.BeatmapSet!.Status), items);

                case GroupMode.BPM:
                    return getGroupsBy(b =>
                    {
                        double bpm = b.BPM;

                        if (BeatmapSetsGroupedTogether)
                            bpm = aggregateMax(b, bb => bb.BPM);

                        return defineGroupByBPM(bpm);
                    }, items);

                case GroupMode.Difficulty:
                    return getGroupsBy(b => defineGroupByStars(b.StarRating), items);

                case GroupMode.Length:
                    return getGroupsBy(b =>
                    {
                        double length = b.Length;

                        if (BeatmapSetsGroupedTogether)
                            length = aggregateMax(b, bb => bb.Length);

                        return defineGroupByLength(length);
                    }, items);

                case GroupMode.Source:
                    return getGroupsBy(b => defineGroupBySource(b.BeatmapSet!.Metadata.Source), items);

                // TODO: need implementation
                //
                // case GroupMode.Collections:
                //     goto case GroupMode.None;
                //
                // case GroupMode.Favourites:
                //     goto case GroupMode.None;
                //
                // case GroupMode.MyMaps:
                //     goto case GroupMode.None;
                //
                // case GroupMode.RankAchieved:
                //     goto case GroupMode.None;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<GroupMapping> getGroupsBy(Func<BeatmapInfo, GroupDefinition> getGroup, List<CarouselItem> items)
        {
            return items.GroupBy(i => getGroup((BeatmapInfo)i.Model))
                        .OrderBy(s => s.Key.Order)
                        .ThenBy(s => s.Key.Title)
                        .Select(g => new GroupMapping(g.Key, g.ToList()))
                        .ToList();
        }

        private GroupDefinition defineGroupAlphabetically(string name)
        {
            char firstChar = name.FirstOrDefault();

            if (char.IsAsciiDigit(firstChar))
                return new GroupDefinition(int.MinValue, "0-9");

            if (char.IsAsciiLetter(firstChar))
                return new GroupDefinition(char.ToUpperInvariant(firstChar) - 'A', char.ToUpperInvariant(firstChar).ToString());

            return new GroupDefinition(int.MaxValue, "Other");
        }

        private GroupDefinition defineGroupByDate(DateTimeOffset date)
        {
            var now = DateTimeOffset.Now;
            var elapsed = now - date;

            if (elapsed.TotalDays < 1)
                return new GroupDefinition(0, "Today");

            if (elapsed.TotalDays < 2)
                return new GroupDefinition(1, "Yesterday");

            if (elapsed.TotalDays < 7)
                return new GroupDefinition(2, "Last week");

            if (elapsed.TotalDays < 30)
                return new GroupDefinition(3, "1 month ago");

            for (int i = 60; i <= 150; i += 30)
            {
                if (elapsed.TotalDays < i)
                    return new GroupDefinition(i, $"{i / 30} months ago");
            }

            return new GroupDefinition(151, "Over 5 months ago");
        }

        private GroupDefinition defineGroupByRankedDate(DateTimeOffset? date)
        {
            if (date == null)
                return new GroupDefinition(0, "Unranked");

            return new GroupDefinition(-date.Value.Year, $"{date.Value.Year}");
        }

        private GroupDefinition defineGroupByStatus(BeatmapOnlineStatus status)
        {
            switch (status)
            {
                case BeatmapOnlineStatus.Ranked:
                case BeatmapOnlineStatus.Approved:
                    return new GroupDefinition(0, BeatmapOnlineStatus.Ranked.GetDescription());

                case BeatmapOnlineStatus.Qualified:
                    return new GroupDefinition(1, status.GetDescription());

                case BeatmapOnlineStatus.WIP:
                    return new GroupDefinition(2, status.GetDescription());

                case BeatmapOnlineStatus.Pending:
                    return new GroupDefinition(3, status.GetDescription());

                case BeatmapOnlineStatus.Graveyard:
                    return new GroupDefinition(4, status.GetDescription());

                case BeatmapOnlineStatus.LocallyModified:
                    return new GroupDefinition(5, status.GetDescription());

                case BeatmapOnlineStatus.None:
                    return new GroupDefinition(6, status.GetDescription());

                case BeatmapOnlineStatus.Loved:
                    return new GroupDefinition(7, status.GetDescription());

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private GroupDefinition defineGroupByBPM(double bpm)
        {
            for (int i = 1; i < 6; i++)
            {
                if (bpm < i * 60)
                    return new GroupDefinition(i, $"Under {i * 60} BPM");
            }

            return new GroupDefinition(6, "Over 300 BPM");
        }

        private GroupDefinition defineGroupByStars(double stars)
        {
            // truncation is intentional - compare `FormatUtils.FormatStarRating()`
            int starInt = (int)stars;
            var starDifficulty = new StarDifficulty(starInt, 0);

            if (starInt == 0)
                return new StarDifficultyGroupDefinition(0, "Below 1 Star", starDifficulty);

            if (starInt == 1)
                return new StarDifficultyGroupDefinition(1, "1 Star", starDifficulty);

            return new StarDifficultyGroupDefinition(starInt, $"{starInt} Stars", starDifficulty);
        }

        private GroupDefinition defineGroupByLength(double length)
        {
            for (int i = 1; i < 6; i++)
            {
                if (length <= i * 60_000)
                {
                    if (i == 1)
                        return new GroupDefinition(1, "1 minute or less");

                    return new GroupDefinition(i, $"{i} minutes or less");
                }
            }

            if (length <= 10 * 60_000)
                return new GroupDefinition(10, "10 minutes or less");

            return new GroupDefinition(11, "Over 10 minutes");
        }

        private GroupDefinition defineGroupBySource(string source)
        {
            if (string.IsNullOrEmpty(source))
                return new GroupDefinition(1, "Unsourced");

            return new GroupDefinition(0, source);
        }

        private static T? aggregateMax<T>(BeatmapInfo b, Func<BeatmapInfo, T> func)
        {
            var beatmaps = b.BeatmapSet!.Beatmaps.Where(bb => !bb.Hidden);
            return beatmaps.Max(func);
        }

        private record GroupMapping(GroupDefinition? Group, List<CarouselItem> ItemsInGroup);
    }
}
