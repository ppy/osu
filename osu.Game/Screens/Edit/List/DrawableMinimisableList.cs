// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableMinimisableList<T> : CompositeDrawable, IDrawableListItem<T>
        where T : Drawable
    {
        private Action onDragAction { get; set; }

        public Action OnDragAction
        {
            get => onDragAction;
            set
            {
                onDragAction = value;
                List.OnDragAction = value;
            }
        }

        private Action<SelectionState> selectAll;

        public Action<SelectionState> SelectAll
        {
            get => selectAll;
            set
            {
                selectAll = value;
                List.SelectAll = value;
            }
        }

        private Func<T, LocalisableString> getName;

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                getName = value;
                List.GetName = value;
            }
        }

        private readonly OsuCheckbox button;
        private readonly Box box;
        private readonly BindableBool enabled = new BindableBool();
        public readonly DrawableList<T> List = new DrawableList<T>();

        public DrawableMinimisableList()
        {
            getName = ((IDrawableListItem<T>)this).GetDefaultText;
            selectAll = ((IDrawableListItem<T>)List).SelectableOnStateChanged;
            onDragAction = () => { };

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
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            box = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 1f,
                                Height = 1f,
                                Colour = new Colour4(255, 255, 0, 0.25f),
                            },
                            button = new OsuCheckbox
                            {
                                LabelText = @"SkinnableContainer",
                                Current = enabled
                            },
                        }
                    },
                    List
                }
            };
            List.X = 10f;
            List.RelativeAnchorPosition = new Vector2(0.05f, 0);

            if (!((IDrawableListItem<T>)this).EnableSelection)
            {
                box.RemoveAndDisposeImmediately();
                box = new Box();
            }

            box.Hide();
            enabled.BindValueChanged(v => SetShown(v.NewValue), true);
            Select(false);
        }

        public void SelectableOnStateChanged(SelectionState obj) =>
            ((IDrawableListItem<T>)List).SelectableOnStateChanged(obj);

        public void Toggle() => SetShown(!enabled.Value, true);

        public void SetShown(bool value, bool setValue = false)
        {
            if (value) List.Show();
            else List.Hide();
            UpdateItem();
            if (setValue) enabled.Value = value;
        }

        public void UpdateItem()
        {
            List.GetName = GetName;
            List.SelectAll = SelectAll;
            List.UpdateItem();
        }

        public void Select(bool value)
        {
            if (value) box.Show();
            else box.Hide();
            List.Select(value);
        }

        public void AddRange(IEnumerable<T>? drawables) => List.AddRange(drawables);

        // public void Add(DrawableListItem<T> drawableListItem) => List.Add(drawableListItem);
        // public void Add(DrawableMinimisableList<T> minimisableList) => List.Add(minimisableList);
        // public void Add(DrawableList<T> list) => list.Add(list);
        public void Add(T? item) => List.Add(item);
        public void Remove(T? item) => List.Remove(item);

        public void SelectInternal(bool value) => List.SelectInternal(value);

        public Drawable GetDrawableListItem() => this;
    }
}
