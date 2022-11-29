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
    public partial class DrawableList<T> : RearrangeableListContainer<IDrawableListRepresetedItem<T>>, IDrawableListItem<T>
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

        public Action<Action<IDrawableListItem<T>>> ApplyAll { get; set; }

        private Func<T, LocalisableString> getName;
        public T? RepresentedItem => null;

        public IReadOnlyDictionary<IDrawableListRepresetedItem<T>, RearrangeableListItem<IDrawableListRepresetedItem<T>>> ItemMaps => ItemMap;

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                getName = value;
                UpdateItem();
            }
        }

        public DrawableList()
        {
            getName = IDrawableListItem<T>.GetDefaultText;
            onDragAction = default_onDragAction;
            ApplyAll = applyAll;

            RelativeSizeAxes = Axes.X;
            //todo: compute this somehow add runtime
            Height = 100f;
            ListContainer.Spacing = new Vector2(2.5f);
            Items.BindCollectionChanged((s, t) =>
            {
                if (t?.NewItems != null && t.NewItems.Count > 0) UpdateItem();
            });
            UpdateItem();
        }

        public virtual Drawable GetDrawableListItem() => this;

        private void default_onDragAction()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var representedItem = Items[i].RepresentedItem;
                if (representedItem is null) continue;

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

        /// <summary>
        /// Selects obj, if it can be cast to a IDrawableListItem.
        /// </summary>
        /// <param name="obj">the object to call Select on</param>
        /// <param name="value">The value to pass to the Select call of IDrawableListItem</param>
        /// <returns>If Select was actually called</returns>
        private static bool select(object obj, bool value)
        {
            if (obj is IDrawableListItem<T> item)
            {
                item.Select(value);
                return true;
            }

            return false;
        }

        public virtual void Select(bool value)
        {
            foreach (var listItem in ItemMap.Values)
            {
                select(listItem, value);
            }
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action)
        {
            for (int i = 0; i < ListContainer.Children.Count; i++)
            {
                if (ListContainer.Children[i] is IDrawableListItem<T> item) item.ApplyAction(action);
            }
        }

        private void applyAll(Action<IDrawableListItem<T>> action)
        {
            for (int i = 0; i < ListContainer.Children.Count; i++)
            {
                if (ListContainer.Children[i] is IDrawableListItem<T> item) item.ApplyAction(action);
            }
        }

        public void SelectInternal(bool value) => Select(value);

        #endregion

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override RearrangeableListItem<IDrawableListRepresetedItem<T>> CreateDrawable(IDrawableListRepresetedItem<T> item)
        {
            // Logger.Log("CreateDrawable");

            if (item is IRearrangableDrawableListItem<T> listItem)
            {
                // Logger.Log("Getting RearrangeableListItem");
                return listItem.GetRearrangeableListItem();
            }

            if (item.RepresentedItem is null) throw new NullReferenceException();

            // Logger.Log("Making DrawableListItem");
            return new DrawableListItem<T>(item)
            {
                ApplyAll = ApplyAll,
                GetName = getName,
            };
            // drawable.UpdateItem();
        }
    }
}
