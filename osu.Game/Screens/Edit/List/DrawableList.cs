// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
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
                drawableItem.Dispose();

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

            if (!IsLoaded)
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

        private void startArrangement(AbstractListItem<T> item, DragStartEvent e)
        {
            currentlyDraggedItem = item;
            screenSpaceDragPosition = e.ScreenSpaceMousePosition;
        }

        private void arrange(AbstractListItem<T> item, DragEvent e) => screenSpaceDragPosition = e.ScreenSpaceMousePosition;

        private void endArrangement(AbstractListItem<T> item, DragEndEvent e) => currentlyDraggedItem = null;

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

            dstIndex = Math.Clamp(dstIndex, 0, Items.Count - 1);

            if (srcIndex == dstIndex)
                return;

            Items.Move(srcIndex, dstIndex);

            // Todo: this could be optimised, but it's a very simple iteration over all the items
            sortItems();
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
