// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osu.Framework.Allocation;
using System.Diagnostics;
using osu.Framework.Logging;

namespace osu.Game.Graphics.Containers.Draggable
{
    // Modified version of <see cref="RearrangeableListContainer{TModel}"/>.
    // Would be nice to move to Framework later.

    // todo : make tests for Draggable-family.

    /// <summary>
    /// A list container that enables its children to be rearranged via dragging.
    /// </summary>
    /// <remarks>
    /// Adding duplicate items is not currently supported.
    /// Also the <see cref="DraggableItem{TModel}"/> shown to be dragged is a different instance from the <see cref="DraggableItem{TModel}"/> that was initially dragged.
    /// </remarks>
    /// <typeparam name="TModel">The type of rearrangeable item.</typeparam>
    public abstract partial class DraggableItemContainer<TModel> : CompositeDrawable
        where TModel : notnull
    {
        /// <summary>
        /// If this container should retain a <see cref="DraggableItem{TModel}"/> when it is dropped outside a of the ScrollContainer.
        /// This container will still lose the item if it is dragged into another <see cref="DraggableItemContainer{TModel}"/> and IsSharedItemRetained is false.
        /// Does nothing if IsAllDroppedItemsRetained is true.
        /// </summary>
        public bool IsDroppedItemRetained = false;

        /// <summary>
        /// If this container should retain a <see cref="DraggableItem{TModel}"/> when it is dropped inside another <see cref="DraggableItemContainer{TModel}"/>.
        /// This container will still lose the item if it is not dragged into another <see cref="DraggableItemContainer{TModel}"/> and IsDroppedItemRetained is false.
        /// </summary>
        public bool IsSharedItemRetained = false;

        /// <summary>
        /// If true, <see cref="DraggableItem{TModel}"/>s can be moved around, but will always revert to their initial position.
        /// This container will still lose the item if it is not dragged into another <see cref="DraggableItemContainer{TModel}"/> and IsDroppedItemRetained is false.
        /// </summary>
        public bool IsStrictlySorted = false;

        /// <summary>
        /// When dragging a <see cref="DraggableItem{TModel}"/> inside a list, how far do you need to move before the empty item switches with the next item.
        /// Measured in relative size of a <see cref="DraggableItem{TModel}"/> from the start of the next item.
        /// </summary>
        /// <remarks>
        /// Currently not implemented.
        /// </remarks>
        public float DraggedItemMoveThreshold = 0.5f;

        /// <summary>
        /// If this container should retain a <see cref="DraggableItem{TModel}"/> when it is dropped by any <see cref="DraggableItemContainer{TModel}"/>s.
        /// This container will not pick up the item if it is dragged into another <see cref="DraggableItemContainer{TModel}"/>.
        /// This container will still lose the item if it is dragged into another <see cref="DraggableItemContainer{TModel}"/> and IsSharedItemRetained is false.
        /// </summary>
        /// <remarks>
        /// Currently not implemented.
        /// </remarks>
        public bool IsAllDroppedItemsRetained;

        private const float exp_base = 1.05f;

        [Resolved]
        protected DraggableSharingContainer<TModel>? DraggableSharingContainer { get; private set; }

        /// <summary>
        /// The items contained by this <see cref="DraggableItemContainer{TModel}"/>, in the order they are arranged.
        /// </summary>
        public readonly BindableList<TModel> Items = new BindableList<TModel>();

        /// <summary>
        /// The maximum exponent of the automatic scroll speed at the boundaries of this <see cref="DraggableItemContainer{TModel}"/>.
        /// </summary>
        protected float MaxExponent = 50;

        /// <summary>
        /// The <see cref="ScrollContainer"/> containing the flow of items.
        /// </summary>
        protected readonly ScrollContainer<Drawable> ScrollContainer;

        /// <summary>
        /// The <see cref="FillFlowContainer"/> containing of all the <see cref="DraggableItem{TModel}"/>s.
        /// </summary>
        protected readonly FillFlowContainer<DraggableItem<TModel>> ListContainer;

        /// <summary>
        /// The mapping of <typeparamref name="TModel"/> to <see cref="DraggableItem{TModel}"/>.
        /// </summary>
        protected IReadOnlyDictionary<TModel, DraggableItem<TModel>> ItemMap => itemMap;

        private readonly Dictionary<TModel, DraggableItem<TModel>> itemMap = new Dictionary<TModel, DraggableItem<TModel>>();

        private DraggableItem<TModel>? currentlyDraggedItem;
        private int revertIndex = -1;
        private bool isCurrentlyDraggedItemTemporary = true;

        // Is a different instance to currentlyDraggedItem. Possibly different subtype aswell.
        private Bindable<DraggableItem<TModel>?> currentlySharedDraggedItem = new();
        private Bindable<Vector2> cursorPosition = new();

        private bool shouldRetainCurrentlyDraggedItem()
        {
            // ugly but should work.
            return (
                (DraggableSharingContainer != null && DraggableSharingContainer.WasShared && isCurrentlyDraggedItemTemporary && ScrollContainerHasCursor()) ||
                (DraggableSharingContainer != null && DraggableSharingContainer.WasShared && !isCurrentlyDraggedItemTemporary && IsSharedItemRetained) ||
                (DraggableSharingContainer != null && !DraggableSharingContainer.WasShared && !isCurrentlyDraggedItemTemporary && IsDroppedItemRetained) ||
                (DraggableSharingContainer == null && ScrollContainerHasCursor())
            );
        }

        /// <summary>
        /// Creates a new <see cref="DraggableItemContainer{TModel}"/>.
        /// </summary>
        protected DraggableItemContainer()
        {
            ListContainer = CreateListFillFlowContainer().With(d =>
            {
                d.RelativeSizeAxes = Axes.X;
                d.AutoSizeAxes = Axes.Y;

                // todo : Add support for FIllDirection.Full
                d.Direction = FillDirection.Vertical;
            });

            InternalChild = ScrollContainer = CreateScrollContainer().With(d =>
            {
                d.RelativeSizeAxes = Axes.Both;
                d.Add(ListContainer);
            });

            Items.CollectionChanged += collectionChanged;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (DraggableSharingContainer != null)
            {
                currentlySharedDraggedItem.BindTo(DraggableSharingContainer.CurrentlySharedDraggedItem);
                cursorPosition.BindTo(DraggableSharingContainer.CursorPosition);
                cursorPosition.BindValueChanged(c =>
                {
                    // todo : Would be more optimal to have in ScrollContainer.OnHover
                    if (currentlyDraggedItem == null && ScrollContainerHasCursor())
                        createTemporaryDraggableItem();
                });
                DraggableSharingContainer.AddItemContainer(this);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            currentlySharedDraggedItem.UnbindAll();
            cursorPosition.UnbindAll();
            DraggableSharingContainer?.RemoveItemContainer(this);
        }

        /// <summary>
        /// Fired whenever new drawable items are added or removed from <see cref="ListContainer"/>.
        /// </summary>
        protected virtual void OnItemsChanged()
        {
        }

        #region Collection management

        private void collectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // todo : imporve efficiency be reducing number of calls to sortItems.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex != Items.Count - 1)
                        insertItems(e.NewItems?.Cast<TModel>() ?? [], e.NewStartingIndex);
                    else
                        addItems(e.NewItems?.Cast<TModel>() ?? []);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    removeItems(e.OldItems?.Cast<TModel>() ?? []);

                    // Explicitly reset scroll position here so that ScrollContainer doesn't retain our
                    // scroll position if we quickly add new items after calling a Clear().
                    if (Items.Count == 0)
                        ScrollContainer.ScrollToStart();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    currentlyDraggedItem = null;
                    ListContainer.Clear();
                    itemMap.Clear();
                    OnItemsChanged();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    IEnumerable<TModel> tOldItems = e.OldItems?.Cast<TModel>() ?? [];
                    IEnumerable<TModel> tNewItems = e.NewItems?.Cast<TModel>() ?? [];

                    // OnItemsChanged is called twice, idk i that is intended functionality or potential bug.
                    removeItems(tOldItems.Except(tNewItems));
                    addItems(tNewItems.Except(tOldItems));
                    break;

                case NotifyCollectionChangedAction.Move:
                    sortItems();
                    OnItemsChanged();
                    break;
            }
        }

