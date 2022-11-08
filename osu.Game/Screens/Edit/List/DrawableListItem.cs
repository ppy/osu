// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Graphics;
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
    public class DrawableListItem<T> : ADrawableListItem<T>
        where T : Drawable
    {
        private readonly OsuSpriteText text;
        protected readonly WeakReference<T> DrawableReference;
        public T? t => getTarget();

        internal DrawableListItem(T d, LocalisableString name)
        {
            DrawableReference = new WeakReference<T>(d);
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                SelectionBox,
                text = new OsuSpriteText
                {
                    Text = name,
                }
            };

            updateText(d);

            if (d is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += SelectableOnStateChanged;
                SelectableOnStateChanged(selectable.State);
            }
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
            if (e.ControlPressed && e.Button == MouseButton.Left && !Selected)
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
                Select(!Selected);
            }
        }

        private T? getTarget()
        {
            T? target;
            DrawableReference.TryGetTarget(out target);
            return target;
        }

        public override void UpdateText()
        {
            if (t is not null)
                updateText(t);

            if (t is IStateful<SelectionState> stateful)
                SelectableOnStateChanged(stateful.State);
        }

        private void updateText(T target)
        {
            //Set the text to the target's name, if set. Else try and get the name of the class that defined T
            text.Text = target.Name.Equals(string.Empty) ? (target.GetType().DeclaringType ?? target.GetType()).Name : target.Name;
        }

        public override void Select(bool value)
        {
            if (!EnableSelection) return;

            if (t is IStateful<SelectionState> selectable)
                selectable.State = value ? SelectionState.Selected : SelectionState.NotSelected;

            SelectInternal(value);
        }
    }
}
