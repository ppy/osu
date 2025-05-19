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

            ApplyToFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);
            WaitForFiltering();

            AddAssert("invocation count correct", () => NewItemsPresentedInvocationCount, () => Is.EqualTo(2));

            CheckDisplayedBeatmapSetsCount(1);
            CheckDisplayedBeatmapsCount(3);

            SelectNextPanel();
            Select();

            WaitForSelection(2, 0);

            for (int i = 0; i < 5; i++)
                SelectNextPanel();

            Select();
            WaitForSelection(2, 1);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
            WaitForFiltering();

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

            ApplyToFilter("filter [5..]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = null;
            });
            WaitForFiltering();
            CheckDisplayedBeatmapsCount(11);

            ApplyToFilter("filter to [0..7]", c =>
            {
                c.UserStarDifficulty.Min = null;
                c.UserStarDifficulty.Max = 7;
            });
            WaitForFiltering();
            CheckDisplayedBeatmapsCount(7);

            ApplyToFilter("filter to [5..7]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = 7;
            });

            WaitForFiltering();
            CheckDisplayedBeatmapsCount(3);

            ApplyToFilter("filter to [2..2]", c =>
            {
                c.UserStarDifficulty.Min = 2;
                c.UserStarDifficulty.Max = 2;
            });

            WaitForFiltering();
            CheckDisplayedBeatmapsCount(1);

            ApplyToFilter("filter to [0..]", c =>
            {
                c.UserStarDifficulty.Min = 0;
                c.UserStarDifficulty.Max = null;
            });
            WaitForFiltering();
            CheckDisplayedBeatmapsCount(15);
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            Guid selectedID = Guid.Empty;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectNextGroup();
            SelectNextPanel();
            Select();

            AddStep("record selection", () => selectedID = ((BeatmapInfo)Carousel.CurrentSelection!).ID);

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
                ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
            }
        }

        [Test]
        public void TestCarouselRemembersSelectionDifficultySort()
        {
            Guid selectedID = Guid.Empty;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SortBy(SortMode.Difficulty);

            SelectNextGroup();

            AddStep("record selection", () => selectedID = ((BeatmapInfo)Carousel.CurrentSelection!).ID);

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
                ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
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

                AddStep($"select diff {diff}", () => Carousel.CurrentSelection = chosenBeatmap = BeatmapSets[20].Beatmaps[diff]);
                AddUntilStep("selection changed", () => Carousel.CurrentSelection, () => Is.EqualTo(chosenBeatmap));

                SortBy(SortMode.Difficulty);
                AddAssert("selection retained", () => Carousel.CurrentSelection, () => Is.EqualTo(chosenBeatmap));

                SortBy(SortMode.Title);
                AddAssert("selection retained", () => Carousel.CurrentSelection, () => Is.EqualTo(chosenBeatmap));
            }
        }

        [Test]
        public void TestExternalRulesetChange()
        {
            ApplyToFilter("allow converted beatmaps", c => c.AllowConvertedBeatmaps = true);
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));

            WaitForFiltering();

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
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 0) == 1;
            });

            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));

            WaitForFiltering();

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 2
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 0) == 1
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 1) == 1;
            });

            ApplyToFilter("filter to catch", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(2));

            WaitForFiltering();

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 2
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 0) == 1
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 2) == 1;
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
            WaitForFiltering();

            CheckDisplayedBeatmapsCount(local_set_count * diffs_per_set);

            ApplyToFilter("filter to normal", c => c.SearchText = "Normal");
            WaitForFiltering();

            CheckDisplayedBeatmapsCount(local_set_count);

            ApplyToFilter("filter to insane", c => c.SearchText = "Insane");
            WaitForFiltering();

            CheckDisplayedBeatmapsCount(local_set_count);
        }

        [Test]
        public void TestFirstDifficultyFiltered()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            SelectNextGroup();
            WaitForSelection(0, 0);

            CheckDisplayedBeatmapsCount(6);

            ApplyToFilter("filter first away", c => c.UserStarDifficulty.Min = 3);
            WaitForFiltering();

            CheckDisplayedBeatmapsCount(4);

            SelectNextGroup();
            SelectPrevGroup();
            WaitForSelection(0, 1);
        }
    }
}
