// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelect
{
    /// <summary>
    /// Covers common steps which can be used for manual testing.
    /// </summary>
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2 : BeatmapCarouselV2TestScene
    {
        [Test]
        [Explicit]
        public void TestBasics()
        {
            CreateCarousel();
            RemoveAllBeatmaps();

            AddBeatmaps(10, randomMetadata: true);
            AddBeatmaps(10);
            AddBeatmaps(1);
        }

        [Test]
        [Explicit]
        public void TestSorting()
        {
            SortBy(new FilterCriteria { Sort = SortMode.Artist });
            SortBy(new FilterCriteria { Group = GroupMode.Difficulty, Sort = SortMode.Difficulty });
            SortBy(new FilterCriteria { Group = GroupMode.Artist, Sort = SortMode.Artist });
        }

        [Test]
        [Explicit]
        public void TestRemovals()
        {
            RemoveFirstBeatmap();
            RemoveAllBeatmaps();
        }

        [Test]
        [Explicit]
        public void TestAddRemoveRepeatedOps()
        {
            AddRepeatStep("add beatmaps", () => BeatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4))), 20);
            AddRepeatStep("remove beatmaps", () => BeatmapSets.RemoveAt(RNG.Next(0, BeatmapSets.Count)), 20);
        }

        [Test]
        [Explicit]
        public void TestMasking()
        {
            AddStep("disable masking", () => Scroll.Masking = false);
            AddStep("enable masking", () => Scroll.Masking = true);
        }

        [Test]
        [Explicit]
        public void TestPerformanceWithManyBeatmaps()
        {
            const int count = 200000;

            List<BeatmapSetInfo> generated = new List<BeatmapSetInfo>();

            AddStep($"populate {count} test beatmaps", () =>
            {
                generated.Clear();
                Task.Run(() =>
                {
                    for (int j = 0; j < count; j++)
                        generated.Add(CreateTestBeatmapSetInfo(3, true));
                }).ConfigureAwait(true);
            });

            AddUntilStep("wait for beatmaps populated", () => generated.Count, () => Is.GreaterThan(count / 3));
            AddUntilStep("this takes a while", () => generated.Count, () => Is.GreaterThan(count / 3 * 2));
            AddUntilStep("maybe they are done now", () => generated.Count, () => Is.EqualTo(count));

            AddStep("add all beatmaps", () => BeatmapSets.AddRange(generated));
        }
    }
}
