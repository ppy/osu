// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            SortAndGroupBy(SortMode.Difficulty, GroupMode.Difficulty);

            AddBeatmaps(10, 3);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionMouse()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<PanelGroupStarDifficulty>(0);
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            CheckNoSelection();

            ClickVisiblePanel<PanelGroupStarDifficulty>(0);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionKeyboard()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            SelectNextPanel();
            Select();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddAssert("keyboard selected is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckNoSelection();

            Select();
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckNoSelection();
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            SelectNextGroup();

            object? selection = null;

            AddStep("store drawable selection", () => selection = GetSelectedPanel()?.Item?.Model);

            CheckHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddBeatmaps(3);
            WaitForDrawablePanels();

            CheckHasSelection();
            AddAssert("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((BeatmapInfo)selection!).BeatmapSet!));

            AddAssert("selection matches original carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));
            AddUntilStep("drawable selection restored", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);

            ClickVisiblePanel<PanelGroupStarDifficulty>(0);
            AddUntilStep("carousel item not visible", GetSelectedPanel, () => Is.Null);

            ClickVisiblePanel<PanelGroupStarDifficulty>(0);
            AddUntilStep("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);
        }

        [Test]
        public void TestGroupSelectionOnHeaderKeyboard()
        {
            SelectNextGroup();
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
            SelectNextGroup();
            WaitForGroupSelection(0, 0);

            AddAssert("keyboard selected panel is beatmap", GetKeyboardSelectedPanel, Is.TypeOf<PanelBeatmapStandalone>);
            AddAssert("selected panel is beatmap", GetSelectedPanel, Is.TypeOf<PanelBeatmapStandalone>);

            ClickVisiblePanel<PanelGroupStarDifficulty>(0);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, Is.TypeOf<PanelGroupStarDifficulty>);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);

            ClickVisiblePanel<PanelGroupStarDifficulty>(0);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, Is.TypeOf<PanelGroupStarDifficulty>);
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);

            AddAssert("selected panel is still beatmap", GetSelectedPanel, Is.TypeOf<PanelBeatmapStandalone>);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            CheckNoSelection();

            // open first group
            Select();
            CheckNoSelection();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmapStandalone>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

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
            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmapStandalone>().Any());

            // Clicks just above the first group panel should not actuate any action.
            ClickVisiblePanelWithOffset<PanelGroupStarDifficulty>(0, new Vector2(0, -(PanelGroupStarDifficulty.HEIGHT / 2 + 1)));

            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmapStandalone>().Any());

            // add lenience to avoid floating-point inaccuracies at edge.
            ClickVisiblePanelWithOffset<PanelGroupStarDifficulty>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2 - 1)));

            AddUntilStep("wait for beatmaps visible", () => GetVisiblePanels<PanelBeatmapStandalone>().Any());
            CheckNoSelection();

            // Beatmap panels expand their selection area to cover holes from spacing.
            ClickVisiblePanelWithOffset<PanelBeatmapStandalone>(0, new Vector2(0, -(CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForGroupSelection(0, 0);

            ClickVisiblePanelWithOffset<PanelBeatmapStandalone>(1, new Vector2(0, (CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForGroupSelection(0, 1);
        }

        [Test]
        public void TestBasicFiltering()
        {
            ApplyToFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);
            WaitForFiltering();

            CheckDisplayedGroupsCount(3);
            CheckDisplayedBeatmapsCount(3);

            // Single result gets selected automatically
            WaitForGroupSelection(0, 0);

            SelectNextPanel();
            Select();
            WaitForGroupSelection(0, 0);

            for (int i = 0; i < 5; i++)
                SelectNextPanel();

            Select();
            SelectNextPanel();
            Select();

            WaitForGroupSelection(1, 0);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
            WaitForFiltering();

            CheckDisplayedGroupsCount(3);
            CheckDisplayedBeatmapsCount(30);
        }

        [Test]
        public void TestExpandedGroupStillExpandedAfterFilter()
        {
            SelectPrevGroup();

            WaitForGroupSelection(2, 9);
            AddAssert("expanded group is last", () => (Carousel.ExpandedGroup as StarDifficultyGroupDefinition)?.Difficulty.Stars, () => Is.EqualTo(6));

            SelectNextPanel();
            Select();

            WaitForGroupSelection(2, 9);
            AddAssert("expanded group is first", () => (Carousel.ExpandedGroup as StarDifficultyGroupDefinition)?.Difficulty.Stars, () => Is.EqualTo(0));

            // doesn't actually filter anything away, but triggers a filter.
            ApplyToFilter("filter", c => c.SearchText = "Some");

            AddAssert("expanded group is still first", () => (Carousel.ExpandedGroup as StarDifficultyGroupDefinition)?.Difficulty.Stars, () => Is.EqualTo(0));
        }
    }
}
