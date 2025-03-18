// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osuTK;

namespace osu.Game.Graphics.Containers.Draggable
{
    /// <summary>
    /// Container for managing interactions between <see cref="DraggableItemContainer{TModel}"/>s.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    [Cached]
    public abstract partial class DraggableSharingContainer<TModel> : Container
        where TModel : notnull
    {
        /// <summary>
        /// Only updates when the user is dragging a <see cref="DraggableItem{TModel}"/>
        /// </summary>
        public readonly Bindable<Vector2> CursorPosition = new Bindable<Vector2>();

        /// <summary>
        /// True if the previous CurrentlySharedDraggedItem was shared with at least one other <see cref="DraggableItemContainer{TModel}"/>.
        /// </summary>
        internal bool WasShared;

        /// <summary>
        /// The <see cref="DraggableItem{TModel}"/> that is currently shared between the tree-descendant <see cref="DraggableItemContainer{TModel}"/>s of this container.
        /// </summary>
        public readonly Bindable<DraggableItem<TModel>?> CurrentlySharedDraggedItem = new Bindable<DraggableItem<TModel>?>();

        /// <summary>
        /// Hooked onto by <see cref="DraggableItemContainer{TModel}"/>. Invoked when the drag has ended.
        /// </summary>
        public Action DragEnded = () => { };

        private readonly List<DraggableItemContainer<TModel>> draggableItemContainers = [];

        protected DraggableSharingContainer()
        {
            CurrentlySharedDraggedItem.BindValueChanged(d =>
            {
                if (d.OldValue == null)
                    AddInternal(d.NewValue);
                else
                {
                    RemoveInternal(d.OldValue, true);
                    if (d.NewValue != null)
                        AddInternal(d.NewValue);
                }
            });

            CursorPosition.BindValueChanged(d =>
            {
                var draggable = draggableItemContainers.FirstOrDefault(c => c?.ScrollContainerHasCursor() ?? false, null);

                if (draggable != null && draggable.DraggableItemType != CurrentlySharedDraggedItem.Value?.GetType())
                {
                    Logger.Log("Draggable type: " + draggable.DraggableItemType);
                    Logger.Log("Shared type: " + CurrentlySharedDraggedItem.Value?.GetType());

                    draggable.CreateDrawableOnTop();
                }
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            CurrentlySharedDraggedItem.Value?.MoveToCursor(CursorPosition.Value);
        }

        public bool IsDragging => CurrentlySharedDraggedItem.Value != null;

        /// <summary>
        /// Called by the <see cref="DraggableItemContainer{TModel}"/> that started the drag. Will end the drag for all tree.descendant <see cref="DraggableItemContainer{TModel}"/>s.
        /// </summary>
        internal void DragEnd()
        {
            Debug.Assert(CurrentlySharedDraggedItem.Value != null, "Cannot end drag when no DraggableItem is selected.");
            // 1 will count dragging a duplicate item to a container as not shared, since we don't allow duplicates.
            // 2 will count those same items as shared. It is useful for dropping unwanted items into a big pool to remove it, no matter if its already there.
            // idk which one is preferred or if there should be a toggle.
            // WasShared = draggableItemContainers.Any(d => d.ScrollContainerHasCursor() && !d.StartedDrag && !d.Items.Contains(CurrentlySharedDraggedItem.Value.Model)); // 1
            WasShared = draggableItemContainers.Any(d => d.ScrollContainerHasCursor() && !d.StartedDrag); // 2
            DragEnded.Invoke();
            DragEnded = () => { };
            CurrentlySharedDraggedItem.Value = null;
            WasShared = false; // Should probably reset it
        }

        /// <summary>
        /// Adds a <see cref="DraggableItemContainer{TModel}"/> to the pool of shared containers.
        /// </summary>
        /// <param name="container">The <see cref="DraggableItemContainer{TModel}"/> that will have its items shared.</param>
        internal void AddItemContainer(DraggableItemContainer<TModel> container) => draggableItemContainers.Add(container);

        /// <summary>
        /// Removes a <see cref="DraggableItemContainer{TModel}"/> from the pool of shared containers.
        /// </summary>
        /// <param name="container">The <see cref="DraggableItemContainer{TModel}"/> that will no longer have its items shared.</param>
        internal void RemoveItemContainer(DraggableItemContainer<TModel> container) => draggableItemContainers.Remove(container);
    }
}
