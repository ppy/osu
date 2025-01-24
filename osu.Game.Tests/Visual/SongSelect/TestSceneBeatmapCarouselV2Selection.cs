// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.SelectV2;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2Selection : BeatmapCarouselV2TestScene
    {
        /// <summary>
        /// Keyboard selection via up and down arrows doesn't actually change the selection until
        /// the select key is pressed.
        /// </summary>
        [Test]
        public void TestKeyboardSelectionKeyRepeat()
        {
            AddBeatmaps(10);
            WaitForDrawablePanels();
            checkNoSelection();

            select();
            checkNoSelection();

            AddStep("press down arrow", () => InputManager.PressKey(Key.Down));
            checkSelectionIterating(false);

            AddStep("press up arrow", () => InputManager.PressKey(Key.Up));
            checkSelectionIterating(false);

            AddStep("release down arrow", () => InputManager.ReleaseKey(Key.Down));
            checkSelectionIterating(false);

            AddStep("release up arrow", () => InputManager.ReleaseKey(Key.Up));
            checkSelectionIterating(false);

            select();
            checkHasSelection();
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
            checkNoSelection();

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

            selectNextGroup();

            object? selection = null;

            AddStep("store drawable selection", () => selection = getSelectedPanel()?.Item?.Model);

            checkHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", getSelectedPanel, () => Is.Null);

            AddBeatmaps(10);
            WaitForDrawablePanels();

            checkHasSelection();
            AddAssert("no drawable selection", getSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((BeatmapInfo)selection!).BeatmapSet!));

            AddUntilStep("drawable selection restored", () => getSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            BeatmapCarouselPanel? getSelectedPanel() => Carousel.ChildrenOfType<BeatmapCarouselPanel>().SingleOrDefault(p => p.Selected.Value);
        }

        [Test]
        public void TestTraversalBeyondStart()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            selectNextGroup();
            waitForSelection(0, 0);
            selectPrevGroup();
            waitForSelection(total_set_count - 1, 0);
        }

        [Test]
        public void TestTraversalBeyondEnd()
        {
            const int total_set_count = 200;

            AddBeatmaps(total_set_count);
            WaitForDrawablePanels();

            selectPrevGroup();
            waitForSelection(total_set_count - 1, 0);
            selectNextGroup();
            waitForSelection(0, 0);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            selectNextPanel();
            selectNextPanel();
            selectNextPanel();
            selectNextPanel();
            checkNoSelection();

            select();
            waitForSelection(3, 0);

            selectNextPanel();
            waitForSelection(3, 0);

            select();
            waitForSelection(3, 1);

            selectNextPanel();
            waitForSelection(3, 1);

            select();
            waitForSelection(3, 2);

            selectNextPanel();
            waitForSelection(3, 2);

            select();
            waitForSelection(4, 0);
        }

        [Test]
        public void TestEmptyTraversal()
        {
            selectNextPanel();
            checkNoSelection();

            selectNextGroup();
            checkNoSelection();

            selectPrevPanel();
            checkNoSelection();

            selectPrevGroup();
            checkNoSelection();
        }

        private void waitForSelection(int set, int? diff = null)
        {
            AddUntilStep($"selected is set{set}{(diff.HasValue ? $" diff{diff.Value}" : "")}", () =>
            {
                if (diff != null)
                    return ReferenceEquals(Carousel.CurrentSelection, BeatmapSets[set].Beatmaps[diff.Value]);

                return BeatmapSets[set].Beatmaps.Contains(Carousel.CurrentSelection);
            });
        }

        private void selectNextPanel() => AddStep("select next panel", () => InputManager.Key(Key.Down));
        private void selectPrevPanel() => AddStep("select prev panel", () => InputManager.Key(Key.Up));
        private void selectNextGroup() => AddStep("select next group", () => InputManager.Key(Key.Right));
        private void selectPrevGroup() => AddStep("select prev group", () => InputManager.Key(Key.Left));

        private void select() => AddStep("select", () => InputManager.Key(Key.Enter));

        private void checkNoSelection() => AddAssert("has no selection", () => Carousel.CurrentSelection, () => Is.Null);
        private void checkHasSelection() => AddAssert("has selection", () => Carousel.CurrentSelection, () => Is.Not.Null);

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
