// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableListItem<T> : AbstractListItem<T>
        where T : Drawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        public LocalisableString Text => text.Text;
        private readonly Box box;
        public bool IsSelected { get; private set; }

        private DrawableList<T>? parentList
        {
            get
            {
                //The immediate parent is a FillFlowContainer. It's parent is the actual list we are intrested in.
                if (Parent?.Parent is DrawableList<T> list) return list;

                return null;
            }
        }

        internal DrawableListItem(DrawableListRepresetedItem<T> represetedItem, DrawableListProperties<T> properties, LocalisableString name)
            : base(represetedItem, properties)
        {
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
                selectable.StateChanged += this.ApplySelectionState;
                this.ApplySelectionState(selectable.State);
            }

            if (!((IDrawableListItem<T>)this).EnableSelection)
            {
                box.RemoveAndDisposeImmediately();
                box = new Box();
            }

            box.Hide();
        }

        internal DrawableListItem(DrawableListRepresetedItem<T> represetedItem, DrawableListProperties<T> properties)
            //select the name of the from the drawable as it's name, if it is set
            //otherwise use the
            : this(represetedItem, properties, string.Empty)
        {
        }

        //imitate the existing SelectionHandler implementation
        protected override bool OnClick(ClickEvent e)
        {
            // while holding control, we only want to add to selection, not replace an existing selection.
            if (e.ControlPressed && e.Button == MouseButton.Left)
            {
                if (IsSelected) Deselect();
                else Select();
            }
            else
            {
                Properties.ApplyAll(static element => element.Deselect());
                Select();
            }

            base.OnClick(e);
            return true;
        }

        public override void UpdateItem()
        {
            updateText(RepresentedItem);

            if (RepresentedItem is IStateful<SelectionState> selectable)
            {
                if (selectable.State == SelectionState.Selected) SelectInternal();
                else DeselectInternal();
            }
        }

        private void updateText(T target)
        {
            //Set the text to the target's name, if set. Else try and get the name of the class that defined T
            Scheduler.Add(() => text.Text = Properties.GetName(target));
        }

        public override void Select()
        {
            if (IsSelected) return;

            SelectInternal();
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = SelectionState.Selected;

            InvokeStateChanged(SelectionState.Selected);
        }

        public override void Deselect()
        {
            if (!IsSelected) return;

            DeselectInternal();
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = SelectionState.NotSelected;

            InvokeStateChanged(SelectionState.NotSelected);
        }

        public override void ApplyAction(Action<IDrawableListItem<T>> action) => action(this);

        public override void SelectInternal(bool invokeChildMethods = true)
        {
            if (IsSelected) return;

            IsSelected = true;
            Scheduler.Add(box.Show);
        }

        public override void DeselectInternal(bool invokeChildMethods = true)
        {
            if (!IsSelected) return;

            IsSelected = false;
            Scheduler.Add(box.Hide);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            Properties.ApplyAll(static element => element.Deselect());
            Select();
            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);
            parentList?.OnDragAction();
            Properties.PostOnDragAction(this);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            parentList?.OnDragAction();
            Properties.PostOnDragAction(this);
        }

        public override SelectionState State
        {
            get => IsSelected ? SelectionState.Selected : SelectionState.NotSelected;
            set
            {
                switch (value)
                {
                    case SelectionState.Selected:
                        Select();
                        break;

                    case SelectionState.NotSelected:
                        Deselect();
                        break;
                }
            }
        }
    }
}
