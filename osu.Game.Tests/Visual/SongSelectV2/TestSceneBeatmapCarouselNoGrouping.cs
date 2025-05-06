// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.SelectV2;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselNoGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
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
            CheckHasSelection();

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
            CheckHasSelection();

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
            Guid selectedID = Guid.Empty;

            RemoveAllBeatmaps();
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
        public void TestTraversalBeyondStart()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            SelectNextGroup();
            WaitForSelection(1, 0);
            SelectPrevGroup();
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

            WaitForSelection(0, 0);

            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();

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
            Select();

            WaitForScrolling();

            // Clicks just above the first group panel should not actuate any action.
            ClickVisiblePanelWithOffset<PanelBeatmapSet>(0, new Vector2(0, -(PanelBeatmapSet.HEIGHT / 2 + 1)));
            WaitForSelection(1, 0);

            ClickVisiblePanelWithOffset<PanelBeatmapSet>(0, new Vector2(0, -(PanelBeatmapSet.HEIGHT / 2)));
            WaitForSelection(0, 0);

            // Beatmap panels expand their selection area to cover holes from spacing.
            ClickVisiblePanelWithOffset<PanelBeatmap>(1, new Vector2(0, -(CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForSelection(0, 0);

            // Panels with higher depth will handle clicks in the gutters for simplicity.
            ClickVisiblePanelWithOffset<PanelBeatmap>(2, new Vector2(0, (CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForSelection(0, 2);

            ClickVisiblePanelWithOffset<PanelBeatmap>(3, new Vector2(0, (CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForSelection(0, 3);
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
    }
}
