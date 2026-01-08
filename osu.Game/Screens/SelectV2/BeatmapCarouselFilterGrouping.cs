// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics.Carousel;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Utils;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        public bool BeatmapSetsGroupedTogether { get; private set; }

        /// <summary>
        /// The total number of beatmap difficulties displayed post filter.
        /// </summary>
        public int BeatmapItemsCount { get; private set; }

        public IDictionary<object, (CarouselItem item, int index)> ItemMap => itemMap;

        /// <summary>
        /// Beatmap sets contain difficulties as related panels. This dictionary holds the relationships between set-difficulties to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupedBeatmapSet, HashSet<CarouselItem>> SetItems => setMap;

        /// <summary>
        /// Groups contain children which are group-selectable. This dictionary holds the relationships between groups-panels to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupDefinition, HashSet<CarouselItem>> GroupItems => groupMap;

        private Dictionary<object, (CarouselItem, int)> itemMap = new Dictionary<object, (CarouselItem, int)>();
        private Dictionary<GroupedBeatmapSet, HashSet<CarouselItem>> setMap = new Dictionary<GroupedBeatmapSet, HashSet<CarouselItem>>();
        private Dictionary<GroupDefinition, HashSet<CarouselItem>> groupMap = new Dictionary<GroupDefinition, HashSet<CarouselItem>>();

        public required Func<FilterCriteria> GetCriteria { get; init; }
        public required Func<List<BeatmapCollection>> GetCollections { get; init; }
        public required Func<FilterCriteria, IReadOnlyDictionary<Guid, ScoreRank>> GetLocalUserTopRanks { get; init; }
        public required Func<HashSet<int>> GetFavouriteBeatmapSets { get; init; }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                // preallocate space for the new mappings using last known estimates
                var newItemMap = new Dictionary<object, (CarouselItem, int)>(itemMap.Count);
                var newSetMap = new Dictionary<GroupedBeatmapSet, HashSet<CarouselItem>>(setMap.Count);
                var newGroupMap = new Dictionary<GroupDefinition, HashSet<CarouselItem>>(groupMap.Count);

                var criteria = GetCriteria();
                var newItems = new List<CarouselItem>();

                BeatmapSetsGroupedTogether = ShouldGroupBeatmapsTogether(criteria);

                var groups = getGroups((List<CarouselItem>)items, criteria);
                int displayedBeatmapsCount = 0;

                foreach (var (group, itemsInGroup) in groups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    CarouselItem? groupItem = null;
                    HashSet<CarouselItem>? currentGroupItems = null;
                    HashSet<CarouselItem>? currentSetItems = null;
                    BeatmapInfo? lastBeatmap = null;

                    if (group != null)
                    {
                        newGroupMap[group] = currentGroupItems = new HashSet<CarouselItem>();

                        addItem(groupItem = new CarouselItem(group)
                        {
                            DrawHeight = PanelGroup.HEIGHT,
                            DepthLayer = -2,
                        });
                    }

                    foreach (var item in itemsInGroup)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var beatmap = (BeatmapInfo)item.Model;

                        bool newBeatmapSet = lastBeatmap?.BeatmapSet!.ID != beatmap.BeatmapSet!.ID;
                        var groupedBeatmapSet = new GroupedBeatmapSet(group, beatmap.BeatmapSet!);

                        if (newBeatmapSet)
                        {
                            if (!newSetMap.TryGetValue(groupedBeatmapSet, out currentSetItems))
                                newSetMap[groupedBeatmapSet] = currentSetItems = new HashSet<CarouselItem>();
                        }

                        if (BeatmapSetsGroupedTogether)
                        {
                            if (newBeatmapSet)
                            {
                                if (groupItem != null)
                                    groupItem.NestedItemCount++;

                                addItem(new CarouselItem(groupedBeatmapSet)
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
                        }

                        addItem(new CarouselItem(new GroupedBeatmap(group, beatmap))
                        {
                            DrawHeight = BeatmapSetsGroupedTogether ? PanelBeatmap.HEIGHT : PanelBeatmapStandalone.HEIGHT,
                        });
                        lastBeatmap = beatmap;
                        displayedBeatmapsCount++;
                    }

                    void addItem(CarouselItem i)
                    {
                        newItems.Add(i);

                        newItemMap[i.Model] = (i, newItems.Count - 1);
                        currentGroupItems?.Add(i);
                        currentSetItems?.Add(i);

                        i.IsVisible = i.Model is GroupDefinition || (group == null && (i.Model is GroupedBeatmapSet || !BeatmapSetsGroupedTogether));
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                Interlocked.Exchange(ref itemMap, newItemMap);
                Interlocked.Exchange(ref setMap, newSetMap);
                Interlocked.Exchange(ref groupMap, newGroupMap);
                BeatmapItemsCount = displayedBeatmapsCount;
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
            if (criteria.Group == GroupMode.RankAchieved)
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

                        if (date == null)
                            return new GroupDefinition(int.MaxValue, "Never").Yield();

                        return defineGroupByDate(date.Value);
                    }, items);

                case GroupMode.RankedStatus:
                    return getGroupsBy(b => defineGroupByStatus(b.BeatmapSet!.Status), items);

                case GroupMode.BPM:
                    return getGroupsBy(b => defineGroupByBPM(FormatUtils.RoundBPM(b.BPM)), items);

                case GroupMode.Difficulty:
                    return getGroupsBy(b => defineGroupByStars(b.StarRating), items);

                case GroupMode.Length:
                    return getGroupsBy(b => defineGroupByLength(b.Length), items);

                case GroupMode.Source:
                    return getGroupsBy(b => defineGroupBySource(b.BeatmapSet!.Metadata.Source), items);

                case GroupMode.Collections:
                {
                    var collections = GetCollections();
                    return defineGroupsByCollection(items, collections);
                }

                case GroupMode.MyMaps:
                    return getGroupsBy(b => defineGroupByOwnMaps(b, criteria.LocalUserId, criteria.LocalUserUsername), items);

                case GroupMode.RankAchieved:
                {
                    var topRankMapping = GetLocalUserTopRanks(criteria);
                    return getGroupsBy(b => defineGroupByRankAchieved(b, topRankMapping), items);
                }

                case GroupMode.Favourites:
                {
                    var favouriteBeatmapSets = GetFavouriteBeatmapSets();
                    return getGroupsBy(b => defineGroupByFavourites(b, favouriteBeatmapSets), items);
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<GroupMapping> getGroupsBy(Func<BeatmapInfo, IEnumerable<GroupDefinition>> defineGroups, List<CarouselItem> items)
        {
            var groups = new Dictionary<GroupDefinition, GroupMapping>();

            foreach (var item in items)
            {
                foreach (var groupDefinition in defineGroups((BeatmapInfo)item.Model))
                {
                    if (!groups.TryGetValue(groupDefinition, out var group))
                        group = groups[groupDefinition] = new GroupMapping(groupDefinition, []);

                    group.ItemsInGroup.Add(item);
                }
            }

            return groups.Values
                         .OrderBy(g => g.Group!.Order)
                         .ThenBy(g => g.Group!.Title.ToString())
                         .ToList();
        }

        private IEnumerable<GroupDefinition> defineGroupAlphabetically(string name)
        {
            char firstChar = name.FirstOrDefault();

            if (char.IsAsciiDigit(firstChar))
                return new GroupDefinition(int.MinValue, "0-9").Yield();

            if (char.IsAsciiLetter(firstChar))
                return new GroupDefinition(char.ToUpperInvariant(firstChar) - 'A', char.ToUpperInvariant(firstChar).ToString()).Yield();

            return new GroupDefinition(int.MaxValue, "Other").Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupByDate(DateTimeOffset date)
        {
            var now = DateTimeOffset.Now;
            var elapsed = now - date;

            if (elapsed.TotalDays < 1)
                return new GroupDefinition(0, "Today").Yield();

            if (elapsed.TotalDays < 2)
                return new GroupDefinition(1, "Yesterday").Yield();

            if (elapsed.TotalDays < 7)
                return new GroupDefinition(2, "Last week").Yield();

            if (elapsed.TotalDays < 30)
                return new GroupDefinition(3, "Last month").Yield();

            if (elapsed.TotalDays < 60)
                return new GroupDefinition(4, "1 month ago").Yield();

            for (int i = 90; i <= 150; i += 30)
            {
                if (elapsed.TotalDays < i)
                    return new GroupDefinition(i, $"{i / 30 - 1} months ago").Yield();
            }

            return new GroupDefinition(151, "Over 5 months ago").Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupByRankedDate(DateTimeOffset? date)
        {
            if (date == null)
                return new GroupDefinition(0, "Unranked").Yield();

            return new GroupDefinition(-date.Value.Year, $"{date.Value.Year}").Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupByStatus(BeatmapOnlineStatus status)
        {
            switch (status)
            {
                case BeatmapOnlineStatus.Ranked:
                case BeatmapOnlineStatus.Approved:
                    return new RankedStatusGroupDefinition(0, BeatmapOnlineStatus.Ranked).Yield();

                case BeatmapOnlineStatus.Qualified:
                    return new RankedStatusGroupDefinition(1, status).Yield();

                case BeatmapOnlineStatus.WIP:
                    return new RankedStatusGroupDefinition(2, status).Yield();

                case BeatmapOnlineStatus.Pending:
                    return new RankedStatusGroupDefinition(3, status).Yield();

                case BeatmapOnlineStatus.Graveyard:
                    return new RankedStatusGroupDefinition(4, status).Yield();

                case BeatmapOnlineStatus.LocallyModified:
                    return new RankedStatusGroupDefinition(5, status).Yield();

                case BeatmapOnlineStatus.None:
                    return new RankedStatusGroupDefinition(6, status).Yield();

                case BeatmapOnlineStatus.Loved:
                    return new RankedStatusGroupDefinition(7, status).Yield();

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private IEnumerable<GroupDefinition> defineGroupByBPM(double bpm)
        {
            if (bpm < 60)
                return new GroupDefinition(60, "Under 60 BPM").Yield();

            for (int i = 70; i <= 300; i += 10)
            {
                if (bpm < i)
                    return new GroupDefinition(i, $"{i - 10} - {i} BPM").Yield();
            }

            return new GroupDefinition(301, "Over 300 BPM").Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupByStars(double stars)
        {
            // truncation is intentional - compare `FormatUtils.FormatStarRating()`
            int starInt = (int)stars;
            var starDifficulty = new StarDifficulty(starInt, 0);

            if (starInt == 0)
                return new StarDifficultyGroupDefinition(0, "Below 1 Star", starDifficulty).Yield();

            if (starInt == 1)
                return new StarDifficultyGroupDefinition(1, "1 Star", starDifficulty).Yield();

            if (starInt < 15)
                return new StarDifficultyGroupDefinition(starInt, $"{starInt} Stars", starDifficulty).Yield();

            return new StarDifficultyGroupDefinition(15, "Over 15 Stars", new StarDifficulty(15, 0)).Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupByLength(double length)
        {
            for (int i = 1; i < 6; i++)
            {
                if (length <= i * 60_000)
                {
                    if (i == 1)
                        return new GroupDefinition(1, "1 minute or less").Yield();

                    return new GroupDefinition(i, $"{i} minutes or less").Yield();
                }
            }

            if (length <= 10 * 60_000)
                return new GroupDefinition(10, "10 minutes or less").Yield();

            return new GroupDefinition(11, "Over 10 minutes").Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupBySource(string source)
        {
            if (string.IsNullOrEmpty(source))
                return new GroupDefinition(1, "Unsourced").Yield();

            return new GroupDefinition(0, source).Yield();
        }

        private List<GroupMapping> defineGroupsByCollection(List<CarouselItem> carouselItems, List<BeatmapCollection> allCollections)
        {
            Dictionary<GroupDefinition, GroupMapping> groupMappings = new Dictionary<GroupDefinition, GroupMapping>();
            // this is a pre-built mapping of MD5s to a list of collections in which this MD5 is found in.
            // the reason to pre-build this is that `BeatmapCollection.BeatmapMD5Hashes` is a list and therefore a naive implementation would be slow,
            // particularly in edge cases where most beatmaps are in more than one collection.
            Dictionary<string, List<GroupDefinition>> md5ToCollectionsMap = new Dictionary<string, List<GroupDefinition>>();

            for (int i = 0; i < allCollections.Count; i++)
            {
                var collection = allCollections[i];
                // NOTE: the ordering of the incoming collection list is significant and needs to be preserved.
                // the fallback to ordering by name cannot be relied on.
                // see xmldoc of `BeatmapCarousel.GetAllCollections()`.
                var groupDefinition = new GroupDefinition(i, collection.Name);
                groupMappings[groupDefinition] = new GroupMapping(groupDefinition, []);

                foreach (string md5 in collection.BeatmapMD5Hashes)
                {
                    if (!md5ToCollectionsMap.TryGetValue(md5, out var collections))
                        md5ToCollectionsMap[md5] = collections = new List<GroupDefinition>();

                    collections.Add(groupDefinition);
                }
            }

            var notInCollection = new GroupDefinition(int.MaxValue, "Not in collection");
            groupMappings[notInCollection] = new GroupMapping(notInCollection, []);

            foreach (var item in carouselItems)
            {
                var beatmap = (BeatmapInfo)item.Model;

                // as a side note, even reading the `MD5Hash` off a realm model is slow if done enough times,
                // so it definitely helps that thanks to the mapping it needs to only be retrieved once
                if (md5ToCollectionsMap.TryGetValue(beatmap.MD5Hash, out var collections))
                {
                    foreach (var collection in collections)
                        groupMappings[collection].ItemsInGroup.Add(item);
                }
                else
                    groupMappings[notInCollection].ItemsInGroup.Add(item);
            }

            return groupMappings.Values
                                // safety against potentially empty eagerly-initialised groups
                                // (could happen if user has a collection with MD5s of maps that aren't locally available)
                                .Where(mapping => mapping.ItemsInGroup.Count > 0)
                                .OrderBy(mapping => mapping.Group!.Order)
                                .ToList();
        }

        private IEnumerable<GroupDefinition> defineGroupByOwnMaps(BeatmapInfo beatmap, int? localUserId, string? localUserUsername)
        {
            var author = beatmap.BeatmapSet!.Metadata.Author;

            if (author.OnlineID == localUserId || (author.OnlineID <= 1 && author.Username == localUserUsername))
                return new GroupDefinition(0, "My maps").Yield();

            // discard beatmaps not owned by the user.
            return [];
        }

        private IEnumerable<GroupDefinition> defineGroupByRankAchieved(BeatmapInfo beatmap, IReadOnlyDictionary<Guid, ScoreRank> topRankMapping)
        {
            if (topRankMapping.TryGetValue(beatmap.ID, out var rank))
                return new RankDisplayGroupDefinition(rank).Yield();

            return new GroupDefinition(int.MaxValue, "Unplayed").Yield();
        }

        private IEnumerable<GroupDefinition> defineGroupByFavourites(BeatmapInfo beatmap, HashSet<int> favouriteBeatmapSets)
        {
            if (beatmap.BeatmapSet?.OnlineID > 0 && favouriteBeatmapSets.Contains(beatmap.BeatmapSet.OnlineID))
                return new GroupDefinition(0, "Favourites").Yield();

            return [];
        }

        private record GroupMapping(GroupDefinition? Group, List<CarouselItem> ItemsInGroup);
    }
}
