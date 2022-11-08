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
        protected Action<SelectionState> SelectAll;

        Action<SelectionState> IDrawableListItem<T>.SelectAll
        {
            get => SelectAll;
            set => SelectAll = value;
        }

        private readonly Dictionary<T, DrawableListItem<T>> elements = new Dictionary<T, DrawableListItem<T>>();
        private readonly List<IDrawableListItem<T>> containers = new List<IDrawableListItem<T>>();
        protected readonly Container<Drawable> Container;

        public DrawableList()
        {
            SelectAll = ((IDrawableListItem<T>)this).SelectableOnStateChanged;
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
                SelectAll(SelectionState.NotSelected);
            base.OnClick(e);
            return e.Button == MouseButton.Left;
        }

        public void Add(DrawableListItem<T> drawableListItem)
        {
            if (drawableListItem.t is not null) addInternal(drawableListItem.t, drawableListItem);
        }

        public void Add(DrawableMinimisableList<T> minimisableList) => AddContainer(minimisableList);
        public void Add(DrawableList<T> list) => AddContainer(list);

        internal void AddContainer(IDrawableListItem<T> drawableList)
        {
            if (containers.Contains(drawableList)) return;

            drawableList.SelectAll = SelectAll;
            containers.Add(drawableList);
            Container.Add(drawableList.GetDrawableListItem());
        }

        internal void Add(IDrawableListItem<T> drawableListItem)
        {
            if (drawableListItem is DrawableListItem<T> item) Add(item);
            //this should also catch DrawableContainer
            else if (drawableListItem is DrawableList<T> list) AddContainer(list);
            else if (drawableListItem is DrawableMinimisableList<T> container) AddContainer(container);
            //and there should be no other implementors, because IDrawableListItem is internal?
            else AddContainer(drawableListItem);
        }

        public void AddRange(IEnumerable<T>? drawables)
        {
            if (drawables is null) return;

            foreach (T drawable in drawables)
            {
                if (drawable is T t) Add(t);
                if (drawable is IDrawableListItem<T> item) Add(item);
            }
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
            listItem.SelectAll = SelectAll;
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

        public void UpdateItem()
        {
            foreach (DrawableListItem<T> items in elements.Values)
            {
                items.UpdateItem();
                items.SelectAll = SelectAll;
            }

            foreach (IDrawableListItem<T> container in containers)
            {
                container.UpdateItem();
                container.SelectAll = SelectAll;
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
