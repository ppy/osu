// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelect
{
    /// <summary>
    /// Currently covers adding and removing of items and scrolling.
    /// If we add more tests here, these two categories can likely be split out into separate scenes.
    /// </summary>
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2Basics : BeatmapCarouselV2TestScene
    {
        [Test]
        public void TestBasics()
        {
            AddBeatmaps(1);
            AddBeatmaps(10);
            RemoveFirstBeatmap();
            RemoveAllBeatmaps();
        }

        [Test]
        public void TestOffScreenLoading()
        {
            AddStep("disable masking", () => Scroll.Masking = false);
            AddStep("enable masking", () => Scroll.Masking = true);
        }

        [Test]
        public void TestAddRemoveOneByOne()
        {
            AddRepeatStep("add beatmaps", () => BeatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4))), 20);
            AddRepeatStep("remove beatmaps", () => BeatmapSets.RemoveAt(RNG.Next(0, BeatmapSets.Count)), 20);
        }

        [Test]
        public void TestSorting()
        {
            AddBeatmaps(10);
            SortBy(new FilterCriteria { Group = GroupMode.Difficulty, Sort = SortMode.Difficulty });
            SortBy(new FilterCriteria { Group = GroupMode.Artist, Sort = SortMode.Artist });
            SortBy(new FilterCriteria { Sort = SortMode.Artist });
        }

        [Test]
        public void TestScrollPositionMaintainedOnAddSecondSelected()
        {
            Quad positionBefore = default;

            AddBeatmaps(10);
            WaitForDrawablePanels();

            AddStep("select middle beatmap", () => Carousel.CurrentSelection = BeatmapSets.ElementAt(BeatmapSets.Count - 2));
            AddStep("scroll to selected item", () => Scroll.ScrollTo(Scroll.ChildrenOfType<BeatmapPanel>().Single(p => p.Selected.Value)));

            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<BeatmapPanel>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            RemoveFirstBeatmap();
            WaitForSorting();

            AddAssert("select screen position unchanged", () => Carousel.ChildrenOfType<BeatmapPanel>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestScrollPositionMaintainedOnAddLastSelected()
        {
            Quad positionBefore = default;

            AddBeatmaps(10);
            WaitForDrawablePanels();

            AddStep("scroll to last item", () => Scroll.ScrollToEnd(false));

            AddStep("select last beatmap", () => Carousel.CurrentSelection = BeatmapSets.Last());

            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<BeatmapPanel>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            RemoveFirstBeatmap();
            WaitForSorting();
            AddAssert("select screen position unchanged", () => Carousel.ChildrenOfType<BeatmapPanel>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
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
                        generated.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4)));
                }).ConfigureAwait(true);
            });

            AddUntilStep("wait for beatmaps populated", () => generated.Count, () => Is.GreaterThan(count / 3));
            AddUntilStep("this takes a while", () => generated.Count, () => Is.GreaterThan(count / 3 * 2));
            AddUntilStep("maybe they are done now", () => generated.Count, () => Is.EqualTo(count));

            AddStep("add all beatmaps", () => BeatmapSets.AddRange(generated));
        }
    }
}
