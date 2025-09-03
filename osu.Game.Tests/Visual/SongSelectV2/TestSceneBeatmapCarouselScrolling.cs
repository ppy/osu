// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselScrolling : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            AddBeatmaps(10);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestScrollPositionMaintainedOnRemove_SecondSelected()
        {
            Quad positionBefore = default;

            AddStep("select middle beatmap", () => Carousel.CurrentGroupedBeatmap = new GroupedBeatmap(null, BeatmapSets.ElementAt(BeatmapSets.Count - 2).Beatmaps.First()));

            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<PanelBeatmap>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            RemoveFirstBeatmap();
            WaitForFiltering();

            AddAssert("select screen position unchanged", () => Carousel.ChildrenOfType<PanelBeatmap>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestScrollPositionMaintainedOnRemove_SecondSelected_WithUserScroll()
        {
            Quad positionBefore = default;

            AddStep("select middle beatmap", () => Carousel.CurrentGroupedBeatmap = new GroupedBeatmap(null, BeatmapSets.ElementAt(BeatmapSets.Count - 2).Beatmaps.First()));
            WaitForScrolling();

            AddStep("override scroll with user scroll", () =>
            {
                InputManager.MoveMouseTo(Scroll.ScreenSpaceDrawQuad.Centre);
                InputManager.ScrollVerticalBy(-1);
            });
            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<PanelBeatmap>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            RemoveFirstBeatmap();
            WaitForFiltering();

            AddAssert("select screen position unchanged", () => Carousel.ChildrenOfType<PanelBeatmap>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestScrollPositionMaintainedOnRemove_LastSelected()
        {
            Quad positionBefore = default;

            AddStep("scroll to end", () => Scroll.ScrollToEnd(false));

            AddStep("select last beatmap", () => Carousel.CurrentGroupedBeatmap = new GroupedBeatmap(null, BeatmapSets.Last().Beatmaps.Last()));

            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<PanelBeatmap>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            RemoveFirstBeatmap();
            WaitForFiltering();
            AddAssert("select screen position unchanged", () => Carousel.ChildrenOfType<PanelBeatmap>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestScrollToSelectionAfterFilter()
        {
            Quad positionBefore = default;

            AddStep("select first beatmap", () => Carousel.CurrentGroupedBeatmap = new GroupedBeatmap(null, BeatmapSets.First().Beatmaps.First()));

            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<PanelBeatmap>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            AddStep("scroll to end", () => Scroll.ScrollToEnd());
            WaitForScrolling();

            ApplyToFilterAndWaitForFilter("search", f => f.SearchText = "Some");

            AddUntilStep("select screen position returned to selection", () => Carousel.ChildrenOfType<PanelBeatmap>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestScrollToSelectionAfterFilter_WithUserScroll()
        {
            Quad positionBefore = default;

            AddStep("select first beatmap", () => Carousel.CurrentGroupedBeatmap = new GroupedBeatmap(null, BeatmapSets.First().Beatmaps.First()));
            WaitForScrolling();

            AddStep("override scroll with user scroll", () =>
            {
                InputManager.MoveMouseTo(Scroll.ScreenSpaceDrawQuad.Centre);
                InputManager.ScrollVerticalBy(-1);
            });
            WaitForScrolling();

            AddStep("save selected screen position", () => positionBefore = Carousel.ChildrenOfType<PanelBeatmap>().FirstOrDefault(p => p.Selected.Value)!.ScreenSpaceDrawQuad);

            ApplyToFilterAndWaitForFilter("search", f => f.SearchText = "Some");

            AddUntilStep("select screen position returned to selection", () => Carousel.ChildrenOfType<PanelBeatmap>().Single(p => p.Selected.Value).ScreenSpaceDrawQuad,
                () => Is.EqualTo(positionBefore));
        }
    }
}
