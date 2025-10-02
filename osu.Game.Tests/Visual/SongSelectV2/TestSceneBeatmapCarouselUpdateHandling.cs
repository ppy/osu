// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselUpdateHandling : BeatmapCarouselTestScene
    {
        private BeatmapSetInfo baseTestBeatmap = null!;

        private const int initial_filter_count = 3;

        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
            WaitForFiltering();
            AddStep("add beatmap", () =>
            {
                var beatmap = CreateTestBeatmapSetInfo(3, false);
                Realm.Write(r => r.Add(beatmap, update: true));
                BeatmapSets.Add(beatmap.Detach());
            });
            WaitForFiltering();
            AddStep("generate and add test beatmap", () =>
            {
                baseTestBeatmap = TestResources.CreateTestBeatmapSetInfo(3);

                var metadata = new BeatmapMetadata
                {
                    Artist = "update test",
                    Title = "beatmap",
                };

                foreach (var b in baseTestBeatmap.Beatmaps)
                    b.Metadata = metadata;

                Realm.Write(r => r.Add(baseTestBeatmap, update: true));
                BeatmapSets.Add(baseTestBeatmap.Detach());
            });
            WaitForFiltering();

            AddAssert("filter count correct", () => Carousel.FilterCount, () => Is.EqualTo(initial_filter_count));
        }

        [Test]
        public void TestBeatmapSetUpdatedNoop()
        {
            List<Panel> originalDrawables = new List<Panel>();

            AddStep("store drawable references", () =>
            {
                originalDrawables.Clear();
                originalDrawables.AddRange(Carousel.ChildrenOfType<Panel>().ToList());
            });

            AddStep("update beatmap with same reference", () => BeatmapSets.ReplaceRange(1, 1, [baseTestBeatmap]));

            WaitForFiltering();
            AddAssert("drawables unchanged", () => Carousel.ChildrenOfType<Panel>(), () => Is.EqualTo(originalDrawables));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestScrollPositionMaintainedWhenSetUpdated(bool difficultySort)
        {
            if (difficultySort)
            {
                SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);
                assertDidFilter(1);
            }

            Panel panel = null!;

            AddStep("find panel", () => panel = Carousel.ChildrenOfType<Panel>().First(p => p.Item != null && p.ChildrenOfType<OsuSpriteText>().Any(t => t.Text.ToString() == "beatmap")));

            AddStep("select panel", () => panel.TriggerClick());

            AddStep("scroll to end", () =>
            {
                // must trigger a user scroll so that carousel doesn't follow the selection.
                InputManager.MoveMouseTo(Carousel);
                InputManager.ScrollVerticalBy(-1000);
            });

            AddUntilStep("is scrolled to end", () => Carousel.ChildrenOfType<UserTrackingScrollContainer>().Single().IsScrolledToEnd());

            updateBeatmap(b =>
            {
                // hash will be updated when important metadata changes, such as title, difficulty, author etc.
                b.Hash = "new hash";
                b.Metadata = new BeatmapMetadata
                {
                    Artist = "updated test",
                    Title = $"beatmap {RNG.Next().ToString()}"
                };
            });

            assertDidFilter(difficultySort ? 2 : 1);
            WaitForFiltering();

            AddAssert("scroll is still at end", () => Carousel.ChildrenOfType<UserTrackingScrollContainer>().Single().IsScrolledToEnd());
        }

        [Test]
        public void TestBeatmapSetMetadataUpdated()
        {
            PanelBeatmapSet panel = null!;

            var metadata = new BeatmapMetadata
            {
                Artist = "updated test",
                Title = "new beatmap title",
            };

            List<Panel> originalDrawables = new List<Panel>();

            AddStep("store drawable references", () =>
            {
                originalDrawables.Clear();
                originalDrawables.AddRange(Carousel.ChildrenOfType<Panel>().ToList());
            });

            AddStep("find panel", () => panel = Carousel.ChildrenOfType<PanelBeatmapSet>().Single(p => p.ChildrenOfType<OsuSpriteText>().Any(t => t.Text.ToString() == "beatmap")));

            updateBeatmap(b =>
            {
                b.Metadata = metadata;
                // hash will be updated when important metadata changes, such as title, difficulty, author etc.
                b.Hash = "new hash";
            });

            assertDidFilter();
            WaitForFiltering();

            AddAssert("drawables unchanged", () => Carousel.ChildrenOfType<Panel>(), () => Is.EqualTo(originalDrawables));

            AddAssert("title updated", () => panel.ChildrenOfType<OsuSpriteText>().Any(t => t.Text.ToString() == metadata.Title));
        }

        [Test]
        public void TestOnlineStatusUpdated()
        {
            List<Panel> originalDrawables = new List<Panel>();

            AddStep("store drawable references", () =>
            {
                originalDrawables.Clear();
                originalDrawables.AddRange(Carousel.ChildrenOfType<Panel>().ToList());
            });

            updateBeatmap(b => b.Status = BeatmapOnlineStatus.Graveyard);

            assertDidFilter();
            WaitForFiltering();

            AddAssert("drawables unchanged", () => Carousel.ChildrenOfType<Panel>(), () => Is.EqualTo(originalDrawables));
        }

        [Test]
        public void TestNoUpdateTriggeredOnUserTagsChange()
        {
            var metadata = new BeatmapMetadata
            {
                Artist = "updated test",
                Title = "new beatmap title",
                UserTags = { "hi" }
            };

            updateBeatmap(b => b.Metadata = metadata);
            assertDidNotFilter();
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void TestSelectionHeld(bool difficultySort, bool hashChanged)
        {
            SelectNextSet();

            if (difficultySort)
            {
                SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);
                assertDidFilter(1);
            }

            WaitForSetSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            updateBeatmap(b =>
            {
                if (hashChanged)
                    b.Hash = "new hash";
            });

            int baseFilterCount = difficultySort ? 1 : 0;

            if (hashChanged)
                assertDidFilter(baseFilterCount + 1);
            else
                assertDidFilter(baseFilterCount);

            WaitForFiltering();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

        [Test] // Checks that we keep selection based on online ID where possible.
        public void TestSelectionHeldDifficultyNameChanged()
        {
            SelectNextSet();

            WaitForSetSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            updateBeatmap(b => b.DifficultyName = "new name");
            assertDidFilter();
            WaitForFiltering();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

        [Test] // Checks that we fallback to keeping selection based on difficulty name.
        public void TestSelectionHeldDifficultyOnlineIDChanged()
        {
            SelectNextSet();

            WaitForSetSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            updateBeatmap(b => b.OnlineID = b.OnlineID + 1);
            assertDidFilter();
            WaitForFiltering();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

        [Test] // Checks that we don't crash if there exists a difficulty with the same online ID as the selected difficulty.
        public void TestDifferentDifficultiesWithSameOnlineID()
        {
            SelectNextSet();

            WaitForSetSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            // Add another difficulty with same online ID.
            updateBeatmap(null, bs =>
            {
                var newBeatmap = createBeatmap(bs);
                newBeatmap.OnlineID = baseTestBeatmap.Beatmaps[0].OnlineID;
                bs.Beatmaps.Add(newBeatmap);
            });

            WaitForFiltering();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

        [Test] // Checks that we don't crash if there exists a difficulty with the same name as the selected difficulty.
        public void TestDifferentDifficultiesWithSameName()
        {
            SelectNextSet();

            WaitForSetSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            // Remove original selected difficulty, and add two difficulties with same name as selection.
            updateBeatmap(null, bs =>
            {
                string selectedName = bs.Beatmaps[0].DifficultyName;
                Realm.Write(r => r.Remove(r.Find<BeatmapInfo>(bs.Beatmaps[0].ID)!));
                bs.Beatmaps.RemoveAt(0);

                var newBeatmap = createBeatmap(bs);
                newBeatmap.ID = Guid.NewGuid();
                newBeatmap.DifficultyName = selectedName;
                newBeatmap.OnlineID = -1;
                bs.Beatmaps.Add(newBeatmap);

                newBeatmap = createBeatmap(bs);
                newBeatmap.ID = Guid.NewGuid();
                newBeatmap.DifficultyName = selectedName;
                newBeatmap.OnlineID = -1;
                bs.Beatmaps.Add(newBeatmap);
            });

            WaitForFiltering();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentBeatmap, () => Is.EqualTo(BeatmapSets[1].Beatmaps[2]));
            AddAssert("visible panel is updateable beatmap", () => (GetSelectedPanel()?.Item?.Model as GroupedBeatmap)?.Beatmap, () => Is.EqualTo(BeatmapSets[1].Beatmaps[2]));
        }

        /// <summary>
        /// Ensures stability is maintained on different sort modes while an item is removed and then immediately re-added.
        /// </summary>
        [Test]
        public void TestSortingStabilityWithRemovedAndReaddedItem()
        {
            RemoveAllBeatmaps();

            const int diff_count = 5;

            AddStep("Populate beatmap sets", () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diff_count);

                    // only need to set the first as they are a shared reference.
                    var beatmap = set.Beatmaps.First();

                    beatmap.Metadata.Artist = "same artist";
                    beatmap.Metadata.Title = "same title";

                    // testing the case where DateAdded happens to equal (quite rare).
                    set.DateAdded = DateTimeOffset.UnixEpoch;

                    BeatmapSets.Add(set);
                }
            });

            BeatmapSetInfo removedBeatmap = null!;
            Guid[] originalOrder = null!;

            SortBy(SortMode.Artist);

            AddAssert("Items in descending added order", () => Carousel.PostFilterBeatmaps.Select(b => b.BeatmapSet!.DateAdded), () => Is.Ordered.Descending);
            AddStep("Save order", () => originalOrder = Carousel.PostFilterBeatmaps.Select(b => b.ID).ToArray());

            AddStep("Remove item", () =>
            {
                removedBeatmap = BeatmapSets[1];
                BeatmapSets.RemoveAt(1);
            });
            AddStep("Re-add item", () => BeatmapSets.Insert(1, removedBeatmap));
            WaitForFiltering();

            AddAssert("Order didn't change", () => Carousel.PostFilterBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));

            SortBy(SortMode.Title);

            AddAssert("Order didn't change", () => Carousel.PostFilterBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));
        }

        /// <summary>
        /// Ensures stability is maintained on different sort modes while a new item is added to the carousel.
        /// </summary>
        [Test]
        public void TestSortingStabilityWithNewItems()
        {
            RemoveAllBeatmaps();

            const int diff_count = 5;

            AddStep("Populate beatmap sets", () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diff_count);

                    // only need to set the first as they are a shared reference.
                    var beatmap = set.Beatmaps.First();

                    beatmap.Metadata.Artist = "same artist";
                    beatmap.Metadata.Title = "same title";

                    // testing the case where DateAdded happens to equal (quite rare).
                    set.DateAdded = DateTimeOffset.UnixEpoch;

                    BeatmapSets.Add(set);
                }
            });

            Guid[] originalOrder = null!;

            SortBy(SortMode.Artist);

            AddAssert("Items in descending added order", () => Carousel.PostFilterBeatmaps.Select(b => b.BeatmapSet!.DateAdded), () => Is.Ordered.Descending);
            AddStep("Save order", () => originalOrder = Carousel.PostFilterBeatmaps.Select(b => b.ID).ToArray());

            AddStep("Add new item", () =>
            {
                var set = TestResources.CreateTestBeatmapSetInfo();

                // only need to set the first as they are a shared reference.
                var beatmap = set.Beatmaps.First();

                beatmap.Metadata.Artist = "same artist";
                beatmap.Metadata.Title = "same title";

                set.DateAdded = DateTimeOffset.FromUnixTimeSeconds(1);

                BeatmapSets.Add(set);

                // add set to expected ordering
                originalOrder = set.Beatmaps.Select(b => b.ID).Concat(originalOrder).ToArray();
            });
            WaitForFiltering();

            AddAssert("Order didn't change", () => Carousel.PostFilterBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));

            SortBy(SortMode.Title);

            AddAssert("Order didn't change", () => Carousel.PostFilterBeatmaps.Select(b => b.ID), () => Is.EqualTo(originalOrder));
        }

        private void assertDidFilter(int count = 1) => AddAssert("did filter", () => Carousel.FilterCount, () => Is.EqualTo(initial_filter_count + count));

        private void assertDidNotFilter() => AddAssert("did not filter", () => Carousel.FilterCount, () => Is.EqualTo(initial_filter_count));

        private void updateBeatmap(Action<BeatmapInfo>? updateBeatmap = null, Action<BeatmapSetInfo>? updateSet = null)
        {
            AddStep("update beatmap with different reference", () =>
            {
                var updatedSet = new BeatmapSetInfo
                {
                    ID = baseTestBeatmap.ID,
                    OnlineID = baseTestBeatmap.OnlineID,
                    DateAdded = baseTestBeatmap.DateAdded,
                    DateSubmitted = baseTestBeatmap.DateSubmitted,
                    DateRanked = baseTestBeatmap.DateRanked,
                    Status = baseTestBeatmap.Status,
                    StatusInt = baseTestBeatmap.StatusInt,
                    DeletePending = baseTestBeatmap.DeletePending,
                    Hash = baseTestBeatmap.Hash,
                    Protected = baseTestBeatmap.Protected,
                };

                var updatedBeatmaps = baseTestBeatmap.Beatmaps.Select(b =>
                {
                    var updatedBeatmap = createBeatmap(updatedSet, b);

                    updateBeatmap?.Invoke(updatedBeatmap);

                    return updatedBeatmap;
                }).ToList();

                updatedSet.Beatmaps.AddRange(updatedBeatmaps);

                updateSet?.Invoke(updatedSet);

                int originalIndex = BeatmapSets.IndexOf(baseTestBeatmap);

                Realm.Write(r => r.Add(updatedSet, update: true));
                BeatmapSets.ReplaceRange(originalIndex, 1, [updatedSet.Detach()]);
            });
        }

        private BeatmapInfo createBeatmap(BeatmapSetInfo set, BeatmapInfo? reference = null)
        {
            reference ??= baseTestBeatmap.Beatmaps.First();

            var updatedBeatmap = new BeatmapInfo
            {
                ID = reference.ID,
                Metadata = reference.Metadata,
                Ruleset = reference.Ruleset,
                DifficultyName = reference.DifficultyName,
                BeatmapSet = set,
                Status = reference.Status,
                OnlineID = reference.OnlineID,
                Length = reference.Length,
                BPM = reference.BPM,
                Hash = reference.Hash,
                StarRating = reference.StarRating,
                MD5Hash = reference.MD5Hash,
                OnlineMD5Hash = reference.OnlineMD5Hash,
            };

            return updatedBeatmap;
        }
    }
}
