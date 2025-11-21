// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselFiltering : BeatmapCarouselTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
        }

        [Test]
        public void TestBasicFiltering()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            AddAssert("invocation count correct", () => NewItemsPresentedInvocationCount, () => Is.EqualTo(1));

            ApplyToFilterAndWaitForFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);

            AddAssert("invocation count correct", () => NewItemsPresentedInvocationCount, () => Is.EqualTo(2));

            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(3);

            WaitForSetSelection(2, 0);

            for (int i = 0; i < 5; i++)
                SelectNextPanel();

            Select();
            WaitForSetSelection(2, 1);

            ApplyToFilterAndWaitForFilter("remove filter", c => c.SearchText = string.Empty);

            AddAssert("invocation count correct", () => NewItemsPresentedInvocationCount, () => Is.EqualTo(3));

            CheckDisplayedBeatmapSetsCount(10);
            CheckDisplayedBeatmapsCount(30);
        }

        [Test]
        public void TestFilteringByUserStarDifficulty()
        {
            AddStep("add mixed difficulty set", () =>
            {
                var set = TestResources.CreateTestBeatmapSetInfo(1);
                set.Beatmaps.Clear();

                for (int i = 1; i <= 15; i++)
                {
                    set.Beatmaps.Add(new BeatmapInfo(new OsuRuleset().RulesetInfo, new BeatmapDifficulty(), new BeatmapMetadata())
                    {
                        BeatmapSet = set,
                        DifficultyName = $"Stars: {i}",
                        StarRating = i,
                    });
                }

                BeatmapSets.Add(set);
            });

            WaitForDrawablePanels();

            ApplyToFilterAndWaitForFilter("filter [5..]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = null;
            });
            CheckDisplayedBeatmapsCount(11);

            ApplyToFilterAndWaitForFilter("filter to [0..7]", c =>
            {
                c.UserStarDifficulty.Min = null;
                c.UserStarDifficulty.Max = 7;
            });
            CheckDisplayedBeatmapsCount(7);

            ApplyToFilterAndWaitForFilter("filter to [5..7]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = 7;
            });
            CheckDisplayedBeatmapsCount(3);

            ApplyToFilterAndWaitForFilter("filter to [2..2]", c =>
            {
                c.UserStarDifficulty.Min = 2;
                c.UserStarDifficulty.Max = 2;
            });
            CheckDisplayedBeatmapsCount(1);

            ApplyToFilterAndWaitForFilter("filter to [0..]", c =>
            {
                c.UserStarDifficulty.Min = 0;
                c.UserStarDifficulty.Max = null;
            });
            CheckDisplayedBeatmapsCount(15);
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            Guid selectedID = Guid.Empty;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            SelectNextPanel();
            Select();

            AddStep("record selection", () => selectedID = Carousel.CurrentBeatmap!.ID);

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilterAndWaitForFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddAssert("selection not changed", () => Carousel.CurrentBeatmap!.ID == selectedID);
                ApplyToFilterAndWaitForFilter("remove filter", c => c.SearchText = string.Empty);
                AddAssert("selection not changed", () => Carousel.CurrentBeatmap!.ID == selectedID);
            }
        }

        [Test]
        public void TestCarouselChangesSelectionOnSingleMatch_FromSelection()
        {
            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectPrevSet();
            WaitForSetSelection(49, 0);

            ApplyToFilterAndWaitForFilter("filter all but one", c => c.SearchText = BeatmapSets.First().Metadata.Title);
            WaitForSetSelection(0, 0);
        }

        [Test]
        public void TestCarouselChangesSelectionOnSingleMatch_FromNoSelection()
        {
            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            CheckNoSelection();
            ApplyToFilterAndWaitForFilter("filter all but one", c => c.SearchText = BeatmapSets.First().Metadata.Title);
            WaitForSetSelection(0, 0);
        }

        [Test]
        public void TestCarouselRemembersSelectionDifficultySort()
        {
            Guid selectedID = Guid.Empty;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SortBy(SortMode.Difficulty);

            SelectNextSet();

            AddStep("record selection", () => selectedID = Carousel.CurrentBeatmap!.ID);

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilterAndWaitForFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddAssert("selection not changed", () => Carousel.CurrentBeatmap!.ID == selectedID);
                ApplyToFilterAndWaitForFilter("remove filter", c => c.SearchText = string.Empty);
                AddAssert("selection not changed", () => Carousel.CurrentBeatmap!.ID == selectedID);
            }
        }

        [Test]
        public void TestCarouselRetainsSelectionFromDifficultySort()
        {
            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            BeatmapInfo chosenBeatmap = null!;

            for (int i = 0; i < 3; i++)
            {
                int diff = i;

                AddStep($"select diff {diff}", () => Carousel.CurrentBeatmap = chosenBeatmap = BeatmapSets[20].Beatmaps[diff]);
                AddUntilStep("selection changed", () => Carousel.CurrentBeatmap, () => Is.EqualTo(chosenBeatmap));

                SortBy(SortMode.Difficulty);
                AddAssert("selection retained", () => Carousel.CurrentBeatmap, () => Is.EqualTo(chosenBeatmap));

                SortBy(SortMode.Title);
                AddAssert("selection retained", () => Carousel.CurrentBeatmap, () => Is.EqualTo(chosenBeatmap));
            }
        }

        [Test]
        public void TestExternalRulesetChange()
        {
            ApplyToFilterAndWaitForFilter("allow converted beatmaps, filter to osu", c =>
            {
                c.AllowConvertedBeatmaps = true;
                c.Ruleset = rulesets.AvailableRulesets.ElementAt(0);
            });

            AddStep("add mixed ruleset beatmapset", () =>
            {
                var testMixed = TestResources.CreateTestBeatmapSetInfo(3);

                for (int i = 0; i <= 2; i++)
                    testMixed.Beatmaps[i].Ruleset = rulesets.AvailableRulesets.ElementAt(i);

                BeatmapSets.Add(testMixed);
            });
            WaitForDrawablePanels();

            SelectNextPanel();
            Select();

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 1
                       && visibleBeatmapPanels.Count(p => ((GroupedBeatmap)p.Item!.Model).Beatmap.Ruleset.OnlineID == 0) == 1;
            });

            ApplyToFilterAndWaitForFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 2
                       && visibleBeatmapPanels.Count(p => ((GroupedBeatmap)p.Item!.Model).Beatmap.Ruleset.OnlineID == 0) == 1
                       && visibleBeatmapPanels.Count(p => ((GroupedBeatmap)p.Item!.Model).Beatmap.Ruleset.OnlineID == 1) == 1;
            });

            ApplyToFilterAndWaitForFilter("filter to catch", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(2));

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 2
                       && visibleBeatmapPanels.Count(p => ((GroupedBeatmap)p.Item!.Model).Beatmap.Ruleset.OnlineID == 0) == 1
                       && visibleBeatmapPanels.Count(p => ((GroupedBeatmap)p.Item!.Model).Beatmap.Ruleset.OnlineID == 2) == 1;
            });
        }

        [Test]
        public void TestSortingWithDifficultyFiltered()
        {
            const int diffs_per_set = 3;
            const int local_set_count = 2;

            AddStep("populate beatmap sets", () =>
            {
                for (int i = 0; i < local_set_count; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diffs_per_set);
                    set.Beatmaps[0].StarRating = 3 - i;
                    set.Beatmaps[0].DifficultyName += $" ({3 - i}*)";
                    set.Beatmaps[1].StarRating = 6 + i;
                    set.Beatmaps[1].DifficultyName += $" ({6 + i}*)";
                    BeatmapSets.Add(set);
                }
            });

            SortBy(SortMode.Difficulty);

            CheckDisplayedBeatmapsCount(local_set_count * diffs_per_set);

            ApplyToFilterAndWaitForFilter("filter to normal", c => c.SearchText = "Normal");

            CheckDisplayedBeatmapsCount(local_set_count);

            ApplyToFilterAndWaitForFilter("filter to insane", c => c.SearchText = "Insane");

            CheckDisplayedBeatmapsCount(local_set_count);
        }

        [Test]
        public void TestFirstDifficultyFiltered()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(0, 0);

            CheckDisplayedBeatmapsCount(6);

            ApplyToFilterAndWaitForFilter("filter first away", c => c.UserStarDifficulty.Min = 3);

            CheckDisplayedBeatmapsCount(4);

            SelectNextSet();
            WaitForSetSelection(1, 1);
            SelectPrevSet();
            WaitForSetSelection(0, 1);
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectNextGroup()
        {
            AddBeatmaps(5, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            SelectNextSet();
            SelectNextSet();
            WaitForSetSelection(2, 0);

            ApplyToFilterAndWaitForFilter("filter first away", c => c.UserStarDifficulty.Min = 3);

            SelectNextSet();
            WaitForSetSelection(0, 1);
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectPrevGroup()
        {
            AddBeatmaps(5, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            SelectNextSet();
            SelectNextSet();
            WaitForSetSelection(2, 0);

            ApplyToFilterAndWaitForFilter("filter first away", c => c.UserStarDifficulty.Min = 3);

            SelectPrevSet();
            WaitForSetSelection(4, 1);
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectPrevGroup_OnlyOnePanelAvailable()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SelectPrevSet();
            WaitForSetSelection(1, 0);

            ApplyToFilterAndWaitForFilter("filter last set away", c => c.SearchText = BeatmapSets.First().Metadata.Title);

            SelectPrevSet();
            WaitForSetSelection(0, 0);
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectNextGroup_OnlyOnePanelAvailable()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(0, 0);

            ApplyToFilterAndWaitForFilter("filter first set away", c => c.SearchText = BeatmapSets.Last().Metadata.Title);

            SelectNextSet();
            WaitForSetSelection(1, 0);
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectNextPanel()
        {
            AddBeatmaps(5, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            SelectNextSet();
            SelectNextSet();
            WaitForSetSelection(2, 0);

            ApplyToFilterAndWaitForFilter("filter first away", c => c.UserStarDifficulty.Min = 3);

            SelectNextPanel();
            AddAssert("keyboard selected is first set",
                () => (GetKeyboardSelectedPanel()?.Item?.Model as GroupedBeatmapSet)?.BeatmapSet,
                () => Is.EqualTo(BeatmapSets.First()));
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectPrevPanel()
        {
            AddBeatmaps(5, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            SelectNextSet();
            SelectNextSet();
            WaitForSetSelection(2, 0);

            ApplyToFilterAndWaitForFilter("filter first away", c => c.UserStarDifficulty.Min = 3);

            SelectPrevPanel();
            AddAssert("keyboard selected is last set",
                () => (GetKeyboardSelectedPanel()?.Item?.Model as GroupedBeatmapSet)?.BeatmapSet,
                () => Is.EqualTo(BeatmapSets.Last()));
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectPrevPanel_OnlyOnePanelAvailable()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SelectPrevSet();
            WaitForSetSelection(1, 0);

            ApplyToFilterAndWaitForFilter("filter last set away", c => c.SearchText = BeatmapSets.First().Metadata.Title);

            SelectPrevPanel();
            AddAssert("keyboard selected is first set",
                () => (GetKeyboardSelectedPanel()?.Item?.Model as GroupedBeatmapSet)?.BeatmapSet,
                () => Is.EqualTo(BeatmapSets.First()));
        }

        [Test]
        public void TestNavigateFromFilteredItem_SelectNextPanel_OnlyOnePanelAvailable()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SelectNextSet();
            WaitForSetSelection(0, 0);

            ApplyToFilterAndWaitForFilter("filter first set away", c => c.SearchText = BeatmapSets.Last().Metadata.Title);

            // Single result is automatically selected for us, so we iterate once backwards to the set header.
            SelectPrevPanel();
            AddAssert("keyboard selected is second set",
                () => (GetKeyboardSelectedPanel()?.Item?.Model as GroupedBeatmapSet)?.BeatmapSet,
                () => Is.EqualTo(BeatmapSets.Last()));
        }
    }
}
