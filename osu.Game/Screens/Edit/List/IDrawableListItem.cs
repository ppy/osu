// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.List
{
    internal interface IDrawableListItem<T>
        where T : Drawable
    {
        event Action<SelectionState> SelectAll;
        public Drawable GetDrawableListItem();

        public void UpdateText();

        /// <summary>
        /// Selects or Deselects this element. This will also update the referenced item, that is connected to this element.
        /// </summary>
        /// <param name="value">if this List Item should be selected or not</param>
        public void Select(bool value);

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
        public void SelectInternal(bool value);
    }
}
