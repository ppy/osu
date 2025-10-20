// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    /// <summary>
    /// Covers common steps which can be used for manual testing.
    /// </summary>
    [TestFixture]
    public partial class TestSceneBeatmapCarousel : BeatmapCarouselTestScene
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
            SortAndGroupBy(SortMode.Artist, GroupMode.None);
            SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);
            SortAndGroupBy(SortMode.Artist, GroupMode.Artist);
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
        public void TestLoadingDisplay()
        {
            AddStep("induce slow filtering", () => Carousel.FilterDelay = 2000);
            SortAndGroupBy(SortMode.Artist, GroupMode.None);
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
        public void TestRandomStatus()
        {
            SortBy(SortMode.Title);
            AddStep("add beatmaps", () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo();
                    set.Status = Enum.GetValues<BeatmapOnlineStatus>().MinBy(_ => RNG.Next());

                    if (i % 2 == 0)
                        set.Status = BeatmapOnlineStatus.None;

                    BeatmapSets.Add(set);
                }
            });
        }

        [Test]
        public void TestHighChurnUpdatesStillShowsPanels()
        {
            ScheduledDelegate updateTask = null!;

            AddBeatmaps(1, 1);

            AddStep("start constantly updating beatmap in background", () =>
            {
                updateTask = Scheduler.AddDelayed(() => { BeatmapSets.ReplaceRange(0, 1, [BeatmapSets.First()]); }, 1, true);
            });

            CreateCarousel();

            AddUntilStep("panels loaded", () => Carousel.ChildrenOfType<Panel>(), () => Is.Not.Empty);

            AddStep("end task", () => updateTask.Cancel());
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

        [Test]
        public void TestSingleItemDisplayed()
        {
            CreateCarousel();
            RemoveAllBeatmaps();

            SortAndGroupBy(SortMode.Difficulty, GroupMode.None);
            AddBeatmaps(1, fixedDifficultiesPerSet: 1);
            AddUntilStep("single item is shown", () => this.ChildrenOfType<PanelBeatmapStandalone>().Count(), () => Is.EqualTo(1));
        }
    }
}
