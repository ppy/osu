// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.SongSelect
{
    public partial class DifficultySearchTest : OsuTestScene
    {
        private static readonly (string title, string difficultyName)[] beatmaps =
        {
            ("Title1", "Diff1"),
            ("Title1", "Diff2"),
            ("My[Favourite]Song", "Expert"),
            ("Title", "My Favourite Diff"),
            ("Another One", "diff ]with [[ brackets]]]"),
        };

        [TestCase("[1]", new[] { 0 })]
        [TestCase("[1", new[] { 0 })]
        [TestCase("My[Favourite", new[] { 2 })]
        [TestCase("My[Favourite]", new[] { 2 })]
        [TestCase("My[Favourite]Song", new[] { 2 })]
        [TestCase("Favourite]", new[] { 2 })]
        [TestCase("[Diff", new[] { 0, 1, 3, 4 })]
        [TestCase("[Diff]", new[] { 0, 1, 3, 4 })]
        [TestCase("[Favourite]", new[] { 3 })]
        [TestCase("Title1 [Diff]", new[] { 0, 1 })]
        [TestCase("Title1[Diff]", new int[] { })]
        [TestCase("[diff ]with]", new[] { 4 })]
        [TestCase("[diff ]with [[ brackets]]]]", new[] { 4 })]
        [TestCase("[diff] another [brackets]", new[] { 4 })]
        public void TestDifficultySearch(string query, int[] expectedBeatmapIndexes)
        {
            var carouselBeatmaps = createCarouselBeatmaps().ToList();

            AddStep("filter beatmaps", () =>
            {
                var criteria = new FilterCriteria();
                FilterQueryParser.ApplyQueries(criteria, query);
                carouselBeatmaps.ForEach(b => b.Filter(criteria));
            });

            AddAssert("filtered correctly", () => carouselBeatmaps.All(b =>
            {
                int index = carouselBeatmaps.IndexOf(b);

                bool filtered = b.Filtered.Value;

                return filtered != expectedBeatmapIndexes.Contains(index);
            }));
        }

        private static IEnumerable<CarouselBeatmap> createCarouselBeatmaps()
        {
            return beatmaps.Select(info => new CarouselBeatmap(new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = info.title
                },
                DifficultyName = info.difficultyName
            }));
        }
    }
}
