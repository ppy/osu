// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableContainer<T> : ADrawableListItem<T>
        where T : Drawable
    {
        private readonly BindableBool enabled = new BindableBool(true);
        private readonly DrawableList<T> list = new DrawableList<T>();

        public DrawableContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
                Children = new Drawable[]
                {
                    SelectionBox,
                    new OsuCheckbox
                    {
                        LabelText = @"SkinnableContainer",
                        Current = enabled
                    },
                    new GridContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 10),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                null,
                                list,
                            }
                        }
                    }
                }
            };

            enabled.BindValueChanged(v => SetShown(v.NewValue), true);
            Select(false);
        }

        public void Toggle() => SetShown(!enabled.Value, true);

        public void SetShown(bool value, bool setValue = false)
        {
            if (value) list.Show();
            else list.Hide();

            if (setValue) enabled.Value = value;
        }

        public override void UpdateText() => list.UpdateText();

        public override void Select(bool value)
        {
            base.SelectInternal(value);
            list.Select(value);
        }

        public void AddRange(IEnumerable<T>? drawables) => list.AddRange(drawables);
        public void Add(DrawableListItem<T> drawableListItem) => list.Add(drawableListItem);
        public void Add(DrawableContainer<T> container) => list.Add(container);
        public void Add(DrawableList<T> list) => list.Add(list);
        public void Add(T? item) => list.Add(item);
        public void Remove(T? item) => list.Remove(item);
        public bool Select(T drawable, bool select) => list.Select(drawable, select);

        public override void SelectInternal(bool value)
        {
            base.SelectInternal(value);
            list.SelectInternal(value);
        }
    }
}
