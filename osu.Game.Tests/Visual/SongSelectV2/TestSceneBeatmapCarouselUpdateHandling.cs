// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselUpdateHandling : BeatmapCarouselTestScene
    {
        private BeatmapSetInfo baseTestBeatmap = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
            AddBeatmaps(1, 3);
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
                BeatmapSets.Add(baseTestBeatmap);
            });

            WaitForSorting();
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

            WaitForSorting();
            AddAssert("drawables unchanged", () => Carousel.ChildrenOfType<Panel>(), () => Is.EqualTo(originalDrawables));
        }

        [Test]
        public void TestBeatmapSetMetadataUpdated()
        {
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

            updateBeatmap(b => b.Metadata = metadata);

            WaitForSorting();
            AddAssert("drawables changed", () => Carousel.ChildrenOfType<Panel>(), () => Is.Not.EqualTo(originalDrawables));
        }

        [Test]
        public void TestSelectionHeld()
        {
            SelectPrevGroup();

            WaitForSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentSelection, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            updateBeatmap();
            WaitForSorting();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentSelection, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

        [Test] // Checks that we keep selection based on online ID where possible.
        public void TestSelectionHeldDifficultyNameChanged()
        {
            SelectPrevGroup();

            WaitForSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentSelection, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            updateBeatmap(b => b.DifficultyName = "new name");
            WaitForSorting();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentSelection, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

        [Test] // Checks that we fallback to keeping selection based on difficulty name.
        public void TestSelectionHeldDifficultyOnlineIDChanged()
        {
            SelectPrevGroup();

            WaitForSelection(1, 0);
            AddAssert("selection is updateable beatmap", () => Carousel.CurrentSelection, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));

            updateBeatmap(b => b.OnlineID = b.OnlineID + 1);
            WaitForSorting();

            AddAssert("selection is updateable beatmap", () => Carousel.CurrentSelection, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
            AddAssert("visible panel is updateable beatmap", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(baseTestBeatmap.Beatmaps[0]));
        }

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

                updateSet?.Invoke(updatedSet);

                var updatedBeatmaps = baseTestBeatmap.Beatmaps.Select(b =>
                {
                    var updatedBeatmap = new BeatmapInfo
                    {
                        ID = b.ID,
                        Metadata = b.Metadata,
                        Ruleset = b.Ruleset,
                        DifficultyName = b.DifficultyName,
                        BeatmapSet = updatedSet,
                        Status = b.Status,
                        OnlineID = b.OnlineID,
                        Length = b.Length,
                        BPM = b.BPM,
                        Hash = b.Hash,
                        StarRating = b.StarRating,
                        MD5Hash = b.MD5Hash,
                        OnlineMD5Hash = b.OnlineMD5Hash,
                    };

                    updateBeatmap?.Invoke(updatedBeatmap);

                    return updatedBeatmap;
                }).ToList();

                updatedSet.Beatmaps.AddRange(updatedBeatmaps);

                int originalIndex = BeatmapSets.IndexOf(baseTestBeatmap);

                BeatmapSets.ReplaceRange(originalIndex, 1, [updatedSet]);
            });
        }
    }
}