        private void removeItems(IEnumerable<TModel> items)
        {
            foreach (var item in items)
            {
                if (currentlyDraggedItem != null && EqualityComparer<TModel>.Default.Equals(currentlyDraggedItem.Model, item))
                    currentlyDraggedItem = null;

                var drawableItem = itemMap[item];

                ListContainer.Remove(drawableItem, false);
                // DisposeChildAsync(drawableItem); // internal method in osu.Framework
                drawableItem.Dispose();

                itemMap.Remove(item);
            }

            // if items are already sorted there is no need to sort on removal.
            sortItems();
            OnItemsChanged();
        }

        private void addItems(IEnumerable<TModel> items)
        {
            var drawablesToAdd = new List<Drawable>();

            foreach (var item in items)
            {
                if (itemMap.ContainsKey(item))
                {
                    throw new InvalidOperationException(
                        $"Duplicate items cannot be added to a {nameof(BindableList<TModel>)} that is currently bound with a {nameof(DraggableItemContainer<TModel>)}.");
                }

                var drawable = CreateDrawable(item).With(d =>
                {
                    d.StartArrangement += startArrangement;
                    d.Arrange += arrange;
                    d.EndArrangement += endArrangement;
                });

                drawablesToAdd.Add(drawable);
                itemMap[item] = drawable;
            }

            if (!IsLoaded)
                addToHierarchy(drawablesToAdd);
            else
                LoadComponentsAsync(drawablesToAdd, addToHierarchy);

            void addToHierarchy(IEnumerable<Drawable> drawables)
            {
                foreach (var d in drawables.Cast<DraggableItem<TModel>>())
                {
                    // Don't add drawables whose models were removed during the async load, or drawables that are no longer attached to the contained model.
                    if (itemMap.TryGetValue(d.Model, out var modelDrawable) && modelDrawable == d)
                        ListContainer.Add(d);
                }

                sortItems();
                OnItemsChanged();
            }
        }

