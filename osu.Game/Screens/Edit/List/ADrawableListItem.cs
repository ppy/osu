// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.List
{
    public abstract class ADrawableListItem<T> : CompositeDrawable
        where T : Drawable
    {
        public Action<SelectionState> SelectAll;
        protected readonly bool EnableSelection = typeof(T).GetInterfaces().Contains(typeof(IStateful<SelectionState>));

        protected Box SelectionBox { get; private set; } = new Box();
        protected bool Selected { get; private set; }

        protected internal ADrawableListItem()
        {
            SelectAll = SelectableOnStateChanged;
        }

        protected override void LoadComplete()
        {
            SelectionBox.RelativeSizeAxes = Axes.Both;
            SelectionBox.Width = 1f;
            SelectionBox.Height = 1f;
            SelectionBox.Colour = new Colour4(255, 255, 0, 0.25f);

            if (!EnableSelection)
            {
                SelectionBox.RemoveAndDisposeImmediately();
                SelectionBox = new Box();
            }

            SelectionBox.Hide();

            base.LoadComplete();
        }

        public abstract void UpdateText();

        /// <summary>
        /// Selects or Deselects this element. This will also update the referenced item, that is connected to this element.
        /// </summary>
        /// <param name="value">if this List Item should be selected or not</param>
        public abstract void Select(bool value);

        public void SelectableOnStateChanged(SelectionState obj)
        {
            switch (obj)
            {
                case SelectionState.Selected:
                    SelectInternal(true);
                    return;

                case SelectionState.NotSelected:
                    SelectInternal(false);
                    return;
            }
        }

        /// <summary>
        /// Selects or Deselects this element. This will not update the referenced item, in order to prevent call cycles.
        /// </summary>
        /// <param name="value">if this List Item should be selected or not</param>
        public virtual void SelectInternal(bool value)
        {
            if (!EnableSelection) return;

            Selected = !Selected;
            if (value) SelectionBox.Show();
            else SelectionBox.Hide();
        }
    }
}
