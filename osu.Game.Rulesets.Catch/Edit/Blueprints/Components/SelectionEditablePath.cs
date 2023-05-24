// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class SelectionEditablePath : EditablePath, IHasContextMenu
    {
        public MenuItem[] ContextMenuItems => getContextMenuItems().ToArray();

        // To handle when the editor is scrolled while dragging.
        private Vector2 dragStartPosition;

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        public SelectionEditablePath(Func<float, double> positionToTime)
            : base(positionToTime)
        {
        }

        public void AddVertex(Vector2 relativePosition)
        {
            double time = Math.Max(0, PositionToTime(relativePosition.Y));
            int index = AddVertex(time, relativePosition.X);
            selectOnly(index);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            int index = getMouseTargetVertex(e.ScreenSpaceMouseDownPosition);
            if (index == -1 || VertexStates[index].IsFixed)
                return false;

            if (e.Button == MouseButton.Left && e.ShiftPressed)
            {
                RemoveVertex(index);
                return true;
            }

            if (e.ControlPressed)
                VertexStates[index].IsSelected = !VertexStates[index].IsSelected;
            else if (!VertexStates[index].IsSelected)
                selectOnly(index);

            // Don't inhibit right click, to show the context menu
            return e.Button != MouseButton.Right;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            int index = getMouseTargetVertex(e.ScreenSpaceMouseDownPosition);
            if (index == -1 || VertexStates[index].IsFixed)
                return false;

            if (e.Button != MouseButton.Left)
                return false;

            dragStartPosition = ToRelativePosition(e.ScreenSpaceMouseDownPosition);

            for (int i = 0; i < VertexCount; i++)
                VertexStates[i].VertexBeforeChange = Vertices[i];

            changeHandler?.BeginChange();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            Vector2 mousePosition = ToRelativePosition(e.ScreenSpaceMousePosition);
            double timeDelta = PositionToTime(mousePosition.Y) - PositionToTime(dragStartPosition.Y);
            float xDelta = mousePosition.X - dragStartPosition.X;
            MoveSelectedVertices(timeDelta, xDelta);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            changeHandler?.EndChange();
        }

        private int getMouseTargetVertex(Vector2 screenSpacePosition)
        {
            for (int i = InternalChildren.Count - 1; i >= 0; i--)
            {
                if (i < VertexCount && InternalChildren[i].ReceivePositionalInputAt(screenSpacePosition))
                    return i;
            }

            return -1;
        }

        private IEnumerable<MenuItem> getContextMenuItems()
        {
            int selectedCount = VertexStates.Count(state => state.IsSelected);

            if (selectedCount != 0)
                yield return new OsuMenuItem($"Delete selected {(selectedCount == 1 ? "vertex" : $"{selectedCount} vertices")}", MenuItemType.Destructive, deleteSelectedVertices);
        }

        private void selectOnly(int index)
        {
            for (int i = 0; i < VertexCount; i++)
                VertexStates[i].IsSelected = i == index;
        }

        private void deleteSelectedVertices()
        {
            for (int i = VertexCount - 1; i >= 0; i--)
            {
                if (VertexStates[i].IsSelected)
                    RemoveVertex(i);
            }
        }
    }
}
