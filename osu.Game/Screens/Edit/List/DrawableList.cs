// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

//todo: remove this preprocessor statement completely, once the implementation is done.
//right now I still sometimes want logging statements

#if false
#define VERBOSE_LOGS
#endif

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
using osu.Framework.Input.Events;
#if VERBOSE_LOGS
using osu.Framework.Logging;
#endif
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
        public event Action<AbstractListItem<T>> ItemAdded = _ => { };

        public DrawableList(DrawableListProperties<T> properties)
        {
            this.properties = properties;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            // old: t odo: compute this somehow add runtime
            // Height = ScrollContainer.AvailableContent + 1;
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

        internal void Default_onDragAction()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                T representedItem = Items[i].RepresentedItem;

                Properties.SetItemDepth.Invoke(representedItem, Items.Count - i);
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

        public virtual void Select() => ApplyAction(t => t.Select());
        public virtual void Deselect() => ApplyAction(t => t.Deselect());

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
            var drawablesToAdd = new List<AbstractListItem<T>>();

            foreach (var item in items.Cast<DrawableListRepresetedItem<T>>())
            {
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

        private void updateArrangement()
        {
            if (currentlyDraggedItem is null) return;

            var localPos = ListItems.ToLocalSpace(screenSpaceDragPosition);
            int srcIndex = Items.IndexOf(currentlyDraggedItem.Model);

            // Find the last item with position < mouse position. Note we can't directly use
            // the item positions as they are being transformed
            float heightAccumulator = 0;
            int dstIndex = 0;
            bool itemWasMovedOut = false;

            for (; dstIndex < Items.Count; dstIndex++)
            {
                var drawable = itemMap[Items[dstIndex]];

                if (!drawable.IsLoaded || !drawable.IsPresent)
                    continue;

                // Using BoundingBox here takes care of scale, paddings, etc...
                float height = drawable.BoundingBox.Height;

#if true
                if (drawable is DrawableMinimisableList<T> minimisableListChild
                    && heightAccumulator + minimisableListChild.ListHeadBoundingBox.Height / 2 > localPos.Y)
                {
#if VERBOSE_LOGS
                    Logger.Log($"moving item into list {minimisableListChild.ListHeadText}");
#endif
                    itemWasMovedOut = insertElementIntoList(currentlyDraggedItem, 0, srcIndex, minimisableListChild.List);
                    break;
                }
#endif

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
#if VERBOSE_LOGS
            if (dstIndex < 0) Logger.Log("destination index < 0");
            if (dstIndex > ListItems.Count - 1) Logger.Log("destination index >= ListItems.Count");
#endif
            if (!itemWasMovedOut && Parent is DrawableMinimisableList<T> minimisableList && minimisableList.Parent?.Parent is DrawableList<T> list)
            {
#if VERBOSE_LOGS
                Logger.Log($"Testing parent moving conditions for {minimisableList.ListHeadText}");
                if (list.Parent is DrawableMinimisableList<T> parentMinimisableListLogTest) Logger.Log($"and {parentMinimisableListLogTest.ListHeadText}");
#endif
                int index = list.Items.IndexOf(minimisableList.Model);
                Optional<int> parentDstIndex = default;
                var parentListLocalPosition = list.ToLocalSpace(screenSpaceDragPosition);

                // var lastItem = ItemMap[Items[^1]];
                //
                // if (dstIndex < 0)
                //     parentDstIndex = index;
                // else
                if (parentListLocalPosition.Y < list.ToLocalSpace(minimisableList.ToScreenSpace(Vector2.Zero)).Y - minimisableList.ListHeadBoundingBox.Height / 2)
                    parentDstIndex = index;
                else if (dstIndex > ListItems.Count - 1 && heightAccumulator + list.ListItems.Spacing.Y + list.ItemMap[list.Items[index + 1]].BoundingBox.Height / 2 < localPos.Y)
                    parentDstIndex = index + 1;

                // else if (list.ToLocalSpace(ListItems.ToScreenSpace(Vector2.UnitY * (heightAccumulator - ListItems.Spacing.Y))).Y + list.ListItems.Spacing.Y + list.ItemMap[list.Items[index + 1]].BoundingBox.Height / 2 > parentListLocalPosition.Y)
                //     parentDstIndex = index + 1;

                if (index < 0)
                {
                    const string fail_message = "DrawableMinimisableList is Child of DrawableList, but it's Model is not registered in the Items of the DrawableList";
                    Debug.Fail(fail_message);
                    //A UnreachableException would be more appropriate, for when we use net7
                    throw new ArgumentOutOfRangeException(fail_message);
                }

                if (parentDstIndex.HasValue)
                {
#if VERBOSE_LOGS
                    Logger.Log($"moving item out of list {minimisableList.ListHeadText}");
                    if (list.Parent is DrawableMinimisableList<T> parentMinimisableListLogMove) Logger.Log($"moving into list {parentMinimisableListLogMove.ListHeadText}");
#endif
                    itemWasMovedOut = insertElementIntoList(currentlyDraggedItem, Math.Clamp(parentDstIndex.Value, 0, list.Items.Count), srcIndex, list);
                }
            }

            //If Items.Count == 0, then this will throw errors!
            dstIndex = Math.Clamp(dstIndex, 0, Math.Max(0, Items.Count - 1));

            if (srcIndex == dstIndex && !itemWasMovedOut)
                return;

            if (!itemWasMovedOut)
                Items.Move(srcIndex, dstIndex);

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
