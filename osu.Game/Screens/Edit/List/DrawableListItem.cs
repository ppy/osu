// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Screens.Edit.List
{
    //todo: rework this, to be simpler.
    //I should probably just implement IStateful<SelectionState>
    public partial class DrawableListItem<T> : RearrangeableListItem<IDrawableListRepresetedItem<T>>, IRearrangableDrawableListItem<T>
        where T : Drawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        private readonly Box box;
        public bool Selected { get; private set; }

        public Action<T, int> SetItemDepth { get; set; } = IDrawableListItem<T>.DEFAULT_SET_ITEM_DEPTH;
        public Action OnDragAction { get; set; } = () => { };

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

        internal DrawableListItem(IDrawableListRepresetedItem<T> represetedItem, LocalisableString name)
            : base(represetedItem)
        {
            getName = IDrawableListItem<T>.GetDefaultText;
            ApplyAll = e => e(this);
            text.Text = name;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Height = 1f,
                    Colour = new Colour4(255, 255, 0, 0.25f),
                },
                text
            };
            updateText(RepresentedItem);

            if (RepresentedItem is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += selectionState => ((IDrawableListItem<T>)this).SelectInternal(selectionState == SelectionState.Selected);
                SelectInternal(selectable.State == SelectionState.Selected);
            }

            if (!((IDrawableListItem<T>)this).EnableSelection)
            {
                box.RemoveAndDisposeImmediately();
                box = new Box();
            }

            box.Hide();
        }

        public DrawableListItem(IDrawableListRepresetedItem<T> represetedItem)
            //select the name of the from the drawable as it's name, if it is set
            //otherwise use the
            : this(represetedItem, string.Empty)
        {
        }

        //imitate the existing SelectionHandler implementation
        //todo: might want to rework this, to just use the SelectionHandler if possible
        protected override bool OnClick(ClickEvent e)
        {
            Logger.Log("OnClick handler for DrawableListItem triggered");

            // while holding control, we only want to add to selection, not replace an existing selection.
            if (e.ControlPressed && e.Button == MouseButton.Left)
            {
                toggleSelection();
            }
            else
            {
                ApplyAll(element => element.Select(false));
                Select(true);
            }

            base.OnClick(e);
            return true;
        }

        private void toggleSelection()
        {
            if (RepresentedItem is IStateful<SelectionState> stateful)
            {
                Select(stateful.State != SelectionState.Selected);
            }
            else
            {
                Select(!Selected);
            }
        }

        public Drawable GetDrawableListItem() => this;
        public RearrangeableListItem<IDrawableListRepresetedItem<T>> GetRearrangeableListItem() => this;

        public void UpdateItem()
        {
            updateText(RepresentedItem!);
            if (RepresentedItem is IStateful<SelectionState> selectable)
                SelectInternal(selectable.State == SelectionState.Selected);
        }

        private void updateText(T? target)
        {
            if (target is null) return;
            //Set the text to the target's name, if set. Else try and get the name of the class that defined T
            text.Text = getName(target);
        }

        public void Select(bool value)
        {
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = value ? SelectionState.Selected : SelectionState.NotSelected;

            SelectInternal(value);
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action) => action(this);

        public void SelectInternal(bool value)
        {
            if (value)
            {
                Selected = true;
                box.Show();
            }
            else
            {
                Selected = false;
                box.Hide();
            }
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            ApplyAll(element => element.Select(false));
            Select(true);
            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            OnDragAction();
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            OnDragAction();
            base.OnDragEnd(e);
        }

        public T? RepresentedItem => Model.RepresentedItem;
    }
}
