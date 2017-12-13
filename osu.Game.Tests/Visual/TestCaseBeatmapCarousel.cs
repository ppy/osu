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
                carousel.Beatmaps = new[]
                {
                    createTestBeatmapSet(1),
                    createTestBeatmapSet(2),
                    createTestBeatmapSet(3),
                    createTestBeatmapSet(4),
                };
            });

            void checkSelected(int set, int diff) =>
                AddAssert($"selected is set{set} diff{diff}", () =>
                    carousel.SelectedBeatmap == carousel.Beatmaps.Skip(set - 1).First().Beatmaps.Skip(diff - 1).First());

            void setSelected(int set, int diff) =>
                AddStep($"select set{set} diff{diff}", () =>
                    carousel.SelectBeatmap(carousel.Beatmaps.Skip(set - 1).First().Beatmaps.Skip(diff - 1).First()));

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

            AddUntilStep(() => carousel.Beatmaps.Any(), "Wait for load");

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

            setSelected(1, diff: 2);
        }

        private BeatmapSetInfo createTestBeatmapSet(int i)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 1234 + i,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    OnlineBeatmapSetID = 1234 + i,
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "peppy",
                    Title = "test set #" + i,
                    AuthorString = "peppy"
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1234 + i,
                        Path = "normal.osu",
                        Version = "Normal",
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 3.5f,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1235 + i,
                        Path = "hard.osu",
                        Version = "Hard",
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 5,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1236 + i,
                        Path = "insane.osu",
                        Version = "Insane",
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
