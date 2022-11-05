// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Edit.List
{
    public interface IDrawableListItem : IEquatable<IDrawableListItem>
    {
        public Drawable GetDrawableListItem();

        /// <summary>
        /// This selects the current item, if it matches drawableListItem.
        /// This method exists so Elements in Lists can be correctly selected.
        /// </summary>
        /// <param name="drawableListItem">item to be selected or deselected</param>
        /// <param name="value">if the item should be selected or deselected</param>
        /// <returns>if the provided item was selected</returns>
        public virtual bool Select(IDrawableListItem drawableListItem, bool value = true)
        {
            bool equals = Equals(drawableListItem);

            if (equals) Select(value);
            return equals;
        }

        /// <summary>
        /// This selects the current item, if it matches drawableListItem.
        /// This method exists so Drawables can be selected.
        /// </summary>
        /// <param name="drawableListItem">item to be selected or deselected</param>
        /// <param name="value">if the item should be selected or deselected</param>
        /// <returns>if the provided item was selected</returns>
        public virtual bool Select(object drawableListItem, bool value = true)
        {
            if (drawableListItem is IDrawableListItem item) return Select(item, value);

            return false;
        }

        /// <summary>
        /// Selects or Deselects this element
        /// </summary>
        /// <param name="value">if this List Item should be selected or not</param>
        public void Select(bool value);
    }
}
