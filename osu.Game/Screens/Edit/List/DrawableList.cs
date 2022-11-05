// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableList : IDrawableListItem
    {
        private readonly Dictionary<Drawable, IDrawableListItem> elements = new Dictionary<Drawable, IDrawableListItem>();
        protected readonly Container<Drawable> Container;

        public DrawableList()
        {
            Container = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(15),
            };
        }

        public void Add(DrawableListItem drawableListItem) => Add((IDrawableListItem)drawableListItem);
        public void Add(DrawableContainer drawableList) => Add((IDrawableListItem)drawableList);

        internal void Add(IDrawableListItem drawableListItem)
        {
            addInternal(drawableListItem.GetDrawableListItem(), drawableListItem);
        }

        public void AddRange(IEnumerable<object> drawables)
        {
            var iter = drawables.GetEnumerator();

            while (iter.MoveNext())
                Add(iter.Current as Drawable);

            iter.Dispose();
        }

        public void Add(Drawable? drawable)
        {
            if (drawable is null) return;

            addInternal(drawable, new DrawableListItem(drawable));
        }

        private void addInternal(Drawable drawable, IDrawableListItem listItem)
        {
            if (elements.ContainsKey(drawable)) return;

            elements.Add(drawable, listItem);
            Container.Add(elements[drawable].GetDrawableListItem());
        }

        public bool Remove(Drawable drawable) => RemoveInternal(drawable, false);

        protected bool RemoveInternal(Drawable drawable, bool disposeImmediately)
        {
            bool remove = Container.Remove(elements[drawable].GetDrawableListItem(), disposeImmediately);
            elements.Remove(drawable);
            if (disposeImmediately) drawable.Dispose();
            return remove;
        }

        public virtual Drawable GetDrawableListItem() => Container;
    }
}
