// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using System;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Graphics.Containers.Draggable
{
    // Modified version of <see cref="RearrangeableListItem{TModel}"/>.
    // Would be nice to move to Framework later.

    /// <summary>
    /// An item of a <see cref="DraggableItemContainer{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract partial class DraggableItem<TModel> : CompositeDrawable
    {
        /// <summary>
        /// Invoked on drag start, if an arrangement should be started.
        /// </summary>
        internal Action<DraggableItem<TModel>, DragStartEvent> StartArrangement = null!;

        /// <summary>
        /// Invoked on drag, if this item is being arranged.
        /// </summary>
        internal Action<DraggableItem<TModel>, DragEvent> Arrange = null!;

        /// <summary>
        /// Invoked on drag end, if this item is being arranged.
        /// </summary>
        internal Action<DraggableItem<TModel>, DragEndEvent> EndArrangement = null!;

        /// <summary>
        /// The item this <see cref="DraggableItem{TModel}"/> represents.
        /// </summary>
        public readonly TModel Model;

        /// <summary>
        /// Creates a new <see cref="DraggableItem{TModel}"/>.
        /// </summary>
        /// <param name="item">The item to represent.</param>
        protected DraggableItem(TModel item)
        {
            Model = item;
            AlwaysPresent = true;
        }

        /// <summary>
        /// Called every frame while this item is being dragged.
        /// </summary>
        /// <param name="cursorPosition">The cursor position in screen space coordinates</param>
        internal void MoveToCursor(Vector2 cursorPosition) => Position = CursorInterpolation(cursorPosition);

        /// <summary>
        /// Given the position of the cursor, return the new position for this item for the current frame.
        /// </summary>
        /// <param name="cursorPosition">The cursor position in screen space coordinates</param>
        protected virtual Vector2 CursorInterpolation(Vector2 cursorPosition) => ToParentSpace(ToLocalSpace(cursorPosition));

        /// <summary>
        /// Whether the item is able to be dragged at the given screen-space position.
        /// </summary>
        protected virtual bool IsDraggableAt(Vector2 screenSpacePos) => true;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (IsDraggableAt(e.ScreenSpaceMouseDownPosition))
            {
                StartArrangement.Invoke(this, e);
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e) => Arrange.Invoke(this, e);

        protected override void OnDragEnd(DragEndEvent e) => EndArrangement.Invoke(this, e);
    }
}
