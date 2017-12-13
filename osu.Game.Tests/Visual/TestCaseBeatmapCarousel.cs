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
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseBeatmapCarousel : OsuTestCase
    {
        private BeatmapCarousel carousel;

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
            Add(carousel = new BeatmapCarousel
            {
                RelativeSizeAxes = Axes.Both,
            });

            AddStep("Load Beatmaps", () =>
            {
                carousel.Beatmaps = new[]
                {
                    createTestBeatmapSet(0),
                    createTestBeatmapSet(1),
                    createTestBeatmapSet(2),
                    createTestBeatmapSet(3),
                };
            });

            AddUntilStep(() => carousel.Beatmaps.Any(), "Wait for load");

            AddStep("SelectNext set", () => carousel.SelectNext());
            AddAssert("set1 diff1", () => carousel.SelectedBeatmap == carousel.Beatmaps.First().Beatmaps.First());

            AddStep("SelectNext diff", () => carousel.SelectNext(1, false));
            AddAssert("set1 diff2", () => carousel.SelectedBeatmap == carousel.Beatmaps.First().Beatmaps.Skip(1).First());

            AddStep("SelectNext backwards", () => carousel.SelectNext(-1));
            AddAssert("set4 diff1", () => carousel.SelectedBeatmap == carousel.Beatmaps.Last().Beatmaps.First());

            AddStep("SelectNext diff backwards", () => carousel.SelectNext(-1, false));
            AddAssert("set3 diff3", () => carousel.SelectedBeatmap == carousel.Beatmaps.Reverse().Skip(1).First().Beatmaps.Last());

            AddStep("SelectNext", () => carousel.SelectNext());
            AddStep("SelectNext", () => carousel.SelectNext());
            AddAssert("set1 diff1", () => carousel.SelectedBeatmap == carousel.Beatmaps.First().Beatmaps.First());

            AddStep("SelectNext diff backwards", () => carousel.SelectNext(-1, false));
            AddAssert("set4 diff3", () => carousel.SelectedBeatmap == carousel.Beatmaps.Last().Beatmaps.Last());

            // AddStep("Clear beatmaps", () => carousel.Beatmaps = new BeatmapSetInfo[] { });
            // AddStep("SelectNext (noop)", () => carousel.SelectNext());
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
                    Artist = "MONACA " + RNG.Next(0, 9),
                    Title = "Black Song " + RNG.Next(0, 9),
                    AuthorString = "Some Guy " + RNG.Next(0, 9),
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
    }
}
