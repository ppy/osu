// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselDifficultyGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            SortBy(SortMode.Difficulty);
            GroupBy(GroupMode.Difficulty);

            AddBeatmaps(10, 3);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestOpenCloseGroupMouse()
        {
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            CheckHasSelection();

            ClickVisiblePanel<PanelGroup>(0);

            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckHasSelection();
        }

        [Test]
        public void TestOpenCloseGroupKeyboard()
        {
            SelectPrevPanel();

            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddAssert("keyboard selected is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckHasSelection();

            Select();

            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckHasSelection();
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            Guid selectedID = Guid.Empty;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectNextGroup();

            AddStep("record selection", () => selectedID = ((BeatmapInfo)Carousel.CurrentSelection!).ID);

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
                ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
                AddAssert("selection not changed", () => ((BeatmapInfo)Carousel.CurrentSelection!).ID == selectedID);
            }
        }

        [Test]
        public void TestGroupSelectionOnHeaderKeyboard()
        {
            WaitForGroupSelection(0, 0);

            SelectPrevPanel();
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);

            SelectPrevGroup();

            WaitForGroupSelection(0, 0);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);

            SelectPrevGroup();

            WaitForGroupSelection(0, 0);
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
        }

        [Test]
        public void TestGroupSelectionOnHeaderMouse()
        {
            WaitForGroupSelection(0, 0);

            AddAssert("keyboard selected panel is beatmap", GetKeyboardSelectedPanel, Is.TypeOf<PanelBeatmap>);
            AddAssert("selected panel is beatmap", GetSelectedPanel, Is.TypeOf<PanelBeatmap>);

            ClickVisiblePanel<PanelGroup>(0);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, Is.TypeOf<PanelGroup>);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);

            ClickVisiblePanel<PanelGroup>(0);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, Is.TypeOf<PanelGroup>);
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);

            AddAssert("selected panel is still beatmap", GetSelectedPanel, Is.TypeOf<PanelBeatmap>);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            WaitForGroupSelection(0, 0);
            SelectPrevPanel();
            Select();

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            WaitForGroupSelection(0, 0);

            // open first group
            Select();
            WaitForGroupSelection(0, 0);
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

            SelectNextPanel();
            Select();
            WaitForGroupSelection(0, 0);

            SelectNextGroup();
            WaitForGroupSelection(0, 1);

            SelectNextGroup();
            WaitForGroupSelection(0, 2);

            SelectPrevGroup();
            WaitForGroupSelection(0, 1);

            SelectPrevGroup();
            WaitForGroupSelection(0, 0);

            SelectPrevGroup();
            WaitForGroupSelection(2, 9);
        }

        [Test]
        public void TestInputHandlingWithinGaps()
        {
            SelectNextPanel();
            Select();
            WaitForGroupSelection(0, 1);

            SelectPrevPanel();
            SelectPrevPanel();
            Select();

            // Clicks just above the first group panel should not actuate any action.
            ClickVisiblePanelWithOffset<PanelGroup>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2 + 1)));
            AddAssert("group not expanded", () => GetVisiblePanels<PanelGroup>().First().Item!.IsExpanded, () => Is.False);

            // minus one to avoid floating point inaccuracies when clicking at the direct edge of the panel.
            ClickVisiblePanelWithOffset<PanelGroup>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2 - 1)));

            AddAssert("group expanded", () => GetVisiblePanels<PanelGroup>().First().Item!.IsExpanded, () => Is.True);
            WaitForGroupSelection(0, 1);

            // Beatmap panels expand their selection area to cover holes from spacing.
            ClickVisiblePanelWithOffset<PanelBeatmap>(0, new Vector2(0, -(CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForGroupSelection(0, 0);

            ClickVisiblePanelWithOffset<PanelBeatmap>(1, new Vector2(0, (CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForGroupSelection(0, 1);
        }
    }
}