        private void insertItems(IEnumerable<TModel> items, int index)
        {
            var drawablesToAdd = new List<Drawable>();

            foreach (var item in items)
            {
                if (itemMap.ContainsKey(item))
                {
                    throw new InvalidOperationException(
                        $"Duplicate items cannot be added to a {nameof(BindableList<TModel>)} that is currently bound with a {nameof(DraggableItemContainer<TModel>)}.");
                }

                var drawable = CreateDrawable(item).With(d =>
                {
                    d.StartArrangement += startArrangement;
                    d.Arrange += arrange;
                    d.EndArrangement += endArrangement;
                });

                drawablesToAdd.Add(drawable);
                itemMap[item] = drawable;
            }

            if (!IsLoaded)
                addToHierarchy(drawablesToAdd);
            else
                LoadComponentsAsync(drawablesToAdd, addToHierarchy);

            void addToHierarchy(IEnumerable<Drawable> drawables)
            {
                foreach (var d in drawables.Cast<DraggableItem<TModel>>())
                {
                    // Don't add drawables whose models were removed during the async load, or drawables that are no longer attached to the contained model.
                    if (itemMap.TryGetValue(d.Model, out var modelDrawable) && modelDrawable == d)
                        ListContainer.Insert(index, d);
                }

                OnItemsChanged();
            }
        }

        private void sortItems()
        {
            Logger.Log("sortItems called!");
            for (int i = 0; i < Items.Count; i++)
            {
                // A drawable for the item may not exist yet, for example in a replace-range operation where the removal happens first.
                if (!itemMap.TryGetValue(Items[i], out var drawable))
                    continue;

                // The item may not be loaded yet, because add operations are asynchronous.
                if (drawable.Parent != ListContainer)
                    continue;

                ListContainer.SetLayoutPosition(drawable, i);
            }
        }

        #endregion

        #region Draggable Management

