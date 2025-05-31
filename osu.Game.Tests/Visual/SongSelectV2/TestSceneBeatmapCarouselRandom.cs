// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselRandom : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
        }

        [Test]
        public void TestRandomObeysFiltering()
        {
            AddBeatmaps(2, 10, true);

            ApplyToFilter("filter", c => c.SearchText = BeatmapSets[0].Beatmaps.Last().DifficultyName);
            WaitForFiltering();

            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(1);

            for (int i = 0; i < 10; i++)
            {
                nextRandom();
                WaitForSelection(0, 9);
            }
        }

        /// <summary>
        /// Test random non-repeating algorithm
        /// </summary>
        [Test]
        public void TestRandomArtistGrouping()
        {
            SortAndGroupBy(SortMode.Artist, GroupMode.Artist);

            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();

            nextRandom();
            ensureRandomDidNotRepeat();
            nextRandom();
            ensureRandomDidNotRepeat();
            nextRandom();
            ensureRandomDidNotRepeat();

            prevRandom();
            checkRewindCorrectSet();
            prevRandom();
            checkRewindCorrectSet();

            nextRandom();
            ensureRandomDidNotRepeat();
            nextRandom();
            ensureRandomDidNotRepeat();

            nextRandom();
            AddAssert("ensure repeat", () => BeatmapSetRequestedSelections.Contains(Carousel.SelectedBeatmapSet!));
        }

        /// <summary>
        /// Test random non-repeating algorithm
        /// </summary>
        [Test]
        public void TestRandomDifficultyGrouping()
        {
            SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);

            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();

            nextRandom();
            ensureRandomDidNotRepeat();
            nextRandom();
            ensureRandomDidNotRepeat();
            nextRandom();
            ensureRandomDidNotRepeat();

            prevRandom();
            checkRewindCorrectSet();
            prevRandom();
            checkRewindCorrectSet();

            nextRandom();
            ensureRandomDidNotRepeat();
            nextRandom();
            ensureRandomDidNotRepeat();

            nextRandom();
            AddAssert("ensure repeat", () => BeatmapSetRequestedSelections.Contains(Carousel.SelectedBeatmapSet!));
        }

        [Test]
        public void TestRewindOverMultipleIterations()
        {
            const int local_set_count = 3;
            const int random_select_count = local_set_count * 3;

            AddBeatmaps(local_set_count, 3, true);
            WaitForDrawablePanels();

            SelectNextGroup();

            for (int i = 0; i < random_select_count; i++)
                nextRandom();

            for (int i = 0; i < random_select_count; i++)
            {
                prevRandom();
                checkRewindCorrectSet();
            }
        }

        [Test]
        public void TestRewindToDeletedBeatmap()
        {
            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();

            BeatmapInfo? originalSelected = null;
            BeatmapInfo? postRandomSelection = null;

            nextRandom();

            CheckHasSelection();
            AddStep("store selection", () => originalSelected = (BeatmapInfo)Carousel.CurrentSelection!);

            nextRandom();
            AddStep("store selection", () => postRandomSelection = (BeatmapInfo)Carousel.CurrentSelection!);

            AddAssert("selection changed", () => originalSelected, () => Is.Not.SameAs(postRandomSelection));

            AddStep("delete previous selection beatmaps", () => BeatmapSets.Remove(originalSelected!.BeatmapSet!));
            WaitForFiltering();

            AddAssert("selection not changed", () => Carousel.CurrentSelection, () => Is.EqualTo(postRandomSelection));

            prevRandom();
            AddAssert("selection not changed", () => Carousel.CurrentSelection, () => Is.EqualTo(postRandomSelection));
        }

        private void nextRandom() =>
            AddStep("select random next", () => Carousel.NextRandom());

        private void ensureRandomDidNotRepeat() =>
            AddAssert("no repeats", () => BeatmapSetRequestedSelections.Distinct().Count(), () => Is.EqualTo(BeatmapSetRequestedSelections.Count));

        private void checkRewindCorrectSet() =>
            AddAssert("rewind matched expected set", () => BeatmapSetRequestedSelections.Peek(), () => Is.EqualTo(Carousel.SelectedBeatmapSet));

        private void prevRandom() => AddStep("select random last", () =>
        {
            Carousel.PreviousRandom();
            BeatmapSetRequestedSelections.Pop();
        });
    }
}
