// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

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

            ApplyToFilterAndWaitForFilter("filter", c => c.SearchText = BeatmapSets[0].Beatmaps.Last().DifficultyName);

            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(1);

            for (int i = 0; i < 10; i++)
            {
                nextRandom();
                WaitForSetSelection(0, 9);
            }
        }

        [Test]
        public void TestGroupingModeChangeStillWorks()
        {
            BeatmapInfo originalSelected = null!;
            GroupDefinition? expanded = null;

            SortAndGroupBy(SortMode.Artist, GroupMode.Artist);
            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();

            nextRandom();
            ensureRandomDidNotRepeat();

            AddStep("store selection", () => originalSelected = Carousel.CurrentBeatmap!);

            SortAndGroupBy(SortMode.Artist, GroupMode.Difficulty);
            WaitForFiltering();

            AddAssert("selection not changed", () => Carousel.CurrentBeatmap, () => Is.EqualTo(originalSelected));

            storeExpandedGroup();

            for (int i = 0; i < 5; i++)
            {
                nextRandom();
                ensureRandomDidNotRepeat();
                checkExpandedGroupUnchanged();
            }

            SortAndGroupBy(SortMode.Artist, GroupMode.None);
            WaitForFiltering();

            for (int i = 0; i < 5; i++)
            {
                nextRandom();
                ensureRandomDidNotRepeat();
            }

            void storeExpandedGroup() => AddStep("store open group", () => expanded = Carousel.ExpandedGroup);

            void checkExpandedGroupUnchanged() => AddAssert("expanded did not change", () => Carousel.ExpandedGroup, () => Is.EqualTo(expanded));
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

            GroupDefinition? expanded = null;

            for (int i = 0; i < 2; i++)
            {
                nextRandom();
                expanded ??= storeExpandedGroup();

                ensureSetRandomDidNotRepeat();
                checkExpandedGroupUnchanged();
            }

            nextRandom();
            ensureSetRandomDidRepeat();
            checkExpandedGroupUnchanged();

            prevRandomSet();
            checkRewindCorrectSet();
            checkExpandedGroupUnchanged();
            prevRandomSet();
            checkRewindCorrectSet();
            checkExpandedGroupUnchanged();

            nextRandom();
            ensureSetRandomDidNotRepeat();
            checkExpandedGroupUnchanged();
            nextRandom();
            ensureSetRandomDidRepeat();
            checkExpandedGroupUnchanged();

            GroupDefinition? storeExpandedGroup()
            {
                AddStep("store open group", () => expanded = Carousel.ExpandedGroup);
                return null;
            }

            void checkExpandedGroupUnchanged() => AddAssert("expanded did not change", () => Carousel.ExpandedGroup, () => Is.EqualTo(expanded));
        }

        /// <summary>
        /// Test random non-repeating algorithm
        /// </summary>
        [Test]
        public void TestRandomDifficultyGroupingRewindsCorrectly()
        {
            SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);

            AddBeatmaps(3, 3, true);
            WaitForDrawablePanels();

            GroupDefinition? expanded = null;

            for (int i = 0; i < 3; i++)
            {
                nextRandom();
                expanded ??= storeExpandedGroup();

                ensureRandomDidNotRepeat();
                checkExpandedGroupUnchanged();
            }

            for (int i = 0; i < 2; i++)
            {
                prevRandom();
                checkRewindCorrect();
                checkExpandedGroupUnchanged();
            }

            for (int i = 0; i < 2; i++)
            {
                nextRandom();
                ensureRandomDidNotRepeat();
                checkExpandedGroupUnchanged();
            }

            nextRandom();
            ensureRandomDidRepeat();
            checkExpandedGroupUnchanged();

            GroupDefinition? storeExpandedGroup()
            {
                AddStep("store open group", () => expanded = Carousel.ExpandedGroup);
                return null;
            }

            void checkExpandedGroupUnchanged() => AddAssert("expanded did not change", () => Carousel.ExpandedGroup, () => Is.EqualTo(expanded));
        }

        /// <summary>
        /// Test random non-repeating algorithm
        /// </summary>
        [Test]
        public void TestRandomDifficultyGroupingRepeatsWhenExhausted()
        {
            SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);

            AddBeatmaps(3, 3, true);
            WaitForDrawablePanels();

            GroupDefinition? expanded = null;

            for (int i = 0; i < 3; i++)
            {
                nextRandom();
                expanded ??= storeExpandedGroup();

                ensureRandomDidNotRepeat();
                checkExpandedGroupUnchanged();
            }

            for (int i = 0; i < 3; i++)
            {
                nextRandom();
                ensureRandomDidRepeat();
            }

            for (int i = 0; i < 5; i++)
            {
                prevRandom();
                checkRewindCorrect();
                checkExpandedGroupUnchanged();
            }

            nextRandom();
            checkExpandedGroupUnchanged();
            // can't assert repeat or otherwise as we went through multiple permutations.

            GroupDefinition? storeExpandedGroup()
            {
                AddStep("store open group", () => expanded = Carousel.ExpandedGroup);
                return null;
            }

            void checkExpandedGroupUnchanged() => AddAssert("expanded did not change", () => Carousel.ExpandedGroup, () => Is.EqualTo(expanded));
        }

        [Test]
        public void TestRewindOverMultipleIterations()
        {
            const int local_set_count = 3;
            const int random_select_count = local_set_count * 3;

            AddBeatmaps(local_set_count, 3, true);
            WaitForDrawablePanels();

            SelectNextSet();

            for (int i = 0; i < random_select_count; i++)
                nextRandom();

            for (int i = 0; i < random_select_count; i++)
            {
                prevRandomSet();
                checkRewindCorrectSet();
            }
        }

        [Test]
        public void TestRewindOverGroupingModeChange()
        {
            const int local_set_count = 3;

            SortAndGroupBy(SortMode.Artist, GroupMode.Artist);
            AddBeatmaps(local_set_count, 3);
            WaitForDrawablePanels();

            SelectNextSet();

            for (int i = 0; i < local_set_count; i++)
                nextRandom();

            SortAndGroupBy(SortMode.Title, GroupMode.LastPlayed);
            WaitForDrawablePanels();

            for (int i = 0; i < local_set_count; i++)
            {
                prevRandomSet();
                checkRewindCorrectSet();
            }
        }

        [Test]
        public void TestRandomThenRewindSameFrame()
        {
            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();

            GroupedBeatmap? originalSelected = null;

            nextRandom();

            CheckHasSelection();
            AddStep("store selection", () => originalSelected = Carousel.CurrentGroupedBeatmap!);

            AddStep("random then rewind", () =>
            {
                Carousel.NextRandom();
                Carousel.PreviousRandom();
            });

            AddAssert("selection not changed", () => Carousel.CurrentGroupedBeatmap, () => Is.EqualTo(originalSelected));
        }

        [Test]
        public void TestRewindToDeletedBeatmap()
        {
            AddBeatmaps(10, 3, true);
            WaitForDrawablePanels();

            GroupedBeatmap? originalSelected = null;
            GroupedBeatmap? postRandomSelection = null;

            nextRandom();

            CheckHasSelection();
            AddStep("store selection", () => originalSelected = Carousel.CurrentGroupedBeatmap!);

            nextRandom();
            AddStep("store selection", () => postRandomSelection = Carousel.CurrentGroupedBeatmap!);

            AddAssert("selection changed", () => originalSelected, () => Is.Not.SameAs(postRandomSelection));

            AddStep("delete previous selection beatmaps", () => BeatmapSets.Remove(originalSelected!.Beatmap.BeatmapSet!));
            WaitForFiltering();

            AddAssert("selection not changed", () => Carousel.CurrentGroupedBeatmap, () => Is.EqualTo(postRandomSelection));

            prevRandomSet();
            AddAssert("selection not changed", () => Carousel.CurrentGroupedBeatmap, () => Is.EqualTo(postRandomSelection));
        }

        private void nextRandom() =>
            AddStep("select random next", () => Carousel.NextRandom());

        private void ensureRandomDidRepeat() =>
            AddAssert("did repeat", () => BeatmapRequestedSelections.Distinct().Count(), () => Is.LessThan(BeatmapRequestedSelections.Count));

        private void ensureRandomDidNotRepeat() =>
            AddAssert("no repeats", () => BeatmapRequestedSelections.Distinct().Count(), () => Is.EqualTo(BeatmapRequestedSelections.Count));

        private void ensureSetRandomDidRepeat() =>
            AddAssert("did repeat", () => BeatmapSetRequestedSelections.Distinct().Count(), () => Is.LessThan(BeatmapSetRequestedSelections.Count));

        private void ensureSetRandomDidNotRepeat() =>
            AddAssert("no repeats", () => BeatmapSetRequestedSelections.Distinct().Count(), () => Is.EqualTo(BeatmapSetRequestedSelections.Count));

        private void checkRewindCorrect() =>
            AddAssert("rewind matched expected beatmap", () => BeatmapRequestedSelections.Peek(), () => Is.EqualTo(Carousel.SelectedBeatmapInfo));

        private void checkRewindCorrectSet() =>
            AddAssert("rewind matched expected set", () => BeatmapSetRequestedSelections.Peek(), () => Is.EqualTo(Carousel.SelectedBeatmapSet));

        private void prevRandom() => AddStep("select last random", () =>
        {
            Carousel.PreviousRandom();
            BeatmapRequestedSelections.Pop();
            // Pop twice because the PreviousRandom call also requests selection.
            BeatmapRequestedSelections.Pop();
        });

        private void prevRandomSet() => AddStep("select last random set", () =>
        {
            Carousel.PreviousRandom();
            BeatmapSetRequestedSelections.Pop();
        });
    }
}
