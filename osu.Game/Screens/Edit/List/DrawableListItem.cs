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
    public class DrawableListItem<T> : RearrangeableListItem<T>, IRearrangableListItem<T>
        where T : Drawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        private readonly Box box;
        private bool selected;
        private Action<SelectionState> selectAll;

        internal DrawableListItem(T d, LocalisableString name)
            : base(d)
        {
            selectAll = ((IDrawableListItem<T>)this).SelectableOnStateChanged;
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
            updateText(d);

            if (d is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += ((IDrawableListItem<T>)this).SelectableOnStateChanged;
                ((IDrawableListItem<T>)this).SelectableOnStateChanged(selectable.State);
            }

            if (!((IDrawableListItem<T>)this).EnableSelection)
            {
                box.RemoveAndDisposeImmediately();
                box = new Box();
            }

            box.Hide();
        }

        public DrawableListItem(T d)
            //select the name of the from the drawable as it's name, if it is set
            //otherwise use the
            : this(d, string.Empty)
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
                selectAll(SelectionState.NotSelected);
                Select(true);
            }

            base.OnClick(e);
            return true;
        }

        private void toggleSelection()
        {
            if (Model is IStateful<SelectionState> stateful)
            {
                switch (stateful.State)
                {
                    case SelectionState.Selected:
                        Select(false);
                        break;

                    case SelectionState.NotSelected:
                        Select(true);
                        break;
                }
            }
            else
            {
                Select(!selected);
            }
        }

        public Action<SelectionState> SelectAll
        {
            get => selectAll;
            set => selectAll = value;
        }

        public Drawable GetDrawableListItem() => this;
        public RearrangeableListItem<T> GetRearrangeableListItem() => this;

        public void UpdateItem()
        {
            updateText(Model);
            if (Model is IStateful<SelectionState> selectable)
                ((IDrawableListItem<T>)this).SelectableOnStateChanged(selectable.State);
        }

        private void updateText(T target)
        {
            //Set the text to the target's name, if set. Else try and get the name of the class that defined T
            text.Text = target.Name.Equals(string.Empty) ? (target.GetType().DeclaringType ?? target.GetType()).Name : target.Name;
        }

        public void Select(bool value)
        {
            if (Model is IStateful<SelectionState> selectable)
                selectable.State = value ? SelectionState.Selected : SelectionState.NotSelected;

            SelectInternal(value);
        }

        public void SelectInternal(bool value)
        {
            if (value)
            {
                selected = true;
                box.Show();
            }
            else
            {
                selected = false;
                box.Hide();
            }
        }
    }
}
