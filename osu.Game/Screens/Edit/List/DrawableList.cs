// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableList : IDrawableListItem
    {
        private readonly Dictionary<Drawable, IDrawableListItem> elements = new Dictionary<Drawable, IDrawableListItem>();
        private readonly DrawableListItem item;
        private readonly OsuSpriteText text;
        private readonly GridContainer gridContainer;
        private readonly Container<Drawable> container;

        public DrawableList()
        {
            gridContainer = new GridContainer
            {
                AutoSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize)
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Text = @"v",
                            Padding = new MarginPadding
                            {
                                Right = 5
                            },
                        },
                        item = new DrawableListItem(new Container(), @"SkinnableContainer"),
                    },
                    new Drawable?[]
                    {
                        null,
                        container = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(15),
                        }
                    }
                }
            };
        }

        public void Add(DrawableListItem drawableListItem) => Add((IDrawableListItem)drawableListItem);
        public void Add(DrawableList drawableList) => Add((IDrawableListItem)drawableList);

        internal void Add(IDrawableListItem drawableListItem)
        {
            addInternal(drawableListItem.GetDrawableListItem(), drawableListItem);
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
            container.Add(elements[drawable].GetDrawableListItem());
        }

        public bool Remove(Drawable drawable) => RemoveInternal(drawable, false);

        protected bool RemoveInternal(Drawable drawable, bool disposeImmediately)
        {
            bool remove = container.Remove(elements[drawable].GetDrawableListItem(), disposeImmediately);
            elements.Remove(drawable);
            if (disposeImmediately) drawable.Dispose();
            return remove;
        }

        // private void recalculateGrid()
        // {
        //     // gridContainer.Content = elements.Select(e => new Drawable[]
        //     // {
        //     //     new DrawableListItem(e)
        //     // }).ToArray();
        //     scrollContainer.Content[0] = elements.ToArray();
        //     Dimension[] dim = new Dimension[elements.Count];
        //
        //     for (int i = 0; i < dim.Length; i++)
        //     {
        //         dim[i] = new Dimension(GridSizeMode.Absolute);
        //     }
        //
        //     scrollContainer.RowDimensions = dim;
        //     scrollContainer.Invalidate();
        // }
        public Drawable GetDrawableListItem() => gridContainer;
    }
}
