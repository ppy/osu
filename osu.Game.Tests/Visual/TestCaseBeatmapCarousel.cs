// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseBeatmapCarousel : OsuTestCase
    {
        private TestBeatmapCarousel carousel;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(carousel = new TestBeatmapCarousel
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep("Load Beatmaps", () =>
            {
                carousel.BeatmapSets = new[]
                {
                    createTestBeatmapSet(1),
                    createTestBeatmapSet(2),
                    createTestBeatmapSet(3),
                    createTestBeatmapSet(4),
                };
            });

            void checkSelected(int set, int diff) =>
                AddAssert($"selected is set{set} diff{diff}", () =>
                    carousel.SelectedBeatmap == carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Skip(diff - 1).First());

            void setSelected(int set, int diff) =>
                AddStep($"select set{set} diff{diff}", () =>
                    carousel.SelectBeatmap(carousel.BeatmapSets.Skip(set - 1).First().Beatmaps.Skip(diff - 1).First()));

            void advanceSelection(bool diff, int direction = 1, int count = 1)
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

            void checkVisibleItemCount(bool diff, int count) =>
                AddAssert($"{count} {(diff ? "diff" : "set")} visible", () =>
                    carousel.Items.Count(s => (diff ? s.Item is CarouselBeatmap : s.Item is CarouselBeatmapSet) && s.Item.Visible) == count);

            AddUntilStep(() => carousel.BeatmapSets.Any(), "Wait for load");

            // test traversal

            advanceSelection(direction: 1, diff: false);
            checkSelected(1, 1);

            advanceSelection(direction: 1, diff: true);
            checkSelected(1, 2);

            advanceSelection(direction: -1, diff: false);
            checkSelected(4, 1);

            advanceSelection(direction: -1, diff: true);
            checkSelected(3, 3);

            advanceSelection(diff: false);
            advanceSelection(diff: false);
            checkSelected(1, 1);

            advanceSelection(direction: -1, diff: true);
            checkSelected(4, 3);

            // test basic filtering

            AddStep("Filter", () => carousel.Filter(new FilterCriteria { SearchText = "set #3" }, false));
            checkVisibleItemCount(diff: false, count: 1);
            checkVisibleItemCount(diff: true, count: 3);
            checkSelected(3, 1);

            advanceSelection(diff: true, count: 4);
            checkSelected(3, 2);

            AddStep("Un-filter (debounce)", () => carousel.Filter(new FilterCriteria()));
            AddUntilStep(() => !carousel.PendingFilterTask, "Wait for debounce");
            checkVisibleItemCount(diff: false, count: 4);
            checkVisibleItemCount(diff: true, count: 3);

            // test filtering some difficulties (and keeping current beatmap set selected).

            setSelected(1, 2);
            AddStep("Filter some difficulties", () => carousel.Filter(new FilterCriteria { SearchText = "Normal" }, false));
            checkSelected(1, 1);
            AddStep("Un-filter", () => carousel.Filter(new FilterCriteria(), false));
            checkSelected(1, 1);

            // test random non-repeating algorithm

            Stack<BeatmapSetInfo> selectedSets = new Stack<BeatmapSetInfo>();

            void nextRandom() =>
                AddStep("select random next", () =>
                {
                    carousel.RandomAlgorithm.Value = RandomSelectAlgorithm.RandomPermutation;

                    if (!selectedSets.Any() && carousel.SelectedBeatmap != null)
                        selectedSets.Push(carousel.SelectedBeatmapSet);

                    carousel.SelectNextRandom();
                    selectedSets.Push(carousel.SelectedBeatmapSet);
                });

            void ensureRandomDidntRepeat() =>
                AddAssert("ensure no repeats", () => selectedSets.Distinct().Count() == selectedSets.Count);

            void prevRandom() => AddStep("select random last", () =>
            {
                carousel.SelectPreviousRandom();
                selectedSets.Pop();
            });

            void ensureRandomFetchSuccess() =>
                AddAssert("ensure prev random fetch worked", () => selectedSets.Peek() == carousel.SelectedBeatmapSet);

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

            // test adding and removing

            AddStep("Add new set #5", () => carousel.UpdateBeatmapSet(createTestBeatmapSet(5)));
            AddStep("Add new set #6", () => carousel.UpdateBeatmapSet(createTestBeatmapSet(6)));

            checkVisibleItemCount(false, 6);

            AddStep("Remove set #4", () => carousel.RemoveBeatmapSet(createTestBeatmapSet(4)));

            checkVisibleItemCount(false, 5);


        }

        private BeatmapSetInfo createTestBeatmapSet(int i)
        {
            return new BeatmapSetInfo
            {
                ID = i,
                OnlineBeatmapSetID = i,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    OnlineBeatmapSetID = i,
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "peppy",
                    Title = "test set #" + i,
                    AuthorString = "peppy",
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = i * 10,
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
                        OnlineBeatmapID = i * 10 + 1,
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
                        OnlineBeatmapID = i * 10 + 2,
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

        private class TestBeatmapCarousel : BeatmapCarousel
        {
            public new List<DrawableCarouselItem> Items => base.Items;

            public bool PendingFilterTask => FilterTask != null;
        }
    }
}
