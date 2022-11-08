// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableList<T> : ADrawableListItem<T>
        where T : Drawable
    {
        private readonly Dictionary<T, DrawableListItem<T>> elements = new Dictionary<T, DrawableListItem<T>>();
        private readonly List<ADrawableListItem<T>> containers = new List<ADrawableListItem<T>>();
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

        internal void AddContainer(ADrawableListItem<T> drawableList)
        {
            if (containers.Contains(drawableList)) return;

            containers.Add(drawableList);
            Container.Add(drawableList);
        }

        internal void Add(ADrawableListItem<T> drawableListItem)
        {
            if (drawableListItem is DrawableListItem<T> item) Add(item);
            else if (drawableListItem is DrawableList<T> list) AddContainer(list);
            else if (drawableListItem is DrawableContainer<T> container) AddContainer(container);
            //this else is here, so if anyone decides to extend ADrawableListItem in the future, this will at least display their item.
            else AddContainer(drawableListItem);
        }

        public void AddRange(IEnumerable<T>? drawables)
        {
            if (drawables is null) return;

            foreach (T drawable in drawables)
            {
                if (drawable is T t) Add(t);
                else if (drawable is ADrawableListItem<T> item) Add(item);
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
            Container.Add(elements[drawable]);
        }

        public bool Remove(T? drawable) => RemoveInternal(drawable, false);

        protected bool RemoveInternal(T? drawable, bool disposeImmediately)
        {
            if (drawable is null || !elements.ContainsKey(drawable)) return false;

            bool remove = Container.Remove(elements[drawable], disposeImmediately);
            elements.Remove(drawable);
            if (disposeImmediately) drawable.Dispose();
            return remove;
        }

        public override void Show()
        {
            UpdateText();
            base.Show();
        }

        public override void UpdateText()
        {
            foreach (DrawableListItem<T> items in elements.Values)
            {
                items.UpdateText();
            }

            foreach (ADrawableListItem<T> container in containers)
            {
                container.UpdateText();
            }
        }

        public bool Select(T drawable, bool value = true)
        {
            if (!elements.ContainsKey(drawable) || !EnableSelection) return false;

            elements[drawable].Select(value);
            return true;
        }

        public override void Select(bool value)
        {
            if (!EnableSelection) return;

            foreach (var listItem in elements.Values)
            {
                listItem.Select(value);
            }
        }

        public override void SelectInternal(bool value) => Select(value);
    }
}
