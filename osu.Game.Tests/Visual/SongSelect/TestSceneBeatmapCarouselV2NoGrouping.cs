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
    public partial class TestSceneBeatmapCarouselV2NoGrouping : BeatmapCarouselV2TestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
            SortBy(new FilterCriteria { Sort = SortMode.Title });
        }

        /// <summary>
        /// Keyboard selection via up and down arrows doesn't actually change the selection until
        /// the select key is pressed.
        /// </summary>
        [Test]
        public void TestKeyboardSelectionKeyRepeat()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();
            CheckNoSelection();

            Select();
            CheckNoSelection();

            AddStep("press down arrow", () => InputManager.PressKey(Key.Down));
            checkSelectionIterating(false);

            AddStep("press up arrow", () => InputManager.PressKey(Key.Up));
            checkSelectionIterating(false);

            AddStep("release down arrow", () => InputManager.ReleaseKey(Key.Down));
            checkSelectionIterating(false);

            AddStep("release up arrow", () => InputManager.ReleaseKey(Key.Up));
            checkSelectionIterating(false);

            Select();
            CheckHasSelection();
        }

        /// <summary>
        /// Keyboard selection via left and right arrows moves between groups, updating the selection
        /// immediately.
        /// </summary>
        [Test]
        public void TestGroupSelectionKeyRepeat()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();
            CheckNoSelection();

            AddStep("press right arrow", () => InputManager.PressKey(Key.Right));
            checkSelectionIterating(true);

            AddStep("press left arrow", () => InputManager.PressKey(Key.Left));
            checkSelectionIterating(true);

            AddStep("release right arrow", () => InputManager.ReleaseKey(Key.Right));
            checkSelectionIterating(true);

            AddStep("release left arrow", () => InputManager.ReleaseKey(Key.Left));
            checkSelectionIterating(false);
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();

            SelectNextGroup();

            object? selection = null;

            AddStep("store drawable selection", () => selection = GetSelectedPanel()?.Item?.Model);

            CheckHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddBeatmaps(10);
            WaitForDrawablePanels();

            CheckHasSelection();
            AddAssert("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((BeatmapInfo)selection!).BeatmapSet!));

            AddAssert("selection matches original carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));
            AddUntilStep("drawable selection restored", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);
        }

        [Test]
        public void TestTraversalBeyondStart()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            SelectNextGroup();
            WaitForSelection(0, 0);
            SelectPrevGroup();
            WaitForSelection(total_set_count - 1, 0);
        }

        [Test]
        public void TestTraversalBeyondEnd()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            SelectPrevGroup();
            WaitForSelection(total_set_count - 1, 0);
            SelectNextGroup();
            WaitForSelection(0, 0);
        }

        [Test]
        public void TestGroupSelectionOnHeader()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextGroup();
            SelectNextGroup();
            WaitForSelection(1, 0);

            SelectPrevPanel();
            SelectPrevGroup();
            WaitForSelection(1, 0);

            SelectPrevPanel();
            SelectNextGroup();
            WaitForSelection(1, 0);
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

            Select();
            WaitForSelection(3, 0);

            SelectNextPanel();
            WaitForSelection(3, 0);

            Select();
            WaitForSelection(3, 1);

            SelectNextPanel();
            WaitForSelection(3, 1);

            Select();
            WaitForSelection(3, 2);

            SelectNextPanel();
            WaitForSelection(3, 2);

            Select();
            WaitForSelection(4, 0);
        }

        [Test]
        public void TestEmptyTraversal()
        {
            SelectNextPanel();
            CheckNoSelection();

            SelectNextGroup();
            CheckNoSelection();

            SelectPrevPanel();
            CheckNoSelection();

            SelectPrevGroup();
            CheckNoSelection();
        }

        [Test]
        public void TestInputHandlingWithinGaps()
        {
            AddBeatmaps(2, 5);
            WaitForDrawablePanels();
            SelectNextGroup();

            clickOnDifficulty(0, 1, p => p.LayoutRectangle.TopLeft + new Vector2(20f, -1f));
            WaitForSelection(0, 1);

            clickOnDifficulty(0, 0, p => p.LayoutRectangle.BottomLeft + new Vector2(20f, 1f));
            WaitForSelection(0, 0);

            SelectNextPanel();
            Select();
            WaitForSelection(0, 1);

            clickOnSet(0, p => p.LayoutRectangle.BottomLeft + new Vector2(20f, 1f));
            WaitForSelection(0, 0);

            AddStep("scroll to end", () => Scroll.ScrollToEnd(false));
            clickOnDifficulty(0, 4, p => p.LayoutRectangle.BottomLeft + new Vector2(20f, 1f));
            WaitForSelection(0, 4);

            clickOnSet(1, p => p.LayoutRectangle.TopLeft + new Vector2(20f, -1f));
            WaitForSelection(1, 0);
        }

        private void checkSelectionIterating(bool isIterating)
        {
            object? selection = null;

            for (int i = 0; i < 3; i++)
            {
                AddStep("store selection", () => selection = Carousel.CurrentSelection);
                if (isIterating)
                    AddUntilStep("selection changed", () => Carousel.CurrentSelection != selection);
                else
                    AddUntilStep("selection not changed", () => Carousel.CurrentSelection == selection);
            }
        }

        private void clickOnSet(int set, Func<BeatmapSetPanel, Vector2> pos)
        {
            AddStep($"click on set{set}", () =>
            {
                var model = BeatmapSets[set];
                var panel = this.ChildrenOfType<BeatmapSetPanel>().Single(b => ReferenceEquals(b.Item!.Model, model));
                InputManager.MoveMouseTo(panel.ToScreenSpace(pos(panel)));
                InputManager.Click(MouseButton.Left);
            });
        }

        private void clickOnDifficulty(int set, int diff, Func<BeatmapPanel, Vector2> pos)
        {
            AddStep($"click on set{set} diff{diff}", () =>
            {
                var model = BeatmapSets[set].Beatmaps[diff];
                var panel = this.ChildrenOfType<BeatmapPanel>().Single(b => ReferenceEquals(b.Item!.Model, model));
                InputManager.MoveMouseTo(panel.ToScreenSpace(pos(panel)));
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
