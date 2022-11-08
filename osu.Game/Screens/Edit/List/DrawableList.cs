// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableList<T> : CompositeDrawable, IDrawableListItem<T>
        where T : Drawable
    {
        public event Action<SelectionState> SelectAll;
        private readonly Dictionary<T, DrawableListItem<T>> elements = new Dictionary<T, DrawableListItem<T>>();
        private readonly List<IDrawableListItem<T>> containers = new List<IDrawableListItem<T>>();
        protected readonly Container<Drawable> Container;

        public DrawableList()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = Container = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2.5f),
            };
            SelectAll = ((IDrawableListItem<T>)this).SelectableOnStateChanged;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button == MouseButton.Left)
                SelectAll.Invoke(SelectionState.NotSelected);
            base.OnClick(e);
            return e.Button == MouseButton.Left;
        }

        public void Add(DrawableListItem<T> drawableListItem)
        {
            if (drawableListItem.t is not null) addInternal(drawableListItem.t, drawableListItem);
        }

        public void Add(DrawableContainer<T> container) => AddContainer(container);
        public void Add(DrawableList<T> list) => AddContainer(list);

        internal void AddContainer(IDrawableListItem<T> drawableList)
        {
            if (containers.Contains(drawableList)) return;

            containers.Add(drawableList);
            Container.Add(drawableList.GetDrawableListItem());
        }

        internal void Add(IDrawableListItem<T> drawableListItem)
        {
            if (drawableListItem is DrawableListItem<T> item) Add(item);
            //this should also catch DrawableContainer
            else if (drawableListItem is DrawableList<T> list) AddContainer(list);
            else if (drawableListItem is DrawableContainer<T> container) AddContainer(container);
            //and there should be no other implementors, because IDrawableListItem is internal?
        }

        public void AddRange(IEnumerable<T>? drawables)
        {
            if (drawables is null) return;

            var iter = drawables.GetEnumerator();

            while (iter.MoveNext())
            {
                if (iter.Current is T t) Add(t);
                if (iter.Current is IDrawableListItem<T> item) Add(item);
            }

            iter.Dispose();
        }

        public void Add(T? drawable)
        {
            if (drawable is null) return;

            addInternal(drawable, new DrawableListItem<T>(drawable));
        }

        private void addInternal(T drawable, DrawableListItem<T> listItem)
        {
            if (elements.ContainsKey(drawable)) return;

            elements.Add(drawable, listItem);
            listItem.SelectAll += SelectAll.Invoke;
            Container.Add(elements[drawable].GetDrawableListItem());
        }

        public bool Remove(T? drawable) => RemoveInternal(drawable, false);

        protected bool RemoveInternal(T? drawable, bool disposeImmediately)
        {
            if (drawable is null || !elements.ContainsKey(drawable)) return false;

            bool remove = Container.Remove(elements[drawable].GetDrawableListItem(), disposeImmediately);
            elements.Remove(drawable);
            if (disposeImmediately) drawable.Dispose();
            return remove;
        }

        public virtual Drawable GetDrawableListItem() => this;

        public void UpdateText()
        {
            foreach (DrawableListItem<T> items in elements.Values)
            {
                items.UpdateText();
            }

            foreach (IDrawableListItem<T> container in containers)
            {
                container.UpdateText();
            }
        }

        public bool Select(T drawable, bool value = true)
        {
            if (!elements.ContainsKey(drawable)) return false;

            elements[drawable].Select(value);
            return true;
        }

        public virtual void Select(bool value)
        {
            foreach (var listItem in elements.Values)
            {
                listItem.Select(value);
            }
        }

        public void SelectInternal(bool value) => Select(value);
    }
}
