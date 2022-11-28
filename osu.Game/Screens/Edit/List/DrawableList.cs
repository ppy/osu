// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableList<T> : RearrangeableListContainer<T>, IDrawableListItem<T>
                                   , IContainerEnumerable<T>, IContainerCollection<T>, ICollection<T>, IReadOnlyList<T>
        where T : Drawable
    {
        private Action onDragAction { get; set; }

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
            ApplyAll = applyAll;
            onDragAction = () => { };

            RelativeSizeAxes = Axes.X;
            //todo: compute this somehow add runtime
            Height = 100f;
            ListContainer.Spacing = new Vector2(2.5f);
            UpdateItem();
        }

        private void addInternal(T? drawable)
        {
            if (drawable is null || Items.Contains(drawable)) return;

            Items.Add(drawable);
        }

        public virtual Drawable GetDrawableListItem() => this;

        public void UpdateItem()
        {
            ItemMap.Values.ForEach(item =>
            {
                if (item is IRearrangableDrawableListItem<T> rearrangableItem)
                {
                    rearrangableItem.ApplyAll = ApplyAll;
                    rearrangableItem.GetName = getName;
                    rearrangableItem.OnDragAction = OnDragAction;
                    rearrangableItem.UpdateItem();
                }
            });
        }

        /// <summary>
        /// Selects obj, if it can be cast to a IRearrangableListItem.
        /// </summary>
        /// <param name="obj">the object to call Select on</param>
        /// <param name="value">The value to pass to the Select call of IRearrangableListItem</param>
        /// <returns>If Select was actually called</returns>
        private static bool select(object obj, bool value)
        {
            if (obj is IRearrangableDrawableListItem<T> item)
            {
                item.Select(value);
                return true;
            }

            return false;
        }

        public bool Select(T drawable, bool value = true)
        {
            if (!ItemMap.ContainsKey(drawable)) return false;

            return select(ItemMap[drawable], value);
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

        public void SelectInternal(bool value) => Select(value);

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override RearrangeableListItem<T> CreateDrawable(T item)
        {
            var drawable = new DrawableListItem<T>(item);
            drawable.ApplyAll = ApplyAll;
            drawable.GetName = getName;
            drawable.OnDragAction = OnDragAction;
            // drawable.UpdateItem();
            return drawable.GetRearrangeableListItem();
        }

        private void applyAll(Action<IDrawableListItem<T>> action)
        {
            for (int i = 0; i < ListContainer.Children.Count; i++)
            {
                if (ListContainer.Children[i] is IDrawableListItem<T> item) item.ApplyAction(action);
            }
        }

        #region IReadOnlyList<T>

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Items).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public T this[int index] => Items[index];

        #endregion

        #region ICollection<T> and IContainerCollection<T> IContainerEnumerable<T>

        public int Count => ItemMap.Count;
        public bool IsReadOnly => false;

        public void Clear() => Items.RemoveAll(_ => true);

        public bool Contains(T item) => Items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        public void Add(T? drawable)
        {
            addInternal(drawable);
            OnDragAction();
        }

        public void AddRange(IEnumerable<T>? drawables)
        {
            drawables.ForEach(addInternal);
            OnDragAction();
        }

        public bool Remove(T? drawable, bool disposeImmediately)
        {
            if (drawable is null || !Items.Contains(drawable)) return false;

            bool remove = Items.Remove(drawable);
            if (disposeImmediately) drawable.Dispose();
            return remove;
        }

        public int RemoveAll(Predicate<T> predicate, bool disposeImmediately)
        {
            int count = 0;

            foreach (T item in Items)
            {
                if (predicate.Invoke(item))
                {
                    count++;
                    Remove(item, disposeImmediately);
                }
            }

            return count;
        }

        public void RemoveRange(IEnumerable<T> range, bool disposeImmediately)
        {
            IEnumerator<T> rangeEnumerator = range.GetEnumerator();

            while (rangeEnumerator.MoveNext())
            {
                Remove(rangeEnumerator.Current, disposeImmediately);
            }

            rangeEnumerator.Dispose();
        }

        public IReadOnlyList<T> Children
        {
            get => this;
            set
            {
                Clear();
                AddRange(value);
            }
        }

        public T Child
        {
            set
            {
                Clear();
                Add(value);
            }
        }

        public IEnumerable<T> ChildrenEnumerable
        {
            set
            {
                Clear();
                AddRange(value);
            }
        }

        public bool Remove(T? drawable) => RemoveInternal(drawable, false);

        #endregion

        #region IContainer

        EdgeEffectParameters IContainer.EdgeEffect { get => EdgeEffect; set => EdgeEffect = value; }
        Vector2 IContainer.RelativeChildSize { get => RelativeChildSize; set => RelativeChildSize = value; }
        Vector2 IContainer.RelativeChildOffset { get => RelativeChildOffset; set => RelativeChildOffset = value; }

        #endregion
    }
}
