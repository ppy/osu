// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableList<T> : RearrangeableListContainer<DrawableListRepresetedItem<T>>, IDrawableListItem<T>
        where T : Drawable
    {
        private Action<T, int> setItemDepth = IDrawableListItem<T>.DEFAULT_SET_ITEM_DEPTH;

        public Action<T, int> SetItemDepth
        {
            get => setItemDepth;
            set
            {
                setItemDepth = value;
                UpdateItem();
            }
        }

        private Action onDragAction;

        public Action OnDragAction
        {
            get => onDragAction;
            set
            {
                onDragAction = value;
                UpdateItem();
            }
        }

        public Action<Action<IDrawableListItem<T>>> ApplyAll
        {
            get => applyAll1;
            set
            {
                applyAll1 = value;
                UpdateItem();
            }
        }

        private Func<T, LocalisableString> getName = IDrawableListItem<T>.GetDefaultText;
        private Action<Action<IDrawableListItem<T>>> applyAll1;
        public T? RepresentedItem => null;

        public IReadOnlyDictionary<DrawableListRepresetedItem<T>, RearrangeableListItem<DrawableListRepresetedItem<T>>> ItemMaps => ItemMap;

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                getName = value;
                UpdateItem();
            }
        }

        public event Action<RearrangeableListItem<DrawableListRepresetedItem<T>>> ItemAdded = _ => { };

        public DrawableList()
        {
            onDragAction = default_onDragAction;
            applyAll1 = ApplyAction;

            RelativeSizeAxes = Axes.X;
            //todo: compute this somehow add runtime
            Height = 100f;
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
                        if (value.Model != key) ListContainer.Add(value);
                    }

                    UpdateItem();
                }
            });
            UpdateItem();
        }

        public virtual Drawable GetDrawableListItem() => this;

        private void default_onDragAction()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                T representedItem = Items[i].RepresentedItem;

                SetItemDepth.Invoke(representedItem, Items.Count - i);
            }
        }

        #region IDrawableListItem<T>

        public void UpdateItem()
        {
            ItemMap.Values.ForEach(item =>
            {
                if (item is IDrawableListItem<T> rearrangableItem)
                {
                    rearrangableItem.ApplyAll = ApplyAll;
                    rearrangableItem.GetName = getName;
                    rearrangableItem.SetItemDepth = SetItemDepth;
                    rearrangableItem.OnDragAction = OnDragAction;
                    rearrangableItem.UpdateItem();
                }
            });
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
                    newItem = new DrawableListItem<T>(item)
                    {
                        ApplyAll = ApplyAll,
                        GetName = GetName,
                        SetItemDepth = SetItemDepth,
                        OnDragAction = OnDragAction,
                    };
                    break;

                case DrawableListEntryType.MinimisableList:
                    newItem = new DrawableMinimisableList<T>(item)
                    {
                        ApplyAll = ApplyAll,
                        GetName = GetName,
                        SetItemDepth = SetItemDepth,
                        OnDragAction = OnDragAction,
                    };
                    break;
            }

            //If this hits, implement new enum Variants
            if (newItem is null) throw new InvalidOperationException();

            Scheduler.Add(newItem.UpdateItem);
            return newItem;
        }
    }
}
