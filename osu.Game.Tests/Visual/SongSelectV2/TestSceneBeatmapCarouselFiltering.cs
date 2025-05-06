// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
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

            SelectNextPanel();
            Select();

            ApplyToFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);
            WaitForFiltering();

            CheckVisibleBeatmapsCount(3);
            CheckVisibleBeatmapSetsCount(1);
            WaitForSelection(2, 0);

            for (int i = 0; i < 5; i++)
                SelectNextPanel();

            Select();
            WaitForSelection(2, 1);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
            WaitForFiltering();

            CheckVisibleBeatmapsCount(30);
            CheckVisibleBeatmapSetsCount(10);
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
            CheckVisibleBeatmapsCount(11);

            ApplyToFilter("filter to [0..7]", c =>
            {
                c.UserStarDifficulty.Min = null;
                c.UserStarDifficulty.Max = 7;
            });
            WaitForFiltering();
            CheckVisibleBeatmapsCount(7);

            ApplyToFilter("filter to [5..7]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = 7;
            });

            WaitForFiltering();
            CheckVisibleBeatmapsCount(3);

            ApplyToFilter("filter to [2..2]", c =>
            {
                c.UserStarDifficulty.Min = 2;
                c.UserStarDifficulty.Max = 2;
            });

            WaitForFiltering();
            CheckVisibleBeatmapsCount(1);

            ApplyToFilter("filter to [0..]", c =>
            {
                c.UserStarDifficulty.Min = 0;
                c.UserStarDifficulty.Max = null;
            });
            WaitForFiltering();
            CheckVisibleBeatmapsCount(15);
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

            CheckVisibleBeatmapSetsCount(3);
            CheckVisibleBeatmapsCount(local_set_count * diffs_per_set);

            ApplyToFilter("filter to normal", c => c.SearchText = "Normal");

            CheckVisibleBeatmapSetsCount(local_set_count);
            CheckVisibleBeatmapsCount(local_set_count);

            ApplyToFilter("filter to insane", c => c.SearchText = "Insane");

            CheckVisibleBeatmapSetsCount(local_set_count);
            CheckVisibleBeatmapsCount(local_set_count);
        }

        [Test]
        public void TestSelectionEnteringFromEmptyRuleset()
        {
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));
            AddStep("create beatmaps for taiko only", () =>
            {
                var rulesetBeatmapSet = TestResources.CreateTestBeatmapSetInfo(1);
                var taikoRuleset = rulesets.AvailableRulesets.ElementAt(1);
                rulesetBeatmapSet.Beatmaps.ForEach(b => b.Ruleset = taikoRuleset);

                BeatmapSets.Add(rulesetBeatmapSet);
            });

            AddAssert("selection is null", () => Carousel.CurrentSelection == null);

            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("selection is not null", () => Carousel.CurrentSelection != null);
        }

        [Test]
        public void TestSelectingFilteredRuleset()
        {
            AddStep("add mixed ruleset beatmapset", () =>
            {
                var testMixed = TestResources.CreateTestBeatmapSetInfo(3);

                for (int i = 0; i <= 2; i++)
                    testMixed.Beatmaps[i].Ruleset = rulesets.AvailableRulesets.ElementAt(i);

                BeatmapSets.Add(testMixed);
            });

            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("taiko difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 1);
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));
            AddUntilStep("osu difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 0);

            RemoveAllBeatmaps();

            AddStep("add single ruleset beatmapset", () =>
            {
                var testSingle = TestResources.CreateTestBeatmapSetInfo(3);
                testSingle.Beatmaps.ForEach(b => b.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
                BeatmapSets.Add(testSingle);
            });
            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("taiko difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 1);
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));
            AddUntilStep("no difficulty selected", () => Carousel.CurrentSelection == null);
            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("taiko difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 1);
        }

        [Test]
        public void TestSelectionChangedOnRemoval()
        {
            AddBeatmaps(2, 3);
            WaitForDrawablePanels();

            AddStep("select second set", () => Carousel.CurrentSelection = BeatmapSets[1].Beatmaps[0]);
            WaitForSelection(1, 0);

            AddStep("remove second beatmap", () => BeatmapSets.RemoveAt(1));

            WaitForSelection(0, 0);
            AddStep("remove remaining beatmap", () => BeatmapSets.RemoveAt(0));

            CheckNoSelection();

            AddBeatmaps(1, 3);
            WaitForSelection(0, 0);

            AddBeatmaps(5, 3);
            WaitForSelection(0, 0);
        }
    }
}