        private void startArrangement(DraggableItem<TModel> item, DragStartEvent e)
        {
            Debug.Assert(currentlyDraggedItem == null,
                "Tried to start an arrangement while the previous one is not finished. Only one arrangement per sharing group is allowed.");
            Debug.Assert(currentlySharedDraggedItem.Value == null,
                "Tried to start an arrangement while the previous one is not finished. Only one arrangement per sharing group is allowed.");

            currentlyDraggedItem = item.With(d => { d.Alpha = 0.0f; });
            revertIndex = Items.IndexOf(currentlyDraggedItem.Model);
            isCurrentlyDraggedItemTemporary = false;

            CreateDrawableOnTop();

            if (DraggableSharingContainer == null)
                AddInternal(currentlySharedDraggedItem.Value);
            else
                DraggableSharingContainer.DragEnded += FinalizeArrangement;

            cursorPosition.Value = e.ScreenSpaceMousePosition;
        }

        private void arrange(DraggableItem<TModel> item, DragEvent e) => cursorPosition.Value = e.ScreenSpaceMousePosition;

        private void endArrangement(DraggableItem<TModel> item, DragEndEvent e)
        {
            DraggableSharingContainer?.DragEnd();

            // Fallback
            if (DraggableSharingContainer == null)
            {
                FinalizeArrangement();

                RemoveInternal(currentlySharedDraggedItem.Value, true);
                currentlySharedDraggedItem.Value = null;
            }

            Logger.Log("Dropped Item!");
        }

        private void createTemporaryDraggableItem()
        {
            Debug.Assert(DraggableSharingContainer != null);
            Debug.Assert(currentlySharedDraggedItem.Value != null);

            Items.Add(currentlySharedDraggedItem.Value.Model);
            itemMap.TryGetValue(currentlySharedDraggedItem.Value.Model, out var draggable);
            Debug.Assert(draggable != null);
            currentlyDraggedItem = draggable.With(d => { d.Alpha = 0.0f; });

            revertIndex = Items.IndexOf(currentlyDraggedItem.Model); // Only required for when IsStrictlySorted is true
            isCurrentlyDraggedItemTemporary = true; // strictly speaking not neccessary

            DraggableSharingContainer.DragEnded += FinalizeArrangement;
        }

        // todo : is there a way to return the DraggableItem type of CreateDrawable statically or without needing to call CreateDrawable first?
        internal Type DraggableItemType => currentlyDraggedItem?.GetType() ?? typeof(DraggableItem<TModel>);

        internal bool ScrollContainerHasCursor() => ScrollContainer.Contains(cursorPosition.Value);

        internal void CreateDrawableOnTop()
        {
            if (currentlyDraggedItem != null)
                currentlySharedDraggedItem.Value = CreateDrawable(currentlyDraggedItem.Model).With(d => { d.Origin = Anchor.Centre; });
        }

