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
    public partial class BeatmapCarouselFilterSortingTest
    {
        [Test]
        public async Task TestSorting()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            const string zzz_lowercase = "zzzzz";
            const string zzz_uppercase = "ZZZZZ";
            const int diff_count = 5;

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

                beatmapSets.Add(set);
            }

            var results = await runSorting(SortMode.Author, beatmapSets);

            Assert.That(results.Last().Metadata.Author.Username, Is.EqualTo(zzz_uppercase));
            Assert.That(results.SkipLast(diff_count).Last().Metadata.Author.Username, Is.EqualTo(zzz_lowercase));

            results = await runSorting(SortMode.Artist, beatmapSets);

            Assert.That(results.Last().Metadata.Artist, Is.EqualTo(zzz_uppercase));
            Assert.That(results.SkipLast(diff_count).Last().Metadata.Artist, Is.EqualTo(zzz_lowercase));
        }

        [Test]
        public async Task TestSortingDateSubmitted()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            const string zzz_string = "zzzzz";

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

                beatmapSets.Add(set);
            }

            var results = await runSorting(SortMode.DateSubmitted, beatmapSets);

            Assert.That(results.Count(), Is.EqualTo(50));

            Assert.That(results.Reverse().TakeWhile(b => b.BeatmapSet!.DateSubmitted == null).Count(), Is.EqualTo(20), () => "missing dates should be at the end");
            Assert.That(results.TakeWhile(b => b.BeatmapSet!.DateSubmitted != null).Count(), Is.EqualTo(30), () => "non-missing dates should be at the start");
        }

        [Test]
        public async Task TestSortByArtistUsesTitleAsTiebreaker()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            const int diff_count = 5;

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

                beatmapSets.Add(set);
            }

            var results = await runSorting(SortMode.Artist, beatmapSets);

            Assert.That(() =>
            {
                var lastItem = results.Last();
                return lastItem.Metadata.Artist == "ZZZ" && lastItem.Metadata.Title == "ZZZ";
            });

            Assert.That(() =>
            {
                var secondLastItem = results.SkipLast(diff_count).Last();
                return secondLastItem.Metadata.Artist == "ZZZ" && secondLastItem.Metadata.Title == "AAA";
            });
        }

        /// <summary>
        /// Ensures that difficulty sorting still works and that any other sort mode will sort first by
        /// itself then by the starrating
        /// </summary>
        [Test]
        public async Task TestSeparateAllDifficultySort()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();
            int amountOfSets = 4;
            int amountOfBeatmaps = 2;

            for (int i = 0; i < amountOfSets; i++)
            {
                int numberMetadata = (i - i % 2) / 2;
                DateTimeOffset dateTimeOffset = new DateTimeOffset(2000, 1, 1, 12, 1, numberMetadata, new TimeSpan());

                var set = new BeatmapSetInfo
                {
                    ID = Guid.NewGuid(),
                    DateAdded = dateTimeOffset,
                    DateRanked = dateTimeOffset,
                    DateSubmitted = dateTimeOffset,
                };

                for (int j = 0; j < amountOfBeatmaps; j++)
                {
                    var metadata = new BeatmapMetadata
                    {
                        Artist = $"{numberMetadata}. Artist",
                        Title = $"{numberMetadata}. beatmap",
                        Author = { Username = $"{numberMetadata}. Guy " },
                        Source = $"{numberMetadata}. source",
                    };

                    double starRating = 0;
                    //Creates a stable pattern of "unsorted" starratings (0, n-1, 2, n-3, 4, n-5)
                    if (amountOfBeatmaps % 2 == 0)
                        starRating = j % 2 == 0 ? j : amountOfBeatmaps - j;
                    else
                        starRating = j % 2 == 0 ? j : amountOfBeatmaps - 1 - j;
                    double BPM = numberMetadata * 30;
                    double Length = numberMetadata * 20;

                    BeatmapInfo beatmapInfo = new BeatmapInfo()
                    {
                        StarRating = starRating,
                        ID = Guid.NewGuid(),
                        Metadata = metadata,
                        BeatmapSet = set,
                        BPM = BPM,
                        LastPlayed = dateTimeOffset,
                        Length = numberMetadata,
                    };

                    set.Beatmaps.Add(beatmapInfo);
                }

                beatmapSets.Add(set);
            }

            foreach (var sortMode in Enum.GetValues<SortMode>())
            {
                if (sortMode == SortMode.Difficulty)
                    continue;

                var sortModeResults = await runSorting(sortMode, true, beatmapSets);

                Assert.That(sortModeResults.Count(), Is.EqualTo(amountOfSets * amountOfBeatmaps));

                for (int i = 0; i < amountOfSets * amountOfBeatmaps; ++i)
                {
                    Assert.That(sortModeResults.Skip(i).First().StarRating, Is.EqualTo(i % amountOfBeatmaps), () => $"{sortMode} incorrectly sorts.");
                }
            }

            var difficultyResults = await runSorting(SortMode.Difficulty, true, beatmapSets);

            Assert.That(difficultyResults.Count(), Is.EqualTo(amountOfSets * amountOfBeatmaps));

            for (int i = 0; i < amountOfSets * amountOfBeatmaps; ++i)
            {
                double correctStarRating = (i - (i % amountOfSets)) / amountOfSets;
                Assert.That(difficultyResults.Skip(i).First().StarRating, Is.EqualTo(correctStarRating), () => $"{SortMode.Difficulty} incorrectly sorts.");
            }
        }

        /// <summary>
        /// Ensures stability is maintained on different sort modes for items with equal properties.
        /// </summary>
        [Test]
        public async Task TestSortingStabilityDateAdded()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            for (int i = 0; i < 10; i++)
            {
                var set = TestResources.CreateTestBeatmapSetInfo();

                set.DateAdded = DateTimeOffset.FromUnixTimeSeconds(i);

                // only need to set the first as they are a shared reference.
                var beatmap = set.Beatmaps.First();

                beatmap.Metadata.Artist = "a";
                beatmap.Metadata.Title = "b";

                beatmapSets.Add(set);
            }

            var results = await runSorting(SortMode.Title, beatmapSets);

            Assert.That(results.Select(b => b.BeatmapSet!.DateAdded), Is.Ordered.Descending);

            results = await runSorting(SortMode.Artist, beatmapSets);

            Assert.That(results.Select(b => b.BeatmapSet!.DateAdded), Is.Ordered.Descending);
        }

        private static async Task<IEnumerable<BeatmapInfo>> runSorting(SortMode sort, List<BeatmapSetInfo> beatmapSets)
        {
            var sorter = new BeatmapCarouselFilterSorting(() => new FilterCriteria { Sort = sort });
            var carouselItems = await sorter.Run(beatmapSets.SelectMany(s => s.Beatmaps.Select(b => new CarouselItem(b))), CancellationToken.None);
            return carouselItems.Select(ci => ci.Model).OfType<BeatmapInfo>();
        }

        private static async Task<IEnumerable<BeatmapInfo>> runSorting(SortMode sort, bool separateAllDifficulties, List<BeatmapSetInfo> beatmapSets)
        {
            var sorter = new BeatmapCarouselFilterSorting(() => new FilterCriteria { Sort = sort, SeparateAllDifficulties = separateAllDifficulties });
            var carouselItems = await sorter.Run(beatmapSets.SelectMany(s => s.Beatmaps.Select(b => new CarouselItem(b))), CancellationToken.None);
            return carouselItems.Select(ci => ci.Model).OfType<BeatmapInfo>();
        }
    }
}
