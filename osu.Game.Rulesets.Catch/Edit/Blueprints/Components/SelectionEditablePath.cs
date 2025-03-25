// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Catch.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class SelectionEditablePath : EditablePath, IHasContextMenu
    {
        public MenuItem[] ContextMenuItems => getContextMenuItems().ToArray();

        private readonly JuiceStream juiceStream;

        // To handle when the editor is scrolled while dragging.
        private Vector2 dragStartPosition;

        public SelectionEditablePath(JuiceStream juiceStream, Func<float, double> positionToTime)
            : base(positionToTime)
        {
            this.juiceStream = juiceStream;
        }

        public void AddVertex(Vector2 relativePosition)
        {
            EditorBeatmap?.BeginChange();

            double time = Math.Max(0, PositionToTime(relativePosition.Y));
            int index = AddVertex(time, relativePosition.X);
            UpdateHitObjectFromPath(juiceStream);
            selectOnly(index);

            EditorBeatmap?.EndChange();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            int index = getMouseTargetVertex(e.ScreenSpaceMouseDownPosition);
            if (index == -1 || VertexStates[index].IsFixed)
                return false;

            if (e.Button == MouseButton.Right && e.ShiftPressed)
            {
                EditorBeatmap?.BeginChange();
                RemoveVertex(index);
                UpdateHitObjectFromPath(juiceStream);
                EditorBeatmap?.EndChange();

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

            EditorBeatmap?.BeginChange();
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
            EditorBeatmap?.EndChange();
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
            EditorBeatmap?.BeginChange();

            for (int i = VertexCount - 1; i >= 0; i--)
            {
                if (VertexStates[i].IsSelected)
                    RemoveVertex(i);
            }

            UpdateHitObjectFromPath(juiceStream);

            EditorBeatmap?.EndChange();
        }

        public override void UpdateHitObjectFromPath(JuiceStream hitObject)
        {
            base.UpdateHitObjectFromPath(hitObject);

            if (hitObject.Path.ControlPoints.Count <= 1 || !hitObject.Path.HasValidLengthForPlacement)
                EditorBeatmap?.Remove(hitObject);
        }
    }
}
