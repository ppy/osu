// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapCarouselSorting : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
        }

        [Test]
        public void TestSorting()
        {
            const string zzz_lowercase = "zzzzz";
            const string zzz_uppercase = "ZZZZZ";
            const int diff_count = 5;

            AddStep("Populuate beatmap sets", () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diff_count);

                    if (i == 4)
                        set.Beatmaps.ForEach(b => b.Metadata.Artist = zzz_uppercase);

                    if (i == 8)
                        set.Beatmaps.ForEach(b => b.Metadata.Artist = zzz_lowercase);

                    if (i == 12)
                        set.Beatmaps.ForEach(b => b.Metadata.Author.Username = zzz_uppercase);

                    if (i == 16)
                        set.Beatmaps.ForEach(b => b.Metadata.Author.Username = zzz_lowercase);

                    BeatmapSets.Add(set);
                }
            });

            SortBy(SortMode.Author);
            WaitForFiltering();

            AddAssert($"Check {zzz_uppercase} is last", () => Sorting.SortedBeatmaps.Last().Metadata.Author.Username == zzz_uppercase);
            AddAssert($"Check {zzz_lowercase} is second last", () => Sorting.SortedBeatmaps.SkipLast(diff_count).Last().Metadata.Author.Username == zzz_lowercase);

            SortBy(SortMode.Artist);
            WaitForFiltering();

            AddAssert($"Check {zzz_uppercase} is last", () => Sorting.SortedBeatmaps.Last().Metadata.Artist == zzz_uppercase);
            AddAssert($"Check {zzz_lowercase} is second last", () => Sorting.SortedBeatmaps.SkipLast(diff_count).Last().Metadata.Artist == zzz_lowercase);
        }

        [Test]
        public void TestSortingDateSubmitted()
        {
            const string zzz_string = "zzzzz";

            AddStep("Populuate beatmap sets", () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(5);

                    // A total of 6 sets have date submitted (4 don't)
                    // A total of 5 sets have artist string (3 of which also have date submitted)

                    if (i >= 2 && i < 8) // i = 2, 3, 4, 5, 6, 7 have submitted date
                        set.DateSubmitted = DateTimeOffset.Now.AddMinutes(i);
                    if (i < 5) // i = 0, 1, 2, 3, 4 have matching string
                        set.Beatmaps.ForEach(b => b.Metadata.Artist = zzz_string);

                    set.Beatmaps.ForEach(b => b.Metadata.Title = $"submitted: {set.DateSubmitted}");

                    BeatmapSets.Add(set);
                }
            });

            SortBy(SortMode.DateSubmitted);
            WaitForFiltering();

            CheckDisplayedBeatmapSetsCount(10);
            CheckDisplayedBeatmapsCount(50);

            AddAssert("missing date are at end",
                () => Sorting.SortedBeatmaps.Reverse().TakeWhile(b => b.BeatmapSet!.DateSubmitted == null).Count(),
                () => Is.EqualTo(20));
            AddAssert("rest are at start",
                () => Sorting.SortedBeatmaps.TakeWhile(b => b.BeatmapSet!.DateSubmitted != null).Count(),
                () => Is.EqualTo(30));

            ApplyToFilter($"search {zzz_string}", c => c.SearchText = zzz_string);
            WaitForFiltering();

            CheckDisplayedBeatmapSetsCount(5);
            CheckDisplayedBeatmapsCount(25);

            AddAssert("missing date are at end",
                () => Sorting.SortedBeatmaps.Reverse().TakeWhile(b => b.BeatmapSet!.DateSubmitted == null).Count(),
                () => Is.EqualTo(10));
            AddAssert("rest are at start",
                () => Sorting.SortedBeatmaps.TakeWhile(b => b.BeatmapSet!.DateSubmitted != null).Count(),
                () => Is.EqualTo(15));
        }

        [Test]
        public void TestSortByArtistUsesTitleAsTiebreaker()
        {
            const int diff_count = 5;

            AddStep("Populuate beatmap sets", () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diff_count);

                    if (i == 4)
                    {
                        set.Beatmaps.ForEach(b =>
                        {
                            b.Metadata.Artist = "ZZZ";
                            b.Metadata.Title = "AAA";
                        });
                    }

                    if (i == 8)
                    {
                        set.Beatmaps.ForEach(b =>
                        {
                            b.Metadata.Artist = "ZZZ";
                            b.Metadata.Title = "ZZZ";
                        });
                    }

                    BeatmapSets.Add(set);
                }
            });

            SortBy(SortMode.Artist);
            WaitForFiltering();

            AddAssert("Check last item", () =>
            {
                var lastItem = Sorting.SortedBeatmaps.Last();
                return lastItem.Metadata.Artist == "ZZZ" && lastItem.Metadata.Title == "ZZZ";
            });

            AddAssert("Check second last item", () =>
            {
                var secondLastItem = Sorting.SortedBeatmaps.SkipLast(diff_count).Last();
                return secondLastItem.Metadata.Artist == "ZZZ" && secondLastItem.Metadata.Title == "AAA";
            });
        }

        /// <summary>
        /// Ensures stability is maintained on different sort modes for items with equal properties.
        /// </summary>
        [Test]
        public void TestSortingStabilityDateAdded()
        {
            AddStep("Populuate beatmap sets", () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo();

                    set.DateAdded = DateTimeOffset.FromUnixTimeSeconds(i);

                    // only need to set the first as they are a shared reference.
                    var beatmap = set.Beatmaps.First();

                    beatmap.Metadata.Artist = "a";
                    beatmap.Metadata.Title = "b";

                    BeatmapSets.Add(set);
                }
            });

            SortBy(SortMode.Title);
            WaitForFiltering();

            AddAssert("Items remain in descending added order", () => Sorting.SortedBeatmaps.Select(b => b.BeatmapSet!.DateAdded), () => Is.Ordered.Descending);

            SortBy(SortMode.Artist);
            WaitForFiltering();

            AddAssert("Items remain in descending added order", () => Sorting.SortedBeatmaps.Select(b => b.BeatmapSet!.DateAdded), () => Is.Ordered.Descending);
        }

        /// <summary>
        /// Ensures stability is maintained on different sort modes while a new item is added to the carousel.
        /// </summary>
        [Test]
        public void TestSortingStabilityWithRemovedAndReaddedItem()
        {
            const int diff_count = 5;

            AddStep("Populuate beatmap sets", () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diff_count);

                    // only need to set the first as they are a shared reference.
                    var beatmap = set.Beatmaps.First();

                    beatmap.Metadata.Artist = "same artist";
                    beatmap.Metadata.Title = "same title";

                    // testing the case where DateAdded happens to equal (quite rare).
                    set.DateAdded = DateTimeOffset.UnixEpoch;

                    BeatmapSets.Add(set);
                }
            });

            BeatmapSetInfo removedBeatmap = null!;
            Guid[] originalOrder = null!;

            SortBy(SortMode.Artist);
            WaitForFiltering();

            AddAssert("Items in descending added order", () => Sorting.SortedBeatmaps.Select(b => b.BeatmapSet!.DateAdded), () => Is.Ordered.Descending);
            AddStep("Save order", () => originalOrder = Sorting.SortedBeatmaps.Select(b => b.ID).ToArray());

            AddStep("Remove item", () =>
            {
                removedBeatmap = BeatmapSets[1];
                BeatmapSets.RemoveAt(1);
            });
            AddStep("Re-add item", () => BeatmapSets.Insert(1, removedBeatmap));
            WaitForFiltering();

            AddAssert("Order didn't change", () => Sorting.SortedBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));

            SortBy(SortMode.Title);
            WaitForFiltering();

            AddAssert("Order didn't change", () => Sorting.SortedBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));
        }

        /// <summary>
        /// Ensures stability is maintained on different sort modes while a new item is added to the carousel.
        /// </summary>
        [Test]
        public void TestSortingStabilityWithNewItems()
        {
            const int diff_count = 5;

            AddStep("Populuate beatmap sets", () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diff_count);

                    // only need to set the first as they are a shared reference.
                    var beatmap = set.Beatmaps.First();

                    beatmap.Metadata.Artist = "same artist";
                    beatmap.Metadata.Title = "same title";

                    // testing the case where DateAdded happens to equal (quite rare).
                    set.DateAdded = DateTimeOffset.UnixEpoch;

                    BeatmapSets.Add(set);
                }
            });

            Guid[] originalOrder = null!;

            SortBy(SortMode.Artist);
            WaitForFiltering();

            AddAssert("Items in descending added order", () => Sorting.SortedBeatmaps.Select(b => b.BeatmapSet!.DateAdded), () => Is.Ordered.Descending);
            AddStep("Save order", () => originalOrder = Sorting.SortedBeatmaps.Select(b => b.ID).ToArray());

            AddStep("Add new item", () =>
            {
                var set = TestResources.CreateTestBeatmapSetInfo();

                // only need to set the first as they are a shared reference.
                var beatmap = set.Beatmaps.First();

                beatmap.Metadata.Artist = "same artist";
                beatmap.Metadata.Title = "same title";

                set.DateAdded = DateTimeOffset.FromUnixTimeSeconds(1);

                BeatmapSets.Add(set);

                // add set to expected ordering
                originalOrder = set.Beatmaps.Select(b => b.ID).Concat(originalOrder).ToArray();
            });
            WaitForFiltering();

            AddAssert("Order didn't change", () => Sorting.SortedBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));

            SortBy(SortMode.Title);
            WaitForFiltering();

            AddAssert("Order didn't change", () => Sorting.SortedBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));
        }
    }
}
