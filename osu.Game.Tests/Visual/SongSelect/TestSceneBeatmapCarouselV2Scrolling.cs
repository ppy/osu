// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Game.Screens.Select;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2Scrolling : BeatmapCarouselV2TestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
            SortBy(new FilterCriteria());

            AddBeatmaps(10);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestScrollPositionMaintainedOnAddSecondSelected()
        {
            Quad positionBefore = default;

            AddStep("select middle beatmap", () => Carousel.CurrentSelection = BeatmapSets.ElementAt(BeatmapSets.Count - 2).Beatmaps.First());
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

            AddStep("scroll to last item", () => Scroll.ScrollToEnd(false));

            AddStep("select last beatmap", () => Carousel.CurrentSelection = BeatmapSets.Last().Beatmaps.Last());

            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<BeatmapPanel>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            RemoveFirstBeatmap();
            WaitForSorting();
            AddAssert("select screen position unchanged", () => Carousel.ChildrenOfType<BeatmapPanel>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }
    }
}
