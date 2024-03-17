// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Edit
{
    public abstract partial class EditorTable<T> : TableContainer
        where T : class
    {
        public event Action<EditorTableBackgroundRow>? OnRowSelected;

        private const float horizontal_inset = 20;

        public const int TEXT_SIZE = 14;

        private readonly List<T> items = new List<T>();

        public IEnumerable<T> Items
        {
            set => SetNewItems(value);
        }

        protected virtual void SetNewItems(IEnumerable<T> newItems)
        {
            Content = null;
            items.Clear();
            items.AddRange(newItems);

            background.RowCount = items.Count;
        }

        private readonly EditorTableBackground background;

        protected EditorTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, EditorTableBackground.ROW_HEIGHT);

            AddInternal(new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Y = EditorTableBackground.ROW_HEIGHT,
                Depth = 1f,
                Padding = new MarginPadding { Horizontal = -horizontal_inset },
                Child = background = new EditorTableBackground
                {
                    RelativeSizeAxes = Axes.X
                }
            });

            background.Selected += index => OnItemSelected(items[index]);
        }

        protected virtual void OnItemSelected(T item)
        {
        }

        // We can avoid potentially thousands of objects being added to the input sub-tree since input is being handled only by the BackgroundFlow anyway.
        protected override bool ShouldBeConsideredForInput(Drawable child) => child is not GridContainer && base.ShouldBeConsideredForInput(child);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => background.ReceivePositionalInputAt(screenSpacePos);

        protected int GetIndexForItem(T? item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == item)
                    return i;
            }

            return -1;
        }

        protected virtual bool SetSelectedRow(T? item)
        {
            bool foundSelection = false;

            for (int i = 0; i < items.Count; i++)
            {
                if (ReferenceEquals(items[i], item))
                {
                    Debug.Assert(!foundSelection);
                    OnRowSelected?.Invoke(background.Select(i));
                    foundSelection = true;
                }
            }

            if (!foundSelection)
                background.Deselect();

            return foundSelection;
        }

        protected T? GetItemAtIndex(int index)
        {
            if (index < 0 || index > items.Count - 1)
                return null;

            return items[index];
        }

        protected override Drawable CreateHeader(int index, TableColumn? column) => new HeaderText(column?.Header ?? default);

        private partial class HeaderText : OsuSpriteText
        {
            public HeaderText(LocalisableString text)
            {
                Text = text.ToUpper();
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold);
            }
        }
    }
}