        /// <summary>
        /// Finalizes the position of currentlyDraggedItem, possibly moving it back to where it started.
        /// </summary>
        internal void FinalizeArrangement()
        {
            if (currentlyDraggedItem != null)
            {
                // currentlyDraggedItem might become invalid, change parameters beforehand.
                currentlyDraggedItem.Alpha = 1.0f;

                if (!ScrollContainerHasCursor())
                    if (shouldRetainCurrentlyDraggedItem())
                        Items.Move(Items.IndexOf(currentlyDraggedItem.Model), revertIndex);
                    else
                        Items.Remove(currentlyDraggedItem.Model);
                else if (IsStrictlySorted)
                    Items.Move(Items.IndexOf(currentlyDraggedItem.Model), revertIndex);

                currentlyDraggedItem = null;
                isCurrentlyDraggedItemTemporary = true;
                revertIndex = -1;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (currentlyDraggedItem != null)
                updateScrollPosition();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (currentlyDraggedItem != null)
                updateArrangement();
        }

        private void updateScrollPosition()
        {
            Debug.Assert(currentlyDraggedItem != null);

            // todo : When the container can be scrolled in a direction, add an overlay on the edge spaces for an arrow and faint shadow

            Vector2 localPos = ScrollContainer.ToLocalSpace(cursorPosition.Value);
            float scrollSpeed = 0;

            // todo : this code (and likely updateArrangement) is not immune to scaling. Needs rewriting.

            // Make sure to only scroll when cursor is within ScrollContainer
            if (localPos.X < 0 || localPos.X > ScrollContainer.DrawWidth)
                return;

            if (localPos.Y < 0)
            {
                float power = Math.Min(MaxExponent, Math.Abs(localPos.Y));
                scrollSpeed = (float)(-MathF.Pow(exp_base, power) * Clock.ElapsedFrameTime * 0.1);
            }
            else if (localPos.Y > ScrollContainer.DrawHeight)
            {
                float power = Math.Min(MaxExponent, Math.Abs(ScrollContainer.DrawHeight - localPos.Y));
                scrollSpeed = (float)(MathF.Pow(exp_base, power) * Clock.ElapsedFrameTime * 0.1);
            }

            if ((scrollSpeed < 0 && !ScrollContainer.IsScrolledToStart()) || (scrollSpeed > 0 && !ScrollContainer.IsScrolledToEnd()))
                ScrollContainer.ScrollBy(scrollSpeed);
        }

        private void updateArrangement()
        {
            Debug.Assert(currentlyDraggedItem != null);

            var localPos = ListContainer.ToLocalSpace(cursorPosition.Value);
            int srcIndex = Items.IndexOf(currentlyDraggedItem.Model);

            // todo : move to on drag update, slightly more efficient.
            currentlyDraggedItem.AlwaysPresent = ScrollContainerHasCursor();
            if (!currentlyDraggedItem.AlwaysPresent)
                return;

            // Find the last item with position < mouse position. Note we can't directly use
            // the item positions as they are being transformed
            float heightAccumulator = 0;
            int dstIndex = 0;

            for (; dstIndex < Items.Count; dstIndex++)
            {
                var drawable = itemMap[Items[dstIndex]];

                if (!drawable.IsLoaded || !drawable.IsPresent)
                    continue;

                // Using BoundingBox here takes care of scale, paddings, etc...
                float height = drawable.BoundingBox.Height;

                // Rearrangement should occur only after the mid-point of items is crossed
                heightAccumulator += height / 2;

                // Check if the midpoint has been crossed (i.e. cursor is located above the midpoint)
                if (heightAccumulator > localPos.Y)
                {
                    if (dstIndex > srcIndex)
                        // Suppose an item is dragged just slightly below its own midpoint. The rearrangement condition (accumulator > pos) will be satisfied for the next immediate item
                        // but not the currently-dragged item, which will invoke a rearrangement. This is an off-by-one condition.
                        // Rearrangement should not occur until the midpoint of the next item is crossed, and so to fix this the next item's index is discarded.
                        dstIndex--;

                    break;
                }

                // Add the remainder of the height of the current item
                heightAccumulator += height / 2 + ListContainer.Spacing.Y;
            }

            dstIndex = Math.Clamp(dstIndex, 0, Items.Count - 1);

            if (srcIndex == dstIndex)
                return;

            Items.Move(srcIndex, dstIndex);
        }

        #endregion

        /// <summary>
        /// Creates the <see cref="FillFlowContainer{DrawableRearrangeableListItem}"/> for the items.
        /// </summary>
        protected virtual FillFlowContainer<DraggableItem<TModel>> CreateListFillFlowContainer() => new FillFlowContainer<DraggableItem<TModel>>();

        /// <summary>
        /// Creates the <see cref="ScrollContainer"/> for the list of items.
        /// </summary>
        protected abstract ScrollContainer<Drawable> CreateScrollContainer();

        /// <summary>
        /// Creates the <see cref="Drawable"/> representation of an item.
        /// </summary>
        /// <param name="item">The item to create the <see cref="Drawable"/> representation of.</param>
        /// <returns>The <see cref="DraggableItem{TModel}"/>.</returns>
        protected abstract DraggableItem<TModel> CreateDrawable(TModel item);
    }
}
