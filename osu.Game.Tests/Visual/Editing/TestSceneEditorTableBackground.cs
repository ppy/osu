// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorTableBackground : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private EditorTableBackground background = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = background = new EditorTableBackground
            {
                RelativeSizeAxes = Axes.X
            };
        });

        [Test]
        public void TestHeight()
        {
            AddStep("Set item count to 10", () => setItemCount(10));
            AddAssert("Height is correct", () => background.Height == 10 * EditorTableBackground.ROW_HEIGHT);
            AddStep("Set item count to 0", () => setItemCount(0));
            AddAssert("Height is correct", () => background.Height == 0);
        }

        [Test]
        public void TestHover()
        {
            AddStep("Set item count to 10", () => setItemCount(10));
            AddStep("Move to 1st item", () => InputManager.MoveMouseTo(getMousePositionFor(0)));
            AddAssert("item hovered", () => getStateFor(0) == RowState.Hovered);
            waitForTotalChildrenCount(1);
            AddStep("Move to 2nd item", () => InputManager.MoveMouseTo(getMousePositionFor(1)));
            AddAssert("item hovered", () => getStateFor(1) == RowState.Hovered);
            AddAssert("1st lost hover", () => getStateFor(0) == RowState.None);
            waitForTotalChildrenCount(1);
            AddStep("Move outside", () => InputManager.MoveMouseTo(getMousePositionFor(10)));
            AddAssert("none is hovered", noneIsHovered);
            waitForTotalChildrenCount(0);
        }

        [Test]
        public void TestManualSelection()
        {
            AddStep("Set item count to 10", () => setItemCount(10));
            AddStep("Move to 1st item", () => InputManager.MoveMouseTo(getMousePositionFor(0)));
            AddAssert("item hovered", () => getStateFor(0) == RowState.Hovered);
            AddStep("Click", () => InputManager.Click(MouseButton.Left));
            AddAssert("item selected", () => getStateFor(0) == RowState.Selected);
            waitForTotalChildrenCount(1);
            AddStep("Move to 2nd item", () => InputManager.MoveMouseTo(getMousePositionFor(1)));
            AddAssert("item hovered", () => getStateFor(1) == RowState.Hovered);
            AddAssert("1st selected", () => getStateFor(0) == RowState.Selected);
            waitForTotalChildrenCount(2);
            AddStep("Click", () => InputManager.Click(MouseButton.Left));
            AddAssert("2nd selected", () => getStateFor(1) == RowState.Selected);
            AddAssert("1st deselected", () => getStateFor(0) == RowState.None);
            waitForTotalChildrenCount(1);
            AddStep("Move outside", () => InputManager.MoveMouseTo(getMousePositionFor(10)));
            AddAssert("none is hovered", noneIsHovered);
            AddAssert("2nd selected", () => getStateFor(1) == RowState.Selected);
            waitForTotalChildrenCount(1);
        }

        [Test]
        public void TestAutoSelection()
        {
            AddStep("Set item count to 10", () => setItemCount(10));
            AddStep("Move to 1st item", () => InputManager.MoveMouseTo(getMousePositionFor(0)));
            AddAssert("item hovered", () => getStateFor(0) == RowState.Hovered);
            AddStep("select hovered item", () => background.Select(0));
            AddAssert("item selected", () => getStateFor(0) == RowState.Selected);
            waitForTotalChildrenCount(1);
            AddStep("select next item", () => background.Select(1));
            AddAssert("next selected", () => getStateFor(1) == RowState.Selected);
            AddAssert("current hovered", () => getStateFor(0) == RowState.Hovered);
            waitForTotalChildrenCount(2);
            AddStep("select hovered item", () => background.Select(0));
            AddAssert("item selected", () => getStateFor(0) == RowState.Selected);
            AddAssert("next deselected", () => getStateFor(1) == RowState.None);
            waitForTotalChildrenCount(1);
        }

        [Test]
        public void TestDeselect()
        {
            AddStep("Set item count to 10", () => setItemCount(10));
            AddStep("Move to 1st item", () => InputManager.MoveMouseTo(getMousePositionFor(0)));
            AddStep("Click", () => InputManager.Click(MouseButton.Left));
            AddAssert("item selected", () => getStateFor(0) == RowState.Selected);
            AddStep("Deselect", () => background.Deselect());
            AddAssert("item hovered", () => getStateFor(0) == RowState.Hovered);
            waitForTotalChildrenCount(1);

            AddStep("Move to 1st item", () => InputManager.MoveMouseTo(getMousePositionFor(0)));
            AddStep("Click", () => InputManager.Click(MouseButton.Left));
            AddAssert("item selected", () => getStateFor(0) == RowState.Selected);
            AddStep("Move to 2nd item", () => InputManager.MoveMouseTo(getMousePositionFor(1)));
            AddAssert("2nd hovered", () => getStateFor(1) == RowState.Hovered);
            AddAssert("1st selected", () => getStateFor(0) == RowState.Selected);
            waitForTotalChildrenCount(2);
            AddStep("Deselect", () => background.Deselect());
            AddAssert("2nd hovered", () => getStateFor(1) == RowState.Hovered);
            waitForTotalChildrenCount(1);
        }

        private void waitForTotalChildrenCount(int count) => AddUntilStep($"Wait for children count {count}", () => background.Count == count);

        private void setItemCount(int count) => background.RowCount = count;

        private bool noneIsHovered()
        {
            bool anyHovered = false;

            foreach (var row in background)
                anyHovered |= row.State == RowState.Hovered;

            return !anyHovered;
        }

        private RowState getStateFor(int index)
        {
            foreach (var row in background)
            {
                if (row.Index == index)
                    return row.State;
            }

            return RowState.None;
        }

        private Vector2 getMousePositionFor(int index) => background.ToScreenSpace(new Vector2(100, index * EditorTableBackground.ROW_HEIGHT + EditorTableBackground.ROW_HEIGHT * 0.5f));
    }
}
