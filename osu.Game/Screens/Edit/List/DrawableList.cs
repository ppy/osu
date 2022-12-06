// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableList<T> : RearrangeableListContainer<DrawableListRepresetedItem<T>>, IDrawableListItem<T>
        where T : Drawable
    {
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
        public IReadOnlyDictionary<DrawableListRepresetedItem<T>, RearrangeableListItem<DrawableListRepresetedItem<T>>> ItemMaps => ItemMap;
        public event Action<RearrangeableListItem<DrawableListRepresetedItem<T>>> ItemAdded = _ => { };

        public DrawableList(DrawableListProperties<T> properties)
        {
            this.properties = properties;
            RelativeSizeAxes = Axes.X;
            //todo: compute this somehow add runtime
            Height = ScrollContainer.AvailableContent + 1;
            ListContainer.Spacing = new Vector2(2.5f);
            Items.BindCollectionChanged((s, t) =>
            {
                if (t?.NewItems != null && t.NewItems.Count > 0)
                {
                    foreach (object item in t.NewItems)
                    {
                        var key = item as DrawableListRepresetedItem<T>;
                        if (key is null) return;

                        ItemMaps.TryGetValue(key, out var value);
                        if (value is null) return;

                        ItemAdded.Invoke(value);
                        //Add new items manually, if they weren't added automatically
                        //todo: is this still needed?
                        if (value.Model != key) ListContainer.Add(value);

                        Height = ScrollContainer.AvailableContent + 1;
                    }

                    UpdateItem();
                }
            });
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
            Scheduler.Add(() => Scheduler.Add(() => Height = ScrollContainer.AvailableContent + 1));
        }

        public virtual void Select() => ApplyAction(t => t.Select());
        public virtual void Deselect() => ApplyAction(t => t.Deselect());

        public void ApplyAction(Action<IDrawableListItem<T>> action)
        {
            for (int i = 0; i < ListContainer.Children.Count; i++)
            {
                if (ListContainer.Children[i] is IDrawableListItem<T> item) item.ApplyAction(action);
            }
        }

        public void SelectInternal() => Select();
        public void DeselectInternal() => Deselect();

        #endregion

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override RearrangeableListItem<DrawableListRepresetedItem<T>> CreateDrawable(DrawableListRepresetedItem<T> item)
        {
            // Logger.Log("CreateDrawable");

            AbstractListItem<T>? newItem = null;

            switch (item.Type)
            {
                case DrawableListEntryType.Item:
                    newItem = new DrawableListItem<T>(item, Properties);
                    break;

                case DrawableListEntryType.MinimisableList:
                    newItem = new DrawableMinimisableList<T>(item, Properties);
                    break;
            }

            //If this hits, implement new enum Variants
            if (newItem is null) throw new InvalidOperationException();

            Scheduler.Add(newItem.UpdateItem);
            return newItem;
        }
    }
}
