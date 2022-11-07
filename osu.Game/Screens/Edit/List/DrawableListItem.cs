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
    public class DrawableListItem<T> : CompositeDrawable, IDrawableListItem<T>
        where T : Drawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        private readonly Box box;
        protected readonly WeakReference<T> DrawableReference;
        private bool selected;

        public T? t => getTarget();

        internal DrawableListItem(T d, LocalisableString name)
        {
            DrawableReference = new WeakReference<T>(d);
            text.Text = name;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = new Colour4(255, 255, 0, 0.25f),
                },
                text
            };
            box.Hide();
            updateText(d);

            if (d is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += ((IDrawableListItem<T>)this).SelectableOnStateChanged;
                ((IDrawableListItem<T>)this).SelectableOnStateChanged(selectable.State);
            }
        }

        public DrawableListItem(T d)
            //select the name of the from the drawable as it's name, if it is set
            //otherwise use the
            : this(d, string.Empty)
        {
        }

        public event Action<SelectionState>? SelectAll;

        //imitate the existing SelectionHandler implementation
        //todo: might want to rework this, to just use the SelectionHandler if possible
        protected override bool OnClick(ClickEvent e)
        {
            Logger.Log("OnClick handler for DrawableListItem triggered");

            // while holding control, we only want to add to selection, not replace an existing selection.
            if (e.ControlPressed && e.Button == MouseButton.Left && !selected)
            {
                toggleSelection();
            }
            else
            {
                SelectAll?.Invoke(SelectionState.NotSelected);
                Select(true);
            }

            base.OnClick(e);
            return true;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Logger.Log("OnMouseDown handler for DrawableListItem triggered");

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Logger.Log("OnMouseUp handler for DrawableListItem triggered");

            base.OnMouseUp(e);
        }

        private void toggleSelection()
        {
            if (t is IStateful<SelectionState> stateful)
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

        public Drawable GetDrawableListItem() => this;

        private T? getTarget()
        {
            T? target;
            DrawableReference.TryGetTarget(out target);
            return target;
        }

        public void UpdateText()
        {
            if (t is not null)
                updateText(t);
        }

        private void updateText(T target)
        {
            //Set the text to the target's name, if set. Else try and get the name of the class that defined T
            text.Text = target.Name.Equals(string.Empty) ? (target.GetType().DeclaringType ?? target.GetType()).Name : target.Name;
            box.Width = text.Width;
            box.Height = text.Height;
        }

        public void Select(bool value)
        {
            if (t is IStateful<SelectionState> selectable)
                selectable.State = value ? SelectionState.Selected : SelectionState.NotSelected;

            SelectInternal(value);
        }

        public void SelectInternal(bool value)
        {
            if (value)
            {
                selected = true;
                box.Show();
                box.Width = text.Width;
                box.Height = text.Height;
            }
            else
            {
                selected = false;
                box.Hide();
                box.Width = text.Width;
                box.Height = text.Height;
            }
        }
    }
}
