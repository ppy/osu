// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class BeatmapCarouselFilterGroupingTest
    {
        #region No grouping

        [Test]
        public async Task TestNoGrouping()
        {
            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(applyTitle('E'), beatmapSets, out var beatmap1);
            addBeatmapSet(applyArtist('D'), beatmapSets, out var beatmap2);
            addBeatmapSet(applyAuthor('H'), beatmapSets, out var beatmap3);
            addBeatmapSet(applyLength(65_000), beatmapSets, out var beatmap4);

            BeatmapInfo[] allBeatmaps =
            [
                ..beatmap1.Beatmaps,
                ..beatmap2.Beatmaps,
                ..beatmap3.Beatmaps,
                ..beatmap4.Beatmaps
            ];

            var results = await runGrouping(GroupMode.NoGrouping, beatmapSets);
            Assert.That(results.Select(r => r.Model).OfType<BeatmapSetInfo>(), Is.EquivalentTo(beatmapSets));
            Assert.That(results.Select(r => r.Model).OfType<BeatmapInfo>(), Is.EquivalentTo(allBeatmaps));
            assertTotal(results, beatmapSets.Count + allBeatmaps.Length);
        }

        #endregion

        #region Alphabetical grouping

        [Test]
        public async Task TestGroupingByArtist() => await testAlphabeticGroupingMode(GroupMode.Artist, applyArtist);

        [Test]
        public async Task TestGroupingByAuthor() => await testAlphabeticGroupingMode(GroupMode.Author, applyAuthor);

        [Test]
        public async Task TestGroupingByTitle() => await testAlphabeticGroupingMode(GroupMode.Title, applyTitle);

        private async Task testAlphabeticGroupingMode(GroupMode mode, Func<char, Action<BeatmapSetInfo>> applyBeatmap)
        {
            int total = 0;
            var beatmapSets = new List<BeatmapSetInfo>();

            addBeatmapSet(applyBeatmap('4'), beatmapSets, out var fourBeatmap);
            addBeatmapSet(applyBeatmap('5'), beatmapSets, out var fiveBeatmap);
            addBeatmapSet(applyBeatmap('A'), beatmapSets, out var aBeatmap);
            addBeatmapSet(applyBeatmap('F'), beatmapSets, out var fBeatmap);
            addBeatmapSet(applyBeatmap('Z'), beatmapSets, out var zBeatmap);
            addBeatmapSet(applyBeatmap('-'), beatmapSets, out var dashBeatmap);
            addBeatmapSet(applyBeatmap('_'), beatmapSets, out var underscoreBeatmap);

            var results = await runGrouping(mode, beatmapSets);
            assertGroup(results, 0, "0-9", new[] { fiveBeatmap, fourBeatmap }, ref total);
            assertGroup(results, 1, "A", new[] { aBeatmap }, ref total);
            assertGroup(results, 2, "F", new[] { fBeatmap }, ref total);
            assertGroup(results, 3, "Z", new[] { zBeatmap }, ref total);
            assertGroup(results, 4, "Other", new[] { dashBeatmap, underscoreBeatmap }, ref total);
            assertTotal(results, total);
        }

        private Action<BeatmapSetInfo> applyArtist(char first)
        {
            return s => s.Beatmaps[0].Metadata.Artist = $"{first}-artist";
        }

        private Action<BeatmapSetInfo> applyAuthor(char first)
        {
            return s => s.Beatmaps[0].Metadata.Author.Username = $"{first}-author";
        }

        private Action<BeatmapSetInfo> applyTitle(char first)
        {
            return s => s.Beatmaps[0].Metadata.Title = $"{first}-title";
        }

        #endregion

        #region Date grouping

        [Test]
        public async Task TestGroupingByDateAdded()
        {
            int total = 0;

            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(s => s.DateAdded = DateTimeOffset.Now.AddHours(-5), beatmapSets, out var todayBeatmap);
            addBeatmapSet(s => s.DateAdded = DateTimeOffset.Now.AddDays(-1), beatmapSets, out var yesterdayBeatmap);
            addBeatmapSet(s => s.DateAdded = DateTimeOffset.Now.AddDays(-4), beatmapSets, out var lastWeekBeatmap);
            addBeatmapSet(s => s.DateAdded = DateTimeOffset.Now.AddDays(-21), beatmapSets, out var oneMonthBeatmap);
            addBeatmapSet(s => s.DateAdded = DateTimeOffset.Now.AddMonths(-2).AddDays(-3), beatmapSets, out var threeMonthBeatmap);

            var results = await runGrouping(GroupMode.DateAdded, beatmapSets);
            assertGroup(results, 0, "Today", new[] { todayBeatmap }, ref total);
            assertGroup(results, 1, "Yesterday", new[] { yesterdayBeatmap }, ref total);
            assertGroup(results, 2, "Last week", new[] { lastWeekBeatmap }, ref total);
            assertGroup(results, 3, "1 month ago", new[] { oneMonthBeatmap }, ref total);
            assertGroup(results, 4, "3 months ago", new[] { threeMonthBeatmap }, ref total);
            assertTotal(results, total);
        }

        [Test]
        public async Task TestGroupingByLastPlayed()
        {
            int total = 0;

            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(applyLastPlayed(DateTimeOffset.Now.AddHours(-5)), beatmapSets, out var todayBeatmap);
            addBeatmapSet(applyLastPlayed(DateTimeOffset.Now.AddDays(-1)), beatmapSets, out var yesterdayBeatmap);
            addBeatmapSet(applyLastPlayed(DateTimeOffset.Now.AddDays(-4)), beatmapSets, out var lastWeekBeatmap);
            addBeatmapSet(applyLastPlayed(DateTimeOffset.Now.AddDays(-21)), beatmapSets, out var oneMonthBeatmap);
            addBeatmapSet(applyLastPlayed(DateTimeOffset.Now.AddMonths(-2).AddDays(-3)), beatmapSets, out var threeMonthBeatmap);
            addBeatmapSet(applyLastPlayed(null), beatmapSets, out var neverBeatmap);

            var results = await runGrouping(GroupMode.LastPlayed, beatmapSets);
            assertGroup(results, 0, "Today", new[] { todayBeatmap }, ref total);
            assertGroup(results, 1, "Yesterday", new[] { yesterdayBeatmap }, ref total);
            assertGroup(results, 2, "Last week", new[] { lastWeekBeatmap }, ref total);
            assertGroup(results, 3, "1 month ago", new[] { oneMonthBeatmap }, ref total);
            assertGroup(results, 4, "3 months ago", new[] { threeMonthBeatmap }, ref total);
            assertGroup(results, 5, "Never", new[] { neverBeatmap }, ref total);
            assertTotal(results, total);
        }

        [Test]
        public async Task TestGroupingByLastPlayed_BeatmapPartiallyPlayed()
        {
            var set = TestResources.CreateTestBeatmapSetInfo(3);
            set.Beatmaps[0].LastPlayed = null;
            set.Beatmaps[1].LastPlayed = null;
            set.Beatmaps[2].LastPlayed = DateTimeOffset.Now;

            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo> { set };

            var results = await runGrouping(GroupMode.LastPlayed, beatmapSets);
            int total = 0;

            assertGroup(results, 0, "Today", new[] { set }, ref total);
            assertTotal(results, total);
        }

        [Test]
        public async Task TestGroupingByLastPlayed_NeverBelowOverFiveMonthsAgo()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(applyLastPlayed(null), beatmapSets, out var neverBeatmap);
            addBeatmapSet(applyLastPlayed(DateTimeOffset.Now.AddMonths(-6)), beatmapSets, out var overFiveMonthsBeatmap);

            var results = await runGrouping(GroupMode.LastPlayed, beatmapSets);
            int total = 0;

            assertGroup(results, 0, "Over 5 months ago", new[] { overFiveMonthsBeatmap }, ref total);
            assertGroup(results, 1, "Never", new[] { neverBeatmap }, ref total);
            assertTotal(results, total);
        }

        private Action<BeatmapSetInfo> applyLastPlayed(DateTimeOffset? lastPlayed)
        {
            return s => s.Beatmaps.ForEach(b => b.LastPlayed = lastPlayed);
        }

        #endregion

        #region Ranked Status

        [Test]
        public async Task TestGroupingByRankedStatus()
        {
            int total = 0;

            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.Ranked, beatmapSets, out var rankedBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.Approved, beatmapSets, out var approvedBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.Qualified, beatmapSets, out var qualifiedBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.Loved, beatmapSets, out var lovedBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.WIP, beatmapSets, out var wipBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.Pending, beatmapSets, out var pendingBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.Graveyard, beatmapSets, out var graveyardBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.None, beatmapSets, out var noneBeatmap);
            addBeatmapSet(s => s.Status = BeatmapOnlineStatus.LocallyModified, beatmapSets, out var localBeatmap);

            var results = await runGrouping(GroupMode.RankedStatus, beatmapSets);
            assertGroup(results, 0, "Ranked", new[] { rankedBeatmap, approvedBeatmap }, ref total);
            assertGroup(results, 1, "Qualified", new[] { qualifiedBeatmap }, ref total);
            assertGroup(results, 2, "WIP", new[] { wipBeatmap }, ref total);
            assertGroup(results, 3, "Pending", new[] { pendingBeatmap }, ref total);
            assertGroup(results, 4, "Graveyard", new[] { graveyardBeatmap }, ref total);
            assertGroup(results, 5, "Local", new[] { localBeatmap }, ref total);
            assertGroup(results, 6, "Unknown", new[] { noneBeatmap }, ref total);
            assertGroup(results, 7, "Loved", new[] { lovedBeatmap }, ref total);
            assertTotal(results, total);
        }

        #endregion

        #region BPM grouping

        [Test]
        public async Task TestGroupingByBPM()
        {
            int total = 0;

            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(applyBPM(30), beatmapSets, out var beatmap30);
            addBeatmapSet(applyBPM(60), beatmapSets, out var beatmap60);
            addBeatmapSet(applyBPM(90), beatmapSets, out var beatmap90);
            addBeatmapSet(applyBPM(120), beatmapSets, out var beatmap120);
            addBeatmapSet(applyBPM(270), beatmapSets, out var beatmap270);
            addBeatmapSet(applyBPM(300), beatmapSets, out var beatmap300);
            addBeatmapSet(applyBPM(330), beatmapSets, out var beatmap330);

            var results = await runGrouping(GroupMode.BPM, beatmapSets);
            assertGroup(results, 0, "Under 60 BPM", new[] { beatmap30 }, ref total);
            assertGroup(results, 1, "Under 120 BPM", new[] { beatmap60, beatmap90 }, ref total);
            assertGroup(results, 2, "Under 180 BPM", new[] { beatmap120 }, ref total);
            assertGroup(results, 3, "Under 300 BPM", new[] { beatmap270 }, ref total);
            assertGroup(results, 4, "Over 300 BPM", new[] { beatmap300, beatmap330 }, ref total);
            assertTotal(results, total);
        }

        private Action<BeatmapSetInfo> applyBPM(double bpm)
        {
            return s => s.Beatmaps.ForEach(b => b.BPM = bpm);
        }

        #endregion

        #region Difficulty grouping

        [Test]
        public async Task TestGroupingByDifficulty()
        {
            int total = 0;

            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(applyStars(0.5), beatmapSets, out var beatmapBelow1);
            addBeatmapSet(applyStars(1.9), beatmapSets, out var beatmapAbove1);
            addBeatmapSet(applyStars(1.995), beatmapSets, out var beatmapAlmost2);
            addBeatmapSet(applyStars(2), beatmapSets, out var beatmap2);
            addBeatmapSet(applyStars(2.1), beatmapSets, out var beatmapAbove2);
            addBeatmapSet(applyStars(7), beatmapSets, out var beatmap7);

            var results = await runGrouping(GroupMode.Difficulty, beatmapSets);
            assertGroup(results, 0, "Below 1 Star", new[] { beatmapBelow1 }, ref total);
            assertGroup(results, 1, "1 Star", new[] { beatmapAbove1 }, ref total);
            assertGroup(results, 2, "2 Stars", new[] { beatmapAlmost2, beatmap2, beatmapAbove2 }, ref total);
            assertGroup(results, 3, "7 Stars", new[] { beatmap7 }, ref total);
            assertTotal(results, total);
        }

        private Action<BeatmapSetInfo> applyStars(double stars)
        {
            return s => s.Beatmaps.ForEach(b => b.StarRating = stars);
        }

        #endregion

        #region Length grouping

        [Test]
        public async Task TestGroupingByLength()
        {
            int total = 0;

            var beatmapSets = new List<BeatmapSetInfo>();
            addBeatmapSet(applyLength(30_000), beatmapSets, out var beatmap30Sec);
            addBeatmapSet(applyLength(60_000), beatmapSets, out var beatmap1Min);
            addBeatmapSet(applyLength(90_000), beatmapSets, out var beatmap1Min30Sec);
            addBeatmapSet(applyLength(120_000), beatmapSets, out var beatmap2Min);
            addBeatmapSet(applyLength(300_000), beatmapSets, out var beatmap5Min);
            addBeatmapSet(applyLength(360_000), beatmapSets, out var beatmap6Min);
            addBeatmapSet(applyLength(600_000), beatmapSets, out var beatmap10Min);
            addBeatmapSet(applyLength(630_000), beatmapSets, out var beatmap10Min30Sec);

            var results = await runGrouping(GroupMode.Length, beatmapSets);
            assertGroup(results, 0, "1 minute or less", new[] { beatmap30Sec, beatmap1Min }, ref total);
            assertGroup(results, 1, "2 minutes or less", new[] { beatmap1Min30Sec, beatmap2Min }, ref total);
            assertGroup(results, 2, "5 minutes or less", new[] { beatmap5Min }, ref total);
            assertGroup(results, 3, "10 minutes or less", new[] { beatmap6Min, beatmap10Min }, ref total);
            assertGroup(results, 4, "Over 10 minutes", new[] { beatmap10Min30Sec }, ref total);
            assertTotal(results, total);
        }

        private Action<BeatmapSetInfo> applyLength(double length)
        {
            return s => s.Beatmaps.ForEach(b => b.Length = length);
        }

        #endregion

        private static async Task<List<CarouselItem>> runGrouping(GroupMode group, List<BeatmapSetInfo> beatmapSets)
        {
            var groupingFilter = new BeatmapCarouselFilterGrouping(() => new FilterCriteria { Group = group });
            var carouselItems = await groupingFilter.Run(beatmapSets.SelectMany(s => s.Beatmaps.Select(b => new CarouselItem(b))).ToList(), CancellationToken.None);

            // sanity check to ensure no detection of two group items with equal order value.
            var groups = carouselItems.Select(i => i.Model).OfType<GroupDefinition>();

            foreach (var header in groups)
            {
                var sameOrder = groups.FirstOrDefault(g => g != header && g.Order == header.Order);
                if (sameOrder != null)
                    Assert.Fail($"Detected two groups with equal order number: \"{header.Title}\" vs. \"{sameOrder.Title}\"");
            }

            return carouselItems;
        }

        private static void assertGroup(List<CarouselItem> items, int index, string expectedTitle, IEnumerable<BeatmapSetInfo> expectedBeatmapSets, ref int totalItems)
        {
            var groupItem = items.Where(i => i.Model is GroupDefinition).ElementAtOrDefault(index);

            if (groupItem == null)
            {
                Assert.Fail($"Expected group at index {index}, but that is out of bounds");
                return;
            }

            var itemsInGroup = items.SkipWhile(i => i != groupItem).Skip(1).TakeWhile(i => i.Model is not GroupDefinition);

            var groupModel = (GroupDefinition)groupItem.Model;

            Assert.That(groupModel.Title, Is.EqualTo(expectedTitle));
            Assert.That(itemsInGroup.Select(i => i.Model).OfType<BeatmapInfo>(), Is.EquivalentTo(expectedBeatmapSets.SelectMany(bs => bs.Beatmaps)));

            totalItems += itemsInGroup.Count() + 1;
        }

        private static void assertTotal(List<CarouselItem> items, int total)
        {
            Assert.That(items.Count, Is.EqualTo(total));
        }

        private static void addBeatmapSet(Action<BeatmapSetInfo> change, List<BeatmapSetInfo> list, out BeatmapSetInfo added)
        {
            var set = TestResources.CreateTestBeatmapSetInfo();
            change(set);
            list.Add(set);
            added = set;
        }
    }
}
