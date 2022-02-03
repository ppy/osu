// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestSceneBeatmapCarousel : OsuManualInputManagerTestScene
    {
        private TestBeatmapCarousel carousel;
        private RulesetStore rulesets;

        private readonly Stack<BeatmapSetInfo> selectedSets = new Stack<BeatmapSetInfo>();
        private readonly HashSet<Guid> eagerSelectedIDs = new HashSet<Guid>();

        private BeatmapInfo currentSelection => carousel.SelectedBeatmapInfo;

        private const int set_count = 5;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        [Test]
        public void TestScrollPositionMaintainedOnAdd()
        {
            loadBeatmaps(count: 1, randomDifficulties: false);

            for (int i = 0; i < 10; i++)
            {
                AddRepeatStep("Add some sets", () => carousel.UpdateBeatmapSet(TestResources.CreateTestBeatmapSetInfo()), 4);

                checkSelectionIsCentered();
            }
        }

        [Test]
        public void TestScrollPositionMaintainedOnDelete()
        {
            loadBeatmaps(count: 50, randomDifficulties: false);

            for (int i = 0; i < 10; i++)
            {
                AddRepeatStep("Remove some sets", () =>
                    carousel.RemoveBeatmapSet(carousel.Items.Select(item => item.Item)
                                                      .OfType<CarouselBeatmapSet>()
                                                      .OrderBy(item => item.GetHashCode())
                                                      .First(item => item.State.Value != CarouselItemState.Selected && item.Visible).BeatmapSet), 4);

                checkSelectionIsCentered();
            }
        }

        [Test]
        public void TestManyPanels()
        {
            loadBeatmaps(count: 5000, randomDifficulties: true);
        }

        [Test]
        public void TestKeyRepeat()
        {
            loadBeatmaps();
            advanceSelection(false);

            AddStep("press down arrow", () => InputManager.PressKey(Key.Down));

            BeatmapInfo selection = null;

            checkSelectionIterating(true);

            AddStep("press up arrow", () => InputManager.PressKey(Key.Up));

            checkSelectionIterating(true);

            AddStep("release down arrow", () => InputManager.ReleaseKey(Key.Down));

            checkSelectionIterating(true);

            AddStep("release up arrow", () => InputManager.ReleaseKey(Key.Up));

            checkSelectionIterating(false);

            void checkSelectionIterating(bool isIterating)
            {
                for (int i = 0; i < 3; i++)
                {
                    AddStep("store selection", () => selection = carousel.SelectedBeatmapInfo);
                    if (isIterating)
                        AddUntilStep("selection changed", () => !carousel.SelectedBeatmapInfo?.Equals(selection) == true);
                    else
                        AddUntilStep("selection not changed", () => carousel.SelectedBeatmapInfo.Equals(selection));
                }
            }
        }

        [Test]
        public void TestRecommendedSelection()
        {
            loadBeatmaps(carouselAdjust: carousel => carousel.GetRecommendedBeatmap = beatmaps => beatmaps.LastOrDefault());

            AddStep("select last", () => carousel.SelectBeatmap(carousel.BeatmapSets.Last().Beatmaps.Last()));

            // check recommended was selected
            advanceSelection(direction: 1, diff: false);
            waitForSelection(1, 3);

            // change away from recommended
            advanceSelection(direction: -1, diff: true);
            waitForSelection(1, 2);

            // next set, check recommended
            advanceSelection(direction: 1, diff: false);
            waitForSelection(2, 3);

            // next set, check recommended
            advanceSelection(direction: 1, diff: false);
            waitForSelection(3, 3);

            // go back to first set and ensure user selection was retained
            advanceSelection(direction: -1, diff: false);
            advanceSelection(direction: -1, diff: false);
            waitForSelection(1, 2);
        }

        /// <summary>
        /// Test keyboard traversal
        /// </summary>
        [Test]
        public void TestTraversal()
        {
            loadBeatmaps();

            AddStep("select first", () => carousel.SelectBeatmap(carousel.BeatmapSets.First().Beatmaps.First()));
            waitForSelection(1, 1);

            advanceSelection(direction: 1, diff: true);
            waitForSelection(1, 2);

            advanceSelection(direction: -1, diff: false);
            waitForSelection(set_count, 1);

            advanceSelection(direction: -1, diff: true);
            waitForSelection(set_count - 1, 3);

            advanceSelection(diff: false);
            advanceSelection(diff: false);
            waitForSelection(1, 2);

            advanceSelection(direction: -1, diff: true);
            advanceSelection(direction: -1, diff: true);
            waitForSelection(set_count, 3);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestTraversalBeyondVisible(bool forwards)
        {
            var sets = new List<BeatmapSetInfo>();

            const int total_set_count = 200;

            for (int i = 0; i < total_set_count; i++)
                sets.Add(TestResources.CreateTestBeatmapSetInfo());

            loadBeatmaps(sets);

            for (int i = 1; i < total_set_count; i += i)
                selectNextAndAssert(i);

            void selectNextAndAssert(int amount)
            {
                setSelected(forwards ? 1 : total_set_count, 1);

                AddStep($"{(forwards ? "Next" : "Previous")} beatmap {amount} times", () =>
                {
                    for (int i = 0; i < amount; i++)
                    {
                        carousel.SelectNext(forwards ? 1 : -1);
                    }
                });

                waitForSelection(forwards ? amount + 1 : total_set_count - amount);
            }
        }

        [Test]
        public void TestTraversalBeyondVisibleDifficulties()
        {
            var sets = new List<BeatmapSetInfo>();

            const int total_set_count = 20;

            for (int i = 0; i < total_set_count; i++)
                sets.Add(TestResources.CreateTestBeatmapSetInfo(3));

            loadBeatmaps(sets);

            // Selects next set once, difficulty index doesn't change
            selectNextAndAssert(3, true, 2, 1);

            // Selects next set 16 times (50 \ 3 == 16), difficulty index changes twice (50 % 3 == 2)
            selectNextAndAssert(50, true, 17, 3);

            // Travels around the carousel thrice (200 \ 60 == 3)
            // continues to select 20 times (200 \ 60 == 20)
            // selects next set 6 times (20 \ 3 == 6)
            // difficulty index changes twice (20 % 3 == 2)
            selectNextAndAssert(200, true, 7, 3);

            // All same but in reverse
            selectNextAndAssert(3, false, 19, 3);
            selectNextAndAssert(50, false, 4, 1);
            selectNextAndAssert(200, false, 14, 1);

            void selectNextAndAssert(int amount, bool forwards, int expectedSet, int expectedDiff)
            {
                // Select very first or very last difficulty
                setSelected(forwards ? 1 : 20, forwards ? 1 : 3);

                AddStep($"{(forwards ? "Next" : "Previous")} difficulty {amount} times", () =>
                {
                    for (int i = 0; i < amount; i++)
                        carousel.SelectNext(forwards ? 1 : -1, false);
                });

                waitForSelection(expectedSet, expectedDiff);
            }
        }

        /// <summary>
        /// Test filtering
        /// </summary>
        [Test]
        public void TestFiltering()
        {
            loadBeatmaps();

            // basic filtering
            setSelected(1, 1);

            AddStep("Filter", () => carousel.Filter(new FilterCriteria { SearchText = carousel.BeatmapSets.ElementAt(2).Metadata.Title }, false));
            checkVisibleItemCount(diff: false, count: 1);
            checkVisibleItemCount(diff: true, count: 3);
            waitForSelection(3, 1);

            advanceSelection(diff: true, count: 4);
            waitForSelection(3, 2);

            AddStep("Un-filter (debounce)", () => carousel.Filter(new FilterCriteria()));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(diff: false, count: set_count);
            checkVisibleItemCount(diff: true, count: 3);

            // test filtering some difficulties (and keeping current beatmap set selected).

            setSelected(1, 2);
            AddStep("Filter some difficulties", () => carousel.Filter(new FilterCriteria { SearchText = "Normal" }, false));
            waitForSelection(1, 1);

            AddStep("Un-filter", () => carousel.Filter(new FilterCriteria(), false));
            waitForSelection(1, 1);

            AddStep("Filter all", () => carousel.Filter(new FilterCriteria { SearchText = "Dingo" }, false));

            checkVisibleItemCount(false, 0);
            checkVisibleItemCount(true, 0);
            AddAssert("Selection is null", () => currentSelection == null);

            advanceSelection(true);
            AddAssert("Selection is null", () => currentSelection == null);

            advanceSelection(false);
            AddAssert("Selection is null", () => currentSelection == null);

            AddStep("Un-filter", () => carousel.Filter(new FilterCriteria(), false));

            AddAssert("Selection is non-null", () => currentSelection != null);

            setSelected(1, 3);
        }

        [Test]
        public void TestFilterRange()
        {
            string searchText = null;

            loadBeatmaps();

            // buffer the selection
            setSelected(3, 2);

            AddStep("get search text", () => searchText = carousel.SelectedBeatmapSet.Metadata.Title);

            setSelected(1, 3);

            AddStep("Apply a range filter", () => carousel.Filter(new FilterCriteria
            {
                SearchText = searchText,
                StarDifficulty = new FilterCriteria.OptionalRange<double>
                {
                    Min = 2,
                    Max = 5.5,
                    IsLowerInclusive = true
                }
            }, false));

            // should reselect the buffered selection.
            waitForSelection(3, 2);
        }

        /// <summary>
        /// Test random non-repeating algorithm
        /// </summary>
        [Test]
        public void TestRandom()
        {
            loadBeatmaps();

            setSelected(1, 1);

            nextRandom();
            ensureRandomDidntRepeat();
            nextRandom();
            ensureRandomDidntRepeat();
            nextRandom();
            ensureRandomDidntRepeat();

            prevRandom();
            ensureRandomFetchSuccess();
            prevRandom();
            ensureRandomFetchSuccess();

            nextRandom();
            ensureRandomDidntRepeat();
            nextRandom();
            ensureRandomDidntRepeat();

            nextRandom();
            AddAssert("ensure repeat", () => selectedSets.Contains(carousel.SelectedBeatmapSet));

            AddStep("Add set with 100 difficulties", () => carousel.UpdateBeatmapSet(TestResources.CreateTestBeatmapSetInfo(100, rulesets.AvailableRulesets.ToArray())));
            AddStep("Filter Extra", () => carousel.Filter(new FilterCriteria { SearchText = "Extra 10" }, false));
            checkInvisibleDifficultiesUnselectable();
            checkInvisibleDifficultiesUnselectable();
            checkInvisibleDifficultiesUnselectable();
            checkInvisibleDifficultiesUnselectable();
            checkInvisibleDifficultiesUnselectable();
            AddStep("Un-filter", () => carousel.Filter(new FilterCriteria(), false));
        }

        /// <summary>
        /// Test adding and removing beatmap sets
        /// </summary>
        [Test]
        public void TestAddRemove()
        {
            loadBeatmaps();

            var firstAdded = TestResources.CreateTestBeatmapSetInfo();
            var secondAdded = TestResources.CreateTestBeatmapSetInfo();

            AddStep("Add new set", () => carousel.UpdateBeatmapSet(firstAdded));
            AddStep("Add new set", () => carousel.UpdateBeatmapSet(secondAdded));

            checkVisibleItemCount(false, set_count + 2);

            AddStep("Remove set", () => carousel.RemoveBeatmapSet(firstAdded));

            checkVisibleItemCount(false, set_count + 1);

            setSelected(set_count + 1, 1);

            AddStep("Remove set", () => carousel.RemoveBeatmapSet(secondAdded));

            checkVisibleItemCount(false, set_count);

            waitForSelection(set_count);
        }

        [Test]
        public void TestSelectionEnteringFromEmptyRuleset()
        {
            var sets = new List<BeatmapSetInfo>();

            AddStep("Create beatmaps for taiko only", () =>
            {
                sets.Clear();

                var rulesetBeatmapSet = TestResources.CreateTestBeatmapSetInfo(1);
                var taikoRuleset = rulesets.AvailableRulesets.ElementAt(1);
                rulesetBeatmapSet.Beatmaps.ForEach(b => b.Ruleset = taikoRuleset);

                sets.Add(rulesetBeatmapSet);
            });

            loadBeatmaps(sets, () => new FilterCriteria { Ruleset = rulesets.AvailableRulesets.ElementAt(0) });

            AddStep("Set non-empty mode filter", () =>
                carousel.Filter(new FilterCriteria { Ruleset = rulesets.AvailableRulesets.ElementAt(1) }, false));

            AddAssert("Something is selected", () => carousel.SelectedBeatmapInfo != null);
        }

        /// <summary>
        /// Test sorting
        /// </summary>
        [Test]
        public void TestSorting()
        {
            var sets = new List<BeatmapSetInfo>();

            const string zzz_string = "zzzzz";

            for (int i = 0; i < 20; i++)
            {
                var set = TestResources.CreateTestBeatmapSetInfo();

                if (i == 4)
                    set.Beatmaps.ForEach(b => b.Metadata.Artist = zzz_string);

                if (i == 16)
                    set.Beatmaps.ForEach(b => b.Metadata.Author.Username = zzz_string);

                sets.Add(set);
            }

            loadBeatmaps(sets);

            AddStep("Sort by author", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Author }, false));
            AddAssert($"Check {zzz_string} is at bottom", () => carousel.BeatmapSets.Last().Metadata.Author.Username == zzz_string);
            AddStep("Sort by artist", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Artist }, false));
            AddAssert($"Check {zzz_string} is at bottom", () => carousel.BeatmapSets.Last().Metadata.Artist == zzz_string);
        }

        [Test]
        public void TestSortingStability()
        {
            var sets = new List<BeatmapSetInfo>();

            for (int i = 0; i < 20; i++)
            {
                var set = TestResources.CreateTestBeatmapSetInfo();

                // only need to set the first as they are a shared reference.
                var beatmap = set.Beatmaps.First();

                beatmap.Metadata.Artist = "same artist";
                beatmap.Metadata.Title = "same title";

                sets.Add(set);
            }

            int idOffset = sets.First().OnlineID;

            loadBeatmaps(sets);

            AddStep("Sort by artist", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Artist }, false));
            AddAssert("Items remain in original order", () => carousel.BeatmapSets.Select((set, index) => set.OnlineID == index + idOffset).All(b => b));

            AddStep("Sort by title", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Title }, false));
            AddAssert("Items remain in original order", () => carousel.BeatmapSets.Select((set, index) => set.OnlineID == index + idOffset).All(b => b));
        }

        [Test]
        public void TestSortingWithFiltered()
        {
            List<BeatmapSetInfo> sets = new List<BeatmapSetInfo>();

            for (int i = 0; i < 3; i++)
            {
                var set = TestResources.CreateTestBeatmapSetInfo(3);
                set.Beatmaps[0].StarRating = 3 - i;
                set.Beatmaps[2].StarRating = 6 + i;
                sets.Add(set);
            }

            loadBeatmaps(sets);

            AddStep("Filter to normal", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Difficulty, SearchText = "Normal" }, false));
            AddAssert("Check first set at end", () => carousel.BeatmapSets.First().Equals(sets.Last()));
            AddAssert("Check last set at start", () => carousel.BeatmapSets.Last().Equals(sets.First()));

            AddStep("Filter to insane", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Difficulty, SearchText = "Insane" }, false));
            AddAssert("Check first set at start", () => carousel.BeatmapSets.First().Equals(sets.First()));
            AddAssert("Check last set at end", () => carousel.BeatmapSets.Last().Equals(sets.Last()));
        }

        [Test]
        public void TestRemoveAll()
        {
            loadBeatmaps();

            setSelected(2, 1);
            AddAssert("Selection is non-null", () => currentSelection != null);

            AddStep("Remove selected", () => carousel.RemoveBeatmapSet(carousel.SelectedBeatmapSet));
            waitForSelection(2);

            AddStep("Remove first", () => carousel.RemoveBeatmapSet(carousel.BeatmapSets.First()));
            AddStep("Remove first", () => carousel.RemoveBeatmapSet(carousel.BeatmapSets.First()));
            waitForSelection(1);

            AddUntilStep("Remove all", () =>
            {
                if (!carousel.BeatmapSets.Any()) return true;

                carousel.RemoveBeatmapSet(carousel.BeatmapSets.Last());
                return false;
            });

            checkNoSelection();
        }

        [Test]
        public void TestEmptyTraversal()
        {
            loadBeatmaps(new List<BeatmapSetInfo>());

            advanceSelection(direction: 1, diff: false);
            checkNoSelection();

            advanceSelection(direction: 1, diff: true);
            checkNoSelection();

            advanceSelection(direction: -1, diff: false);
            checkNoSelection();

            advanceSelection(direction: -1, diff: true);
            checkNoSelection();
        }

        [Test]
        public void TestHiding()
        {
            BeatmapSetInfo hidingSet = null;
            List<BeatmapSetInfo> hiddenList = new List<BeatmapSetInfo>();

            AddStep("create hidden set", () =>
            {
                hidingSet = TestResources.CreateTestBeatmapSetInfo(3);
                hidingSet.Beatmaps[1].Hidden = true;

                hiddenList.Clear();
                hiddenList.Add(hidingSet);
            });

            loadBeatmaps(hiddenList);

            setSelected(1, 1);

            checkVisibleItemCount(true, 2);
            advanceSelection(true);
            waitForSelection(1, 3);

            setHidden(3);
            waitForSelection(1, 1);

            setHidden(2, false);
            advanceSelection(true);
            waitForSelection(1, 2);

            setHidden(1);
            waitForSelection(1, 2);

            setHidden(2);
            checkNoSelection();

            void setHidden(int diff, bool hidden = true)
            {
                AddStep((hidden ? "" : "un") + $"hide diff {diff}", () =>
                {
                    hidingSet.Beatmaps[diff - 1].Hidden = hidden;
                    carousel.UpdateBeatmapSet(hidingSet);
                });
            }
        }

        [Test]
        public void TestSelectingFilteredRuleset()
        {
            BeatmapSetInfo testMixed = null;

            createCarousel(new List<BeatmapSetInfo>());

            AddStep("add mixed ruleset beatmapset", () =>
            {
                testMixed = TestResources.CreateTestBeatmapSetInfo(3);

                for (int i = 0; i <= 2; i++)
                {
                    testMixed.Beatmaps[i].Ruleset = rulesets.AvailableRulesets.ElementAt(i);
                }

                carousel.UpdateBeatmapSet(testMixed);
            });
            AddStep("filter to ruleset 0", () =>
                carousel.Filter(new FilterCriteria { Ruleset = rulesets.AvailableRulesets.ElementAt(0) }, false));
            AddStep("select filtered map skipping filtered", () => carousel.SelectBeatmap(testMixed.Beatmaps[1], false));
            AddAssert("unfiltered beatmap not selected", () => carousel.SelectedBeatmapInfo.Ruleset.OnlineID == 0);

            AddStep("remove mixed set", () =>
            {
                carousel.RemoveBeatmapSet(testMixed);
                testMixed = null;
            });
            BeatmapSetInfo testSingle = null;
            AddStep("add single ruleset beatmapset", () =>
            {
                testSingle = TestResources.CreateTestBeatmapSetInfo(3);
                testSingle.Beatmaps.ForEach(b =>
                {
                    b.Ruleset = rulesets.AvailableRulesets.ElementAt(1);
                });

                carousel.UpdateBeatmapSet(testSingle);
            });
            AddStep("select filtered map skipping filtered", () => carousel.SelectBeatmap(testSingle.Beatmaps[0], false));
            checkNoSelection();
            AddStep("remove single ruleset set", () => carousel.RemoveBeatmapSet(testSingle));
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            List<BeatmapSetInfo> manySets = new List<BeatmapSetInfo>();

            for (int i = 1; i <= 50; i++)
                manySets.Add(TestResources.CreateTestBeatmapSetInfo(3));

            loadBeatmaps(manySets);

            advanceSelection(direction: 1, diff: false);

            for (int i = 0; i < 5; i++)
            {
                AddStep("Toggle non-matching filter", () =>
                {
                    carousel.Filter(new FilterCriteria { SearchText = Guid.NewGuid().ToString() }, false);
                });

                AddStep("Restore no filter", () =>
                {
                    carousel.Filter(new FilterCriteria(), false);
                    eagerSelectedIDs.Add(carousel.SelectedBeatmapSet.ID);
                });
            }

            // always returns to same selection as long as it's available.
            AddAssert("Selection was remembered", () => eagerSelectedIDs.Count == 1);
        }

        [Test]
        public void TestRandomFallbackOnNonMatchingPrevious()
        {
            List<BeatmapSetInfo> manySets = new List<BeatmapSetInfo>();

            AddStep("populate maps", () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    manySets.Add(TestResources.CreateTestBeatmapSetInfo(3, new[]
                    {
                        // all taiko except for first
                        rulesets.GetRuleset(i > 0 ? 1 : 0)
                    }));
                }
            });

            loadBeatmaps(manySets);

            for (int i = 0; i < 10; i++)
            {
                AddStep("Reset filter", () => carousel.Filter(new FilterCriteria(), false));

                AddStep("select first beatmap", () => carousel.SelectBeatmap(manySets.First().Beatmaps.First()));

                AddStep("Toggle non-matching filter", () =>
                {
                    carousel.Filter(new FilterCriteria { SearchText = Guid.NewGuid().ToString() }, false);
                });

                AddAssert("selection lost", () => carousel.SelectedBeatmapInfo == null);

                AddStep("Restore different ruleset filter", () =>
                {
                    carousel.Filter(new FilterCriteria { Ruleset = rulesets.GetRuleset(1) }, false);
                    eagerSelectedIDs.Add(carousel.SelectedBeatmapSet.ID);
                });

                AddAssert("selection changed", () => !carousel.SelectedBeatmapInfo.Equals(manySets.First().Beatmaps.First()));
            }

            AddAssert("Selection was random", () => eagerSelectedIDs.Count > 2);
        }

        [Test]
        public void TestFilteringByUserStarDifficulty()
        {
            BeatmapSetInfo set = null;

            loadBeatmaps(new List<BeatmapSetInfo>());

            AddStep("add mixed difficulty set", () =>
            {
                set = TestResources.CreateTestBeatmapSetInfo(1);
                set.Beatmaps.Clear();

                for (int i = 1; i <= 15; i++)
                {
                    set.Beatmaps.Add(new BeatmapInfo(new OsuRuleset().RulesetInfo, new BeatmapDifficulty(), new BeatmapMetadata())
                    {
                        DifficultyName = $"Stars: {i}",
                        StarRating = i,
                    });
                }

                carousel.UpdateBeatmapSet(set);
            });

            AddStep("select added set", () => carousel.SelectBeatmap(set.Beatmaps[0], false));

            AddStep("filter [5..]", () => carousel.Filter(new FilterCriteria { UserStarDifficulty = { Min = 5 } }));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(true, 11);

            AddStep("filter to [0..7]", () => carousel.Filter(new FilterCriteria { UserStarDifficulty = { Max = 7 } }));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(true, 7);

            AddStep("filter to [5..7]", () => carousel.Filter(new FilterCriteria { UserStarDifficulty = { Min = 5, Max = 7 } }));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(true, 3);

            AddStep("filter [2..2]", () => carousel.Filter(new FilterCriteria { UserStarDifficulty = { Min = 2, Max = 2 } }));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(true, 1);

            AddStep("filter to [0..]", () => carousel.Filter(new FilterCriteria { UserStarDifficulty = { Min = 0 } }));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(true, 15);
        }

        private void loadBeatmaps(List<BeatmapSetInfo> beatmapSets = null, Func<FilterCriteria> initialCriteria = null, Action<BeatmapCarousel> carouselAdjust = null, int? count = null, bool randomDifficulties = false)
        {
            bool changed = false;

            if (beatmapSets == null)
            {
                beatmapSets = new List<BeatmapSetInfo>();

                for (int i = 1; i <= (count ?? set_count); i++)
                {
                    beatmapSets.Add(randomDifficulties
                        ? TestResources.CreateTestBeatmapSetInfo()
                        : TestResources.CreateTestBeatmapSetInfo(3));
                }
            }

            createCarousel(beatmapSets, c =>
            {
                carouselAdjust?.Invoke(c);

                carousel.Filter(initialCriteria?.Invoke() ?? new FilterCriteria());
                carousel.BeatmapSetsChanged = () => changed = true;
                carousel.BeatmapSets = beatmapSets;
            });

            AddUntilStep("Wait for load", () => changed);
        }

        private void createCarousel(List<BeatmapSetInfo> beatmapSets, Action<BeatmapCarousel> carouselAdjust = null, Container target = null)
        {
            AddStep("Create carousel", () =>
            {
                selectedSets.Clear();
                eagerSelectedIDs.Clear();

                carousel = new TestBeatmapCarousel
                {
                    RelativeSizeAxes = Axes.Both,
                };

                carouselAdjust?.Invoke(carousel);

                carousel.BeatmapSets = beatmapSets;

                (target ?? this).Child = carousel;
            });
        }

        private void ensureRandomFetchSuccess() =>
            AddAssert("ensure prev random fetch worked", () => selectedSets.Peek().Equals(carousel.SelectedBeatmapSet));

        private void waitForSelection(int set, int? diff = null) =>
            AddUntilStep($"selected is set{set}{(diff.HasValue ? $" diff{diff.Value}" : "")}", () =>
            {
                if (diff != null)
                    return carousel.SelectedBeatmapInfo?.Equals(carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Skip(diff.Value - 1).First()) == true;

                return carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Contains(carousel.SelectedBeatmapInfo);
            });

        private void setSelected(int set, int diff) =>
            AddStep($"select set{set} diff{diff}", () =>
                carousel.SelectBeatmap(carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Skip(diff - 1).First()));

        private void advanceSelection(bool diff, int direction = 1, int count = 1)
        {
            if (count == 1)
            {
                AddStep($"select {(direction > 0 ? "next" : "prev")} {(diff ? "diff" : "set")}", () =>
                    carousel.SelectNext(direction, !diff));
            }
            else
            {
                AddRepeatStep($"select {(direction > 0 ? "next" : "prev")} {(diff ? "diff" : "set")}", () =>
                    carousel.SelectNext(direction, !diff), count);
            }
        }

        private void checkVisibleItemCount(bool diff, int count)
        {
            // until step required as we are querying against alive items, which are loaded asynchronously inside DrawableCarouselBeatmapSet.
            AddUntilStep($"{count} {(diff ? "diffs" : "sets")} visible", () =>
                carousel.Items.Count(s => (diff ? s.Item is CarouselBeatmap : s.Item is CarouselBeatmapSet) && s.Item.Visible) == count);
        }

        private void checkSelectionIsCentered()
        {
            AddAssert("Selected panel is centered", () =>
            {
                return Precision.AlmostEquals(
                    carousel.ScreenSpaceDrawQuad.Centre,
                    carousel.Items
                            .First(i => i.Item.State.Value == CarouselItemState.Selected)
                            .ScreenSpaceDrawQuad.Centre, 100);
            });
        }

        private void checkNoSelection() => AddAssert("Selection is null", () => currentSelection == null);

        private void nextRandom() =>
            AddStep("select random next", () =>
            {
                carousel.RandomAlgorithm.Value = RandomSelectAlgorithm.RandomPermutation;

                if (!selectedSets.Any() && carousel.SelectedBeatmapInfo != null)
                    selectedSets.Push(carousel.SelectedBeatmapSet);

                carousel.SelectNextRandom();
                selectedSets.Push(carousel.SelectedBeatmapSet);
            });

        private void ensureRandomDidntRepeat() =>
            AddAssert("ensure no repeats", () => selectedSets.Distinct().Count() == selectedSets.Count);

        private void prevRandom() => AddStep("select random last", () =>
        {
            carousel.SelectPreviousRandom();
            selectedSets.Pop();
        });

        private bool selectedBeatmapVisible()
        {
            var currentlySelected = carousel.Items.FirstOrDefault(s => s.Item is CarouselBeatmap && s.Item.State.Value == CarouselItemState.Selected);
            if (currentlySelected == null)
                return true;

            return currentlySelected.Item.Visible;
        }

        private void checkInvisibleDifficultiesUnselectable()
        {
            nextRandom();
            AddAssert("Selection is visible", selectedBeatmapVisible);
        }

        private class TestBeatmapCarousel : BeatmapCarousel
        {
            public bool PendingFilterTask => PendingFilter != null;

            public IEnumerable<DrawableCarouselItem> Items
            {
                get
                {
                    foreach (var item in Scroll.Children)
                    {
                        yield return item;

                        if (item is DrawableCarouselBeatmapSet set)
                        {
                            foreach (var difficulty in set.DrawableBeatmaps)
                                yield return difficulty;
                        }
                    }
                }
            }
        }
    }
}
