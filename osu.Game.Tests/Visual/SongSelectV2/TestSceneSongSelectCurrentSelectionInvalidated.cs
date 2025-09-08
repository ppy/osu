// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    /// <summary>
    /// The fallback behaviour guaranteed by SongSelect is that a random selection will happen in worst case scenario.
    /// Every case we're testing here is expected to have a *custom behaviour* – engaging and overriding this random selection fallback.
    ///
    /// The scenarios we care abouts are:
    /// - Ruleset change (select another difficulty from same set for the new ruleset, if possible).
    /// - Beatmap difficulty hidden (select closest valid difficulty from same set)
    /// - Beatmap set deleted (select closest valid beatmap post-deletion)
    ///
    /// We are working with 5 sets, each with 3 difficulties (all osu! ruleset).
    /// </summary>
    public partial class TestSceneSongSelectCurrentSelectionInvalidated : SongSelectTestScene
    {
        private BeatmapInfo? selectedBeatmap => Carousel.CurrentBeatmap;
        private BeatmapSetInfo? selectedBeatmapSet => selectedBeatmap?.BeatmapSet;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            for (int i = 0; i < 5; i++)
                ImportBeatmapForRuleset(0);

            LoadSongSelect();
        }

        [Test]
        public void TestRulesetChange()
        {
            AddStep("disable converts", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            ImportBeatmapForRuleset(0, 1);
            ImportBeatmapForRuleset(0, 1);
            ImportBeatmapForRuleset(0, 2);
            waitForFiltering(5);

            ChangeRuleset(1);
            waitForFiltering(6);

            BeatmapInfo? initiallySelected = null;
            AddAssert("selected is taiko", () => (initiallySelected = selectedBeatmap)?.Ruleset.OnlineID, () => Is.EqualTo(1));

            ChangeRuleset(0);
            waitForFiltering(7);
            AddAssert("selected is osu", () => selectedBeatmap?.Ruleset.OnlineID, () => Is.EqualTo(0));
            AddAssert("selected is same set as original", () => selectedBeatmap?.BeatmapSet, () => Is.EqualTo(initiallySelected!.BeatmapSet));

            ChangeRuleset(1);
            waitForFiltering(8);
            AddAssert("selected is taiko", () => selectedBeatmap?.Ruleset.OnlineID, () => Is.EqualTo(1));
            AddAssert("selected is same set as original", () => selectedBeatmap?.BeatmapSet, () => Is.EqualTo(initiallySelected!.BeatmapSet));

            ChangeRuleset(2);
            waitForFiltering(9);
            AddAssert("selected is catch", () => selectedBeatmap?.Ruleset.OnlineID, () => Is.EqualTo(2));
            AddAssert("selected is different set", () => selectedBeatmap?.BeatmapSet, () => Is.Not.EqualTo(initiallySelected!.BeatmapSet));
        }

        /// <summary>
        /// Make sure that deleting all sets doesn't hit some weird edge case / crash.
        /// </summary>
        [TestCase(SortMode.Title)]
        [TestCase(SortMode.Artist)]
        [TestCase(SortMode.Difficulty)]
        public void TestDeleteAllSets(SortMode sortMode)
        {
            int filterCount = sortMode != SortMode.Title ? 2 : 1;

            SortBy(sortMode);
            waitForFiltering(filterCount);

            BeatmapSetInfo deletedSet = null!;

            for (int i = 0; i < 4; i++)
            {
                AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
                waitForFiltering(filterCount + 1 + i);
                selectionChangedFrom(() => deletedSet);
            }

            // The carousel still holds an invalid selection after the final deletion. Probably fine?
            AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
            AddUntilStep("wait for no global selection", () => Beatmap.IsDefault, () => Is.True);
        }

        [Test]
        public void DifficultiesGrouped_DeleteSet_SelectsAdjacent()
        {
            SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);
            waitForFiltering(2);

            makePanelSelected<PanelGroupStarDifficulty>(2);
            makePanelSelected<PanelBeatmapStandalone>(3);

            // Deleting second-last, should select last
            BeatmapSetInfo deletedSet = null!;
            AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
            waitForFiltering(3);

            selectionChangedFrom(() => deletedSet);
            assertPanelSelected<PanelBeatmapStandalone>(3);

            // Deleting last, should select previous
            AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
            waitForFiltering(4);

            selectionChangedFrom(() => deletedSet);
            assertPanelSelected<PanelBeatmapStandalone>(2);
        }

        [TestCase(SortMode.Title)]
        [TestCase(SortMode.Artist)]
        public void SetsGrouped_DeleteSet_SelectsAdjacent(SortMode sortMode)
        {
            int filterCount = sortMode != SortMode.Title ? 2 : 1;

            SortBy(sortMode);
            waitForFiltering(filterCount);

            makePanelSelected<PanelBeatmapSet>(3);

            // Deleting second-last, should select last
            BeatmapSetInfo deletedSet = null!;
            AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
            waitForFiltering(filterCount + 1);

            selectionChangedFrom(() => deletedSet);
            assertPanelSelected<PanelBeatmapSet>(3);
            assertPanelSelected<PanelBeatmap>(0);

            // Deleting last, should select previous
            AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
            waitForFiltering(filterCount + 2);

            selectionChangedFrom(() => deletedSet);
            assertPanelSelected<PanelBeatmapSet>(2);
            assertPanelSelected<PanelBeatmap>(0);
        }

        // Same scenario as the test case above, but where selected difficulty before deletion is not first index in the expanded set.
        // Basically ensures that the reselection is running `RequestRecommendedSelection` and not just relying on indices.
        [TestCase(SortMode.Title)]
        [TestCase(SortMode.Artist)]
        public void SetsGrouped_DeleteSet_SelectsNextSetRecommendedDifficulty(SortMode sortMode)
        {
            int filterCount = sortMode != SortMode.Title ? 2 : 1;

            SortBy(sortMode);
            waitForFiltering(filterCount);

            makePanelSelected<PanelBeatmapSet>(2);
            makePanelSelected<PanelBeatmap>(2);

            AddUntilStep("wait for beatmap to be selected", () => selectedBeatmapSet != null);

            BeatmapSetInfo deletedSet = null!;
            AddStep("delete selected", () => Beatmaps.Delete(deletedSet = selectedBeatmapSet!));
            waitForFiltering(++filterCount);

            selectionChangedFrom(() => deletedSet);
            assertPanelSelected<PanelBeatmapSet>(2);
            assertPanelSelected<PanelBeatmap>(0);
        }

        [Test]
        public void TestHideBeatmap()
        {
            makePanelSelected<PanelBeatmapSet>(2);
            makePanelSelected<PanelBeatmap>(1);

            BeatmapInfo hiddenBeatmap = null!;

            AddStep("hide selected", () => Beatmaps.Hide(hiddenBeatmap = selectedBeatmap!));
            waitForFiltering(2);

            AddAssert("selected beatmap below", () => selectedBeatmap!.BeatmapSet, () => Is.EqualTo(hiddenBeatmap.BeatmapSet));

            AddStep("hide selected", () => Beatmaps.Hide(hiddenBeatmap = selectedBeatmap!));
            waitForFiltering(3);

            AddAssert("selected beatmap below", () => selectedBeatmap!.BeatmapSet, () => Is.EqualTo(hiddenBeatmap.BeatmapSet));
            assertPanelSelected<PanelBeatmap>(0);
        }

        [Test]
        [Explicit]
        public void TestDebounceNotBypassedOnUpdate()
        {
            BeatmapInfo? selectedBefore = null;
            BeatmapInfo? selectedBeatmapDuringDebounce = null;

            // we're testing the song select side debounce, so let's make filtering immediate
            AddStep("set filter debounce delay to zero", () => Carousel.DebounceDelay = 0);

            WaitForFiltering();

            AddUntilStep("wait for global beatmap selection", () => !Beatmap.IsDefault);

            AddStep("store selection", () => selectedBefore = Beatmap.Value.BeatmapInfo);

            AddStep("traverse to next panel and update simultaneously", () =>
            {
                InputManager.Key(Key.Right);

                Beatmaps.Delete(Beatmaps.GetAllUsableBeatmapSets().Last());

                // check selection during debounce
                Scheduler.AddDelayed(() => selectedBeatmapDuringDebounce = Beatmap.Value.BeatmapInfo, Screens.SelectV2.SongSelect.SELECTION_DEBOUNCE / 2f);
            });

            WaitForFiltering();

            AddUntilStep("wait for pre-debounce selection", () => selectedBeatmapDuringDebounce, () => Is.Not.Null);

            AddAssert("selection during debounce didn't change", () => selectedBeatmapDuringDebounce, () => Is.EqualTo(selectedBefore));

            // Due to nunit runs having limited precision this tends to fail when headless, even though you'd expect the previous step to fail.
            // Interactively, things fail as expected.
            AddUntilStep("selection has changed after debounce", () => selectedBeatmapDuringDebounce, () => Is.Not.EqualTo(Beatmap.Value.BeatmapInfo));
        }

        private void waitForFiltering(int filterCount = 1)
        {
            AddUntilStep("wait for filter count", () => Carousel.FilterCount, () => Is.EqualTo(filterCount));
            AddUntilStep("filtering finished", () => Carousel.IsFiltering, () => Is.False);
        }

        private void makePanelSelected<T>(int index)
            where T : Panel
        {
            AddStep($"click panel at index {index} if not selected", () =>
            {
                var panel = allPanels<T>().ElementAt(index).ChildrenOfType<Panel>().Single();

                // May have already been selected randomly. Don't click a second time or gameplay will start.
                if (!panel.Selected.Value)
                    panel.TriggerClick();
            });

            assertPanelSelected<T>(index);
        }

        private void selectionChangedFrom(Func<BeatmapSetInfo> deletedSet) =>
            AddUntilStep("selection changed", () => selectedBeatmapSet, () => Is.Not.EqualTo(deletedSet()));

        private void assertPanelSelected<T>(int index)
            where T : Panel
            => AddUntilStep($"selected panel at index {index}", getActivePanelIndex<T>, () => Is.EqualTo(index));

        private int getActivePanelIndex<T>()
            where T : Panel
            => allPanels<T>().ToList().FindIndex(p =>
            {
                switch (p)
                {
                    case PanelBeatmapStandalone pb:
                        return pb.Selected.Value;

                    case PanelBeatmap pb:
                        return pb.Selected.Value;

                    case Panel pbs:
                        return pbs.Expanded.Value;

                    default:
                        throw new InvalidOperationException();
                }
            });

        private IEnumerable<T> allPanels<T>()
            where T : Panel
            => Carousel.ChildrenOfType<T>().Where(p => p.Item != null).OrderBy(p => p.Y);
    }
}
