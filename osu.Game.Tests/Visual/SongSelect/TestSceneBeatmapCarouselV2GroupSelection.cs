// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2GroupSelection : BeatmapCarouselV2TestScene
    {
        public override void SetUpSteps()
        {
            RemoveAllBeatmaps();

            CreateCarousel();

            SortBy(new FilterCriteria { Group = GroupMode.Difficulty, Sort = SortMode.Difficulty });
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionMouse()
        {
            AddBeatmaps(10, 5);
            WaitForDrawablePanels();

            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<GroupPanel>(0);
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            CheckNoSelection();

            ClickVisiblePanel<GroupPanel>(0);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionKeyboard()
        {
            AddBeatmaps(10, 5);
            WaitForDrawablePanels();

            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            SelectNextPanel();
            Select();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddAssert("keyboard selected is expanded", () => getKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckNoSelection();

            Select();
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => getKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckNoSelection();

            GroupPanel? getKeyboardSelectedPanel() => Carousel.ChildrenOfType<GroupPanel>().SingleOrDefault(p => p.KeyboardSelected.Value);
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();

            SelectNextGroup();

            object? selection = null;

            AddStep("store drawable selection", () => selection = getSelectedPanel()?.Item?.Model);

            CheckHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", getSelectedPanel, () => Is.Null);

            AddBeatmaps(10);
            WaitForDrawablePanels();

            CheckHasSelection();
            AddAssert("no drawable selection", getSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((BeatmapInfo)selection!).BeatmapSet!));

            AddAssert("selection matches original carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));
            AddUntilStep("drawable selection restored", () => getSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("carousel item is visible", () => getSelectedPanel()?.Item?.IsVisible, () => Is.True);

            ClickVisiblePanel<GroupPanel>(0);
            AddUntilStep("carousel item not visible", getSelectedPanel, () => Is.Null);

            ClickVisiblePanel<GroupPanel>(0);
            AddUntilStep("carousel item is visible", () => getSelectedPanel()?.Item?.IsVisible, () => Is.True);

            BeatmapPanel? getSelectedPanel() => Carousel.ChildrenOfType<BeatmapPanel>().SingleOrDefault(p => p.Selected.Value);
        }

        [Test]
        public void TestGroupSelectionOnHeader()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextGroup();
            WaitForGroupSelection(0, 0);

            SelectPrevPanel();
            SelectPrevGroup();
            WaitForGroupSelection(2, 9);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            CheckNoSelection();

            // open first group
            Select();
            CheckNoSelection();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<BeatmapPanel>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

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
            AddBeatmaps(5, 2);
            WaitForDrawablePanels();
            SelectNextGroup();

            clickOnPanel(0, 1, p => p.LayoutRectangle.TopLeft + new Vector2(p.LayoutRectangle.Centre.X, -1f));
            WaitForGroupSelection(0, 1);

            clickOnPanel(0, 0, p => p.LayoutRectangle.BottomLeft + new Vector2(p.LayoutRectangle.Centre.X, 1f));
            WaitForGroupSelection(0, 0);

            SelectNextPanel();
            Select();
            WaitForGroupSelection(0, 1);

            clickOnGroup(0, p => p.LayoutRectangle.BottomLeft + new Vector2(p.LayoutRectangle.Centre.X, 1f));
            AddAssert("group 0 collapsed", () => this.ChildrenOfType<GroupPanel>().OrderBy(g => g.Y).ElementAt(0).Expanded.Value, () => Is.False);
            clickOnGroup(0, p => p.LayoutRectangle.Centre);
            AddAssert("group 0 expanded", () => this.ChildrenOfType<GroupPanel>().OrderBy(g => g.Y).ElementAt(0).Expanded.Value, () => Is.True);

            AddStep("scroll to end", () => Scroll.ScrollToEnd(false));
            clickOnPanel(0, 4, p => p.LayoutRectangle.BottomLeft + new Vector2(p.LayoutRectangle.Centre.X, 1f));
            WaitForGroupSelection(0, 4);

            clickOnGroup(1, p => p.LayoutRectangle.TopLeft + new Vector2(p.LayoutRectangle.Centre.X, -1f));
            AddAssert("group 1 expanded", () => this.ChildrenOfType<GroupPanel>().OrderBy(g => g.Y).ElementAt(1).Expanded.Value, () => Is.True);
        }

        private void clickOnGroup(int group, Func<GroupPanel, Vector2> pos)
        {
            AddStep($"click on group{group}", () =>
            {
                var groupingFilter = Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();
                var model = groupingFilter.GroupItems.Keys.ElementAt(group);

                var panel = this.ChildrenOfType<GroupPanel>().Single(b => ReferenceEquals(b.Item!.Model, model));
                InputManager.MoveMouseTo(panel.ToScreenSpace(pos(panel)));
                InputManager.Click(MouseButton.Left);
            });
        }

        private void clickOnPanel(int group, int panel, Func<BeatmapPanel, Vector2> pos)
        {
            AddStep($"click on group{group} panel{panel}", () =>
            {
                var groupingFilter = Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();

                var g = groupingFilter.GroupItems.Keys.ElementAt(group);
                // offset by one because the group itself is included in the items list.
                object model = groupingFilter.GroupItems[g].ElementAt(panel + 1).Model;

                var p = this.ChildrenOfType<BeatmapPanel>().Single(b => ReferenceEquals(b.Item!.Model, model));
                InputManager.MoveMouseTo(p.ToScreenSpace(pos(p)));
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
