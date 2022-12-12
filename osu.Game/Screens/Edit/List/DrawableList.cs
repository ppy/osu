// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableList<T> : CompositeDrawable, IDrawableListItem<T>
        where T : Drawable
    {
        // private RearrangeableListContainer<T>;
        protected FillFlowContainer<AbstractListItem<T>> ListItems { get; }

        private DrawableListProperties<T> properties;

        public DrawableListProperties<T> Properties
        {
            get => properties;
            internal set
            {
                properties = value;
                UpdateItem();
            }
        }

        public T? RepresentedItem => null;
        public IReadOnlyDictionary<DrawableListRepresetedItem<T>, AbstractListItem<T>> ItemMaps => ItemMap;
        public event Action<AbstractListItem<T>> ItemAdded = static _ => { };

        /// <summary>
        /// This event is called if an item is dragged out of the current list.
        /// This event will be called just before the item is relocated to the new list.
        /// </summary>
        /// Parameters are the item, that is dragged, and the list that the item will be dragged into.
        public event Action<AbstractListItem<T>, DrawableList<T>> ItemDraggedOut = static (_, _) => { };

        /// <summary>
        /// This event is called if an item is dragged into of the current list.
        /// This event will be called just before the item is relocated into the new list.
        /// This also means that the ItemAdded event will trigger AFTER this event.
        /// </summary>
        /// Parameters are the item, that is dragged, and the list that the item was dragged out of.
        public event Action<AbstractListItem<T>, DrawableList<T>> ItemDraggedIn = static (_, _) => { };

        public DrawableList(DrawableListProperties<T> properties)
        {
            this.properties = properties;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = ListItems = new FillFlowContainer<AbstractListItem<T>>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2.5f),
            };
            Items.CollectionChanged += collectionChanged;
            UpdateItem();
        }

        public DrawableList()
            //cannot access this here
            : this(new DrawableListProperties<T>())
        {
            Properties.TopLevelItem = this;
        }

        public virtual Drawable GetDrawableListItem() => this;

        internal void OnDragAction()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                T representedItem = Items[i].RepresentedItem;

                Properties.SetItemDepth.Invoke(representedItem, i);
            }
        }

        #region IDrawableListItem<T>

        DrawableListProperties<T> IDrawableListItem<T>.Properties
        {
            get => Properties;
            set => Properties = value;
        }

        public void UpdateItem()
        {
            ItemMap.Values.ForEach(item =>
            {
                if (item is IDrawableListItem<T> rearrangableItem)
                {
                    rearrangableItem.Properties = Properties;
                    rearrangableItem.UpdateItem();
                }
            });
            //just one scheduler is not enough
            // Scheduler.Add(() => Scheduler.Add(() => Height = ScrollContainer.AvailableContent + 1));
        }

        public virtual void Select() => ApplyAction(static t => t.Select());
        public virtual void Deselect() => ApplyAction(static t => t.Deselect());

        public void ApplyAction(Action<IDrawableListItem<T>> action)
        {
            for (int i = 0; i < ListItems.Children.Count; i++)
            {
                if (ListItems.Children[i] is IDrawableListItem<T> item) item.ApplyAction(action);
            }
        }

        public void SelectInternal() => Select();
        public void DeselectInternal() => Deselect();

        #endregion

        #region RearrangableListContainer reimplementation

        //this field will allow adding items to Items, which are already present in the itemMap.
        //all adds will happen syncronously.
        //this field will also prevent removed items from being disposed explicitly.
        //HANDLE WITH EXTREME CARE. THIS CAN AND PROBABLY WILL BREAK STUFF IF USED MALICOUSLY.
        private bool allowAlreadyExistingDictEntry;

        private const float exp_base = 1.05f;

        /// <summary>
        /// The items contained by this <see cref="DrawableList{T}"/>, in the order they are arranged.
        /// </summary>
        public readonly BindableList<DrawableListRepresetedItem<T>> Items = new BindableList<DrawableListRepresetedItem<T>>();

        /// <summary>
        /// The maximum exponent of the automatic scroll speed at the boundaries of this <see cref="DrawableList{T}"/>.
        /// </summary>
        protected float MaxExponent = 50;

        /// <summary>
        /// The mapping of <see cref="DrawableListRepresetedItem{T}" /> to <see cref="AbstractListItem{T}"/>.
        /// </summary>
        protected IReadOnlyDictionary<DrawableListRepresetedItem<T>, AbstractListItem<T>> ItemMap => itemMap;

        private readonly Dictionary<DrawableListRepresetedItem<T>, AbstractListItem<T>> itemMap = new Dictionary<DrawableListRepresetedItem<T>, AbstractListItem<T>>();
        private AbstractListItem<T>? currentlyDraggedItem;
        private Vector2 screenSpaceDragPosition;

        protected ScrollContainer<Drawable>? ScrollContainer => this.FindClosestParent<ScrollContainer<Drawable>>();

        /// <summary>
        /// Fired whenever new drawable items are added or removed from <see cref="ListItems"/> internally.
        /// </summary>
        protected virtual void OnItemsChanged()
        {
            if (Items.Count <= 0) return;

            OnDragAction();
            Properties.PostOnDragAction(ItemMap[Items[0]]);
        }

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    addItems(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    removeItems(e.OldItems);

                    // Explicitly reset scroll position here so that ScrollContainer doesn't retain our
                    // scroll position if we quickly add new items after calling a Clear().
                    if (Items.Count == 0)
                        ScrollContainer?.ScrollToStart();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    currentlyDraggedItem = null;
                    ListItems.Clear();
                    itemMap.Clear();
                    OnItemsChanged();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    removeItems(e.OldItems);
                    addItems(e.NewItems);
                    break;
            }
        }

        private void removeItems(IList items)
        {
            foreach (var item in items.Cast<DrawableListRepresetedItem<T>>())
            {
                if (currentlyDraggedItem != null && EqualityComparer<DrawableListRepresetedItem<T>>.Default.Equals(currentlyDraggedItem.Model, item))
                    currentlyDraggedItem = null;

                var drawableItem = itemMap[item];

                ListItems.Remove(drawableItem, false);
                if (!allowAlreadyExistingDictEntry) drawableItem.Dispose();

                itemMap.Remove(item);
            }

            sortItems();
            OnItemsChanged();
        }

        private void addItems(IList items)
        {
            Action<int, T> setItemPosition = (oldIndex, item) =>
            {
                //Lazers default positions for new items was at the top of the list.
                //We keep that default for now, IF items have no/invalid depth information
                if (oldIndex > 0 && oldIndex < Items.Count)
                {
                    int intDepth = (int)Properties.GetDepth(item);
                    int depthIndex = 0;

                    //look for the appropriate place, for items of the given depth.
                    //We cannot rely on the default OnDragAction, because it can be changed.
                    for (; depthIndex < Items.Count; depthIndex++)
                    {
                        if (Properties.GetDepth(Items[depthIndex].RepresentedItem) >= intDepth) break;
                    }

                    //now that we found the appropriate place, we move the item there.
                    //this will leave us with a sorted list (when looking at depths), given that the starting list is sorted.
                    //When adding new elements, we might end up with multiple elements of the same depth.
                    //In that case new items should always get ordered before old items of the same depth.
                    //the depths still need to be fixed by the next OnDepthAction call, that happens in OnItemsChanged.
                    Items.Move(oldIndex, !Properties.GetDepth(item).Equals(intDepth) ? 0 : depthIndex);
                }
            };
            var drawablesToAdd = new List<AbstractListItem<T>>();

            foreach (var item in items.Cast<DrawableListRepresetedItem<T>>())
            {
                int oldIndex = Items.IndexOf(item);
                setItemPosition(oldIndex, item.RepresentedItem);

                if (itemMap.ContainsKey(item))
                {
                    if (allowAlreadyExistingDictEntry)
                    {
                        var listItem = ItemMap[item];
                        listItem.StartArrangement = startArrangement;
                        listItem.Arrange = arrange;
                        listItem.EndArrangement = endArrangement;
                        drawablesToAdd.Add(listItem);
                        continue;
                    }

                    throw new InvalidOperationException(
                        $"Duplicate items cannot be added to a {nameof(BindableList<DrawableListRepresetedItem<T>>)} that is currently bound with a {nameof(RearrangeableListContainer<DrawableListRepresetedItem<T>>)}.");
                }

                AbstractListItem<T> drawable = CreateDrawable(item).With(d =>
                {
                    d.StartArrangement += startArrangement;
                    d.Arrange += arrange;
                    d.EndArrangement += endArrangement;
                });

                drawablesToAdd.Add(drawable);
                itemMap[item] = drawable;
            }

            if (!IsLoaded || allowAlreadyExistingDictEntry)
                addToHierarchy(drawablesToAdd);
            else
                LoadComponentsAsync(drawablesToAdd, addToHierarchy);

            void addToHierarchy(IEnumerable<Drawable> drawables)
            {
                foreach (var d in drawables.Cast<AbstractListItem<T>>())
                {
                    // Don't add drawables whose models were removed during the async load, or drawables that are no longer attached to the contained model.
                    if (itemMap.TryGetValue(d.Model, out var modelDrawable) && modelDrawable == d)
                    {
                        d.ResetEvents();
                        ItemAdded.Invoke(d);
                        ListItems.Add(d);
                    }
                }

                sortItems();
                OnItemsChanged();
            }
        }

        private void sortItems()
        {
            //sync ListItems Positions to Items
            for (int i = 0; i < Items.Count; i++)
            {
                var drawable = itemMap[Items[i]];

                // If the async load didn't complete, the item wouldn't exist in the container and an exception would be thrown
                if (drawable.Parent == ListItems)
                    ListItems.SetLayoutPosition(drawable, i);
            }
        }

        private void startArrangement(AbstractListItem<T> item, DragStartEvent e) => startArrangement(item, e.ScreenSpaceMousePosition);

        private void startArrangement(AbstractListItem<T> item, Vector2 screenSpaceMousePosition)
        {
            currentlyDraggedItem = item;
            screenSpaceDragPosition = screenSpaceMousePosition;
        }

        private void arrange(AbstractListItem<T> item, Vector2 screenSpaceMousePosition) => screenSpaceDragPosition = screenSpaceMousePosition;
        private void arrange(AbstractListItem<T> item, DragEvent e) => arrange(item, e.ScreenSpaceMousePosition);

        private void endArrangement(AbstractListItem<T> item, DragEndEvent e) => endArrangement(item);
        private void endArrangement(AbstractListItem<T> item) => currentlyDraggedItem = null;

        protected override void Update()
        {
            base.Update();

            if (currentlyDraggedItem != null)
                updateScrollPosition();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            updateArrangement();
        }

        private void updateScrollPosition()
        {
            if (ScrollContainer is null) return;

            Vector2 localPos = ScrollContainer.ToLocalSpace(screenSpaceDragPosition);
            float scrollSpeed = 0;

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

        /// <summary>
        /// Gives a Value back, to which the currentlyDragged item should be moved.
        /// If the currentlyDragged item doesn't need to be moved, or is removed from this list, an empty Value is returned
        /// </summary>
        /// <returns>A index to which the currentlyDraggedItem should be moved, or an empty Optional</returns>
        private Optional<int> checkArrangement()
        {
            if (currentlyDraggedItem is null) return default;

            int srcIndex = Items.IndexOf(currentlyDraggedItem.Model);
            var localPos = ListItems.ToLocalSpace(screenSpaceDragPosition);

            if (localPos.Y > currentlyDraggedItem.BoundingBox.Top && localPos.Y < currentlyDraggedItem.BoundingBox.Bottom) return default;

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

                if (drawable is DrawableMinimisableList<T> minimisableListChild
                    && heightAccumulator + getBoundingBox(minimisableListChild).Height / 2 < localPos.Y
                    && heightAccumulator + drawable.BoundingBox.Height - getBoundingBox(minimisableListChild).Height / 2 > localPos.Y)
                {
                    if (insertElementIntoList(currentlyDraggedItem, 0, srcIndex, minimisableListChild.List))
                        return default;

                    break;
                }

                // Rearrangement should occur only after the mid-point of items is crossed
                heightAccumulator += height / 2;

                // Check if the midpoint has been crossed (i.e. cursor is located above the midpoint)
                if (heightAccumulator > localPos.Y)
                {
                    if (dstIndex > srcIndex)
                    {
                        // Suppose an item is dragged just slightly below its own midpoint. The rearrangement condition (accumulator > pos) will be satisfied for the next immediate item
                        // but not the currently-dragged item, which will invoke a rearrangement. This is an off-by-one condition.
                        // Rearrangement should not occur until the midpoint of the next item is crossed, and so to fix this the next item's index is discarded.
                        dstIndex--;
                    }

                    break;
                }

                // Add the remainder of the height of the current item
                heightAccumulator += height / 2 + ListItems.Spacing.Y;
            }

            if (Parent is DrawableMinimisableList<T> minimisableList
                && minimisableList.Parent?.Parent is DrawableList<T> list)
            {
                var posInList = list.ListItems.ToLocalSpace(screenSpaceDragPosition);
                int parentIndex = list.Items.IndexOf(minimisableList.Model);

                if (parentIndex < 0)
                {
                    const string fail_message = "DrawableMinimisableList is Child of DrawableList, but it's Model is not registered in the Items of the DrawableList";
                    Debug.Fail(fail_message);
                    //A UnreachableException would be more appropriate, for when we use net7
                    throw new ArgumentOutOfRangeException(fail_message);
                }

                if (posInList.Y < ToSpaceOfOtherDrawable(BoundingBox.TopLeft, list.ListItems).Y
                    && insertElementIntoList(currentlyDraggedItem, parentIndex, srcIndex, list))
                    return default;
                if (posInList.Y > ToSpaceOfOtherDrawable(BoundingBox.BottomLeft, list.ListItems).Y
                    && insertElementIntoList(currentlyDraggedItem, parentIndex + 1, srcIndex, list))
                    return default;
            }

            return dstIndex;
        }

        private void updateArrangement()
        {
            var index = checkArrangement();

            if (index.HasValue && currentlyDraggedItem is not null)
            {
                int srcIndex = Items.IndexOf(currentlyDraggedItem.Model);

                //If Items.Count == 0, then this will throw errors!
                int dstIndex = Math.Clamp(index.Value, 0, Math.Max(0, Items.Count - 1));

                if (srcIndex == dstIndex)
                    return;

                Items.Move(srcIndex, dstIndex);
            }

            // Todo: this could be optimised, but it's a very simple iteration over all the items
            sortItems();
        }

        /// <summary>
        /// Moves item (at index srcIndex) into list at (listDstIndex)
        /// </summary>
        /// <param name="item">the item to be moved</param>
        /// <param name="listDstIndex">the index in the target list, the item should be at</param>
        /// <param name="srcIndex">the index of item in this list</param>
        /// <param name="list">the target list, to add the item to</param>
        /// <returns>if the item was actally moved. e.g. we do not want to move a list into itself</returns>
        private bool insertElementIntoList(AbstractListItem<T> item, int listDstIndex, int srcIndex, DrawableList<T> list)
        {
            //don't add a list to itself please.
            if (item is DrawableMinimisableList<T> itemMinimisableList && itemMinimisableList.List == list) return false;
            //if we drag a item into a list, expand it
            if (list.Parent is DrawableMinimisableList<T> listParentMinimisableList && !listParentMinimisableList.Enabled.Value) listParentMinimisableList.ShowList();

            ItemDraggedOut.Invoke(item, list);
            list.ItemDraggedIn.Invoke(item, this);
            // Logger.Log($"Determined parent index to be {listDstIndex}");

            //remove item from this list
            allowAlreadyExistingDictEntry = true;
            Items.RemoveAt(srcIndex); //remove from items and itemMap (synced via CollectionChanged Event)
            allowAlreadyExistingDictEntry = false;
            //add item to passed list argument
            list.allowAlreadyExistingDictEntry = true;
            list.itemMap[item.Model] = item;
            list.Items.Insert(listDstIndex, item.Model);
            list.allowAlreadyExistingDictEntry = false;
            //set the flags in the target list correctly, so it doesn't get confused on a drag event.
            list.startArrangement(list.ItemMap[item.Model], screenSpaceDragPosition);
            return true;
        }

        private Quad getBoundingBox(DrawableMinimisableList<T> item) => item.ToSpaceOfOtherDrawable(item.ListHeadBoundingBox, ListItems);

        #endregion

        protected AbstractListItem<T> CreateDrawable(DrawableListRepresetedItem<T> item)
        {
            // Logger.Log("CreateDrawable");

            AbstractListItem<T> newItem;

            switch (item.Type)
            {
                case DrawableListEntryType.Item:
                    newItem = new DrawableListItem<T>(item, Properties);
                    break;

                case DrawableListEntryType.MinimisableList:
                    newItem = new DrawableMinimisableList<T>(item, Properties);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(item.Type), (int)item.Type, "Recived invalid enum Variant. If you see this, check if all DrawableListEntryType variants are handled.");
            }

            Scheduler.Add(newItem.UpdateItem);
            return newItem;
        }
    }
}
