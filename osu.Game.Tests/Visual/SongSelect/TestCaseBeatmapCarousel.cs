// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestCaseBeatmapCarousel : OsuTestCase
    {
        private TestBeatmapCarousel carousel;
        private RulesetStore rulesets;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CarouselItem),
            typeof(CarouselGroup),
            typeof(CarouselGroupEagerSelect),
            typeof(CarouselBeatmap),
            typeof(CarouselBeatmapSet),

            typeof(DrawableCarouselItem),
            typeof(CarouselItemState),

            typeof(DrawableCarouselBeatmap),
            typeof(DrawableCarouselBeatmapSet),
        };

        private readonly Stack<BeatmapSetInfo> selectedSets = new Stack<BeatmapSetInfo>();
        private readonly HashSet<int> eagerSelectedIDs = new HashSet<int>();

        private BeatmapInfo currentSelection;

        private const int set_count = 5;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;

            Add(carousel = new TestBeatmapCarousel
            {
                RelativeSizeAxes = Axes.Both,
            });

            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            for (int i = 1; i <= set_count; i++)
                beatmapSets.Add(createTestBeatmapSet(i));

            carousel.SelectionChanged = s => currentSelection = s;

            loadBeatmaps(beatmapSets);

            testTraversal();
            testFiltering();
            testRandom();
            testAddRemove();
            testSorting();

            testRemoveAll();
            testEmptyTraversal();
            testHiding();
            testSelectingFilteredRuleset();
            testCarouselRootIsRandom();
        }

        private void loadBeatmaps(List<BeatmapSetInfo> beatmapSets)
        {
            bool changed = false;
            AddStep($"Load {beatmapSets.Count} Beatmaps", () =>
            {
                carousel.BeatmapSetsChanged = () => changed = true;
                carousel.BeatmapSets = beatmapSets;
            });
            AddUntilStep("Wait for load", () => changed);
        }

        private void ensureRandomFetchSuccess() =>
            AddAssert("ensure prev random fetch worked", () => selectedSets.Peek() == carousel.SelectedBeatmapSet);

        private void checkSelected(int set, int? diff = null) =>
            AddAssert($"selected is set{set}{(diff.HasValue ? $" diff{diff.Value}" : "")}", () =>
            {
                if (diff != null)
                    return carousel.SelectedBeatmap == carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Skip(diff.Value - 1).First();

                return carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Contains(carousel.SelectedBeatmap);
            });

        private void setSelected(int set, int diff) =>
            AddStep($"select set{set} diff{diff}", () =>
                carousel.SelectBeatmap(carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Skip(diff - 1).First()));

        private void advanceSelection(bool diff, int direction = 1, int count = 1)
        {
            if (count == 1)
                AddStep($"select {(direction > 0 ? "next" : "prev")} {(diff ? "diff" : "set")}", () =>
                    carousel.SelectNext(direction, !diff));
            else
            {
                AddRepeatStep($"select {(direction > 0 ? "next" : "prev")} {(diff ? "diff" : "set")}", () =>
                    carousel.SelectNext(direction, !diff), count);
            }
        }

        private void checkVisibleItemCount(bool diff, int count) =>
            AddAssert($"{count} {(diff ? "diffs" : "sets")} visible", () =>
                carousel.Items.Count(s => (diff ? s.Item is CarouselBeatmap : s.Item is CarouselBeatmapSet) && s.Item.Visible) == count);

        private void checkNoSelection() => AddAssert("Selection is null", () => currentSelection == null);

        private void nextRandom() =>
            AddStep("select random next", () =>
            {
                carousel.RandomAlgorithm.Value = RandomSelectAlgorithm.RandomPermutation;

                if (!selectedSets.Any() && carousel.SelectedBeatmap != null)
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
            var currentlySelected = carousel.Items.Find(s => s.Item is CarouselBeatmap && s.Item.State.Value == CarouselItemState.Selected);
            if (currentlySelected == null)
                return true;

            return currentlySelected.Item.Visible;
        }

        private void checkInvisibleDifficultiesUnselectable()
        {
            nextRandom();
            AddAssert("Selection is visible", selectedBeatmapVisible);
        }

        private void checkNonmatchingFilter()
        {
            AddStep("Toggle non-matching filter", () =>
            {
                carousel.Filter(new FilterCriteria { SearchText = "Dingo" }, false);
                carousel.Filter(new FilterCriteria(), false);
                eagerSelectedIDs.Add(carousel.SelectedBeatmapSet.ID);
            });
        }

        /// <summary>
        /// Test keyboard traversal
        /// </summary>
        private void testTraversal()
        {
            advanceSelection(direction: 1, diff: false);
            checkSelected(1, 1);

            advanceSelection(direction: 1, diff: true);
            checkSelected(1, 2);

            advanceSelection(direction: -1, diff: false);
            checkSelected(set_count, 1);

            advanceSelection(direction: -1, diff: true);
            checkSelected(set_count - 1, 3);

            advanceSelection(diff: false);
            advanceSelection(diff: false);
            checkSelected(1, 2);

            advanceSelection(direction: -1, diff: true);
            advanceSelection(direction: -1, diff: true);
            checkSelected(set_count, 3);
        }

        /// <summary>
        /// Test filtering
        /// </summary>
        private void testFiltering()
        {
            // basic filtering

            setSelected(1, 1);

            AddStep("Filter", () => carousel.Filter(new FilterCriteria { SearchText = "set #3!" }, false));
            checkVisibleItemCount(diff: false, count: 1);
            checkVisibleItemCount(diff: true, count: 3);
            checkSelected(3, 1);

            advanceSelection(diff: true, count: 4);
            checkSelected(3, 2);

            AddStep("Un-filter (debounce)", () => carousel.Filter(new FilterCriteria()));
            AddUntilStep("Wait for debounce", () => !carousel.PendingFilterTask);
            checkVisibleItemCount(diff: false, count: set_count);
            checkVisibleItemCount(diff: true, count: 3);

            // test filtering some difficulties (and keeping current beatmap set selected).

            setSelected(1, 2);
            AddStep("Filter some difficulties", () => carousel.Filter(new FilterCriteria { SearchText = "Normal" }, false));
            checkSelected(1, 1);

            AddStep("Un-filter", () => carousel.Filter(new FilterCriteria(), false));
            checkSelected(1, 1);

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
        }

        /// <summary>
        /// Test random non-repeating algorithm
        /// </summary>
        private void testRandom()
        {
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

            AddStep("Add set with 100 difficulties", () => carousel.UpdateBeatmapSet(createTestBeatmapSetWithManyDifficulties(set_count + 1)));
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
        private void testAddRemove()
        {
            AddStep("Add new set", () => carousel.UpdateBeatmapSet(createTestBeatmapSet(set_count + 1)));
            AddStep("Add new set", () => carousel.UpdateBeatmapSet(createTestBeatmapSet(set_count + 2)));

            checkVisibleItemCount(false, set_count + 2);

            AddStep("Remove set", () => carousel.RemoveBeatmapSet(createTestBeatmapSet(set_count + 2)));

            checkVisibleItemCount(false, set_count + 1);

            setSelected(set_count + 1, 1);

            AddStep("Remove set", () => carousel.RemoveBeatmapSet(createTestBeatmapSet(set_count + 1)));

            checkVisibleItemCount(false, set_count);

            checkSelected(set_count);
        }

        /// <summary>
        /// Test sorting
        /// </summary>
        private void testSorting()
        {
            AddStep("Sort by author", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Author }, false));
            AddAssert("Check zzzzz is at bottom", () => carousel.BeatmapSets.Last().Metadata.AuthorString == "zzzzz");
            AddStep("Sort by artist", () => carousel.Filter(new FilterCriteria { Sort = SortMode.Artist }, false));
            AddAssert($"Check #{set_count} is at bottom", () => carousel.BeatmapSets.Last().Metadata.Title.EndsWith($"#{set_count}!"));
        }

        private void testRemoveAll()
        {
            setSelected(2, 1);
            AddAssert("Selection is non-null", () => currentSelection != null);

            AddStep("Remove selected", () => carousel.RemoveBeatmapSet(carousel.SelectedBeatmapSet));
            checkSelected(2);

            AddStep("Remove first", () => carousel.RemoveBeatmapSet(carousel.BeatmapSets.First()));
            AddStep("Remove first", () => carousel.RemoveBeatmapSet(carousel.BeatmapSets.First()));
            checkSelected(1);

            AddUntilStep("Remove all", () =>
            {
                if (!carousel.BeatmapSets.Any()) return true;

                carousel.RemoveBeatmapSet(carousel.BeatmapSets.Last());
                return false;
            });

            checkNoSelection();
        }

        private void testEmptyTraversal()
        {
            advanceSelection(direction: 1, diff: false);
            checkNoSelection();

            advanceSelection(direction: 1, diff: true);
            checkNoSelection();

            advanceSelection(direction: -1, diff: false);
            checkNoSelection();

            advanceSelection(direction: -1, diff: true);
            checkNoSelection();
        }

        private void testHiding()
        {
            var hidingSet = createTestBeatmapSet(1);
            hidingSet.Beatmaps[1].Hidden = true;
            AddStep("Add set with diff 2 hidden", () => carousel.UpdateBeatmapSet(hidingSet));
            setSelected(1, 1);

            checkVisibleItemCount(true, 2);
            advanceSelection(true);
            checkSelected(1, 3);

            setHidden(3);
            checkSelected(1, 1);

            setHidden(2, false);
            advanceSelection(true);
            checkSelected(1, 2);

            setHidden(1);
            checkSelected(1, 2);

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

        private void testSelectingFilteredRuleset()
        {
            var testMixed = createTestBeatmapSet(set_count + 1);
            AddStep("add mixed ruleset beatmapset", () =>
            {
                for (int i = 0; i <= 2; i++)
                {
                    testMixed.Beatmaps[i].Ruleset = rulesets.AvailableRulesets.ElementAt(i);
                    testMixed.Beatmaps[i].RulesetID = i;
                }

                carousel.UpdateBeatmapSet(testMixed);
            });
            AddStep("filter to ruleset 0", () =>
                carousel.Filter(new FilterCriteria { Ruleset = rulesets.AvailableRulesets.ElementAt(0) }, false));
            AddStep("select filtered map skipping filtered", () => carousel.SelectBeatmap(testMixed.Beatmaps[1], false));
            AddAssert("unfiltered beatmap selected", () => carousel.SelectedBeatmap.Equals(testMixed.Beatmaps[0]));

            AddStep("remove mixed set", () =>
            {
                carousel.RemoveBeatmapSet(testMixed);
                testMixed = null;
            });
            var testSingle = createTestBeatmapSet(set_count + 2);
            testSingle.Beatmaps.ForEach(b =>
            {
                b.Ruleset = rulesets.AvailableRulesets.ElementAt(1);
                b.RulesetID = b.Ruleset.ID ?? 1;
            });
            AddStep("add single ruleset beatmapset", () => carousel.UpdateBeatmapSet(testSingle));
            AddStep("select filtered map skipping filtered", () => carousel.SelectBeatmap(testSingle.Beatmaps[0], false));
            checkNoSelection();
            AddStep("remove single ruleset set", () => carousel.RemoveBeatmapSet(testSingle));
        }

        private void testCarouselRootIsRandom()
        {
            List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            for (int i = 1; i <= 50; i++)
                beatmapSets.Add(createTestBeatmapSet(i));

            loadBeatmaps(beatmapSets);
            advanceSelection(direction: 1, diff: false);
            checkNonmatchingFilter();
            checkNonmatchingFilter();
            checkNonmatchingFilter();
            checkNonmatchingFilter();
            checkNonmatchingFilter();
            AddAssert("Selection was random", () => eagerSelectedIDs.Count > 1);
        }

        private BeatmapSetInfo createTestBeatmapSet(int id)
        {
            return new BeatmapSetInfo
            {
                ID = id,
                OnlineBeatmapSetID = id,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = $"peppy{id.ToString().PadLeft(6, '0')}",
                    Title = $"test set #{id}!",
                    AuthorString = string.Concat(Enumerable.Repeat((char)('z' - Math.Min(25, id - 1)), 5))
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = id * 10,
                        Path = "normal.osu",
                        Version = "Normal",
                        StarDifficulty = 2,
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 3.5f,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = id * 10 + 1,
                        Path = "hard.osu",
                        Version = "Hard",
                        StarDifficulty = 5,
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 5,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = id * 10 + 2,
                        Path = "insane.osu",
                        Version = "Insane",
                        StarDifficulty = 6,
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 7,
                        }
                    },
                }),
            };
        }

        private BeatmapSetInfo createTestBeatmapSetWithManyDifficulties(int id)
        {
            var toReturn = new BeatmapSetInfo
            {
                ID = id,
                OnlineBeatmapSetID = id,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = $"peppy{id.ToString().PadLeft(6, '0')}",
                    Title = $"test set #{id}!",
                    AuthorString = string.Concat(Enumerable.Repeat((char)('z' - Math.Min(25, id - 1)), 5))
                },
                Beatmaps = new List<BeatmapInfo>(),
            };
            for (int b = 1; b < 101; b++)
            {
                toReturn.Beatmaps.Add(new BeatmapInfo
                {
                    OnlineBeatmapID = b * 10,
                    Path = $"extra{b}.osu",
                    Version = $"Extra {b}",
                    StarDifficulty = 2,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    }
                });
            }

            return toReturn;
        }

        private class TestBeatmapCarousel : BeatmapCarousel
        {
            public new List<DrawableCarouselItem> Items => base.Items;

            public bool PendingFilterTask => PendingFilter != null;
        }
    }
}
