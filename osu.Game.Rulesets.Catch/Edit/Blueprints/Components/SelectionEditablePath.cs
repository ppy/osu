// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public class SelectionEditablePath : EditablePath
    {
        // To handle when the editor is scrolled while dragging.
        private Vector2 dragStartPosition;

        [Resolved(CanBeNull = true)]
        [CanBeNull]
        private IEditorChangeHandler changeHandler { get; set; }

        public SelectionEditablePath(Func<float, double> positionToDistance)
            : base(positionToDistance)
        {
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            int index = getMouseTargetVertex(e.ScreenSpaceMouseDownPosition);

            if (index == -1)
                return false;

            if (e.ControlPressed)
                VertexStates[index].IsSelected = !VertexStates[index].IsSelected;
            else if (!VertexStates[index].IsSelected)
                selectOnly(index);

            // Don't inhabit right click, to show the context menu
            return e.Button != MouseButton.Right;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button != MouseButton.Left || getMouseTargetVertex(e.ScreenSpaceMouseDownPosition) == -1) return false;

            dragStartPosition = ToRelativePosition(e.ScreenSpaceMouseDownPosition);

            for (int i = 0; i < VertexCount; i++)
                VertexStates[i].VertexBeforeChange = Vertices[i];

            changeHandler?.BeginChange();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            Vector2 mousePosition = ToLocalSpace(e.ScreenSpaceMousePosition) - new Vector2(0, DrawHeight);
            double distanceDelta = PositionToDistance(mousePosition.Y) - PositionToDistance(dragStartPosition.Y);
            float xDelta = mousePosition.X - dragStartPosition.X;
            MoveSelectedVertices(distanceDelta, xDelta);
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

        private void selectOnly(int index)
        {
            for (int i = 0; i < VertexCount; i++)
                VertexStates[i].IsSelected = i == index;
        }
    }
}
