// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.List
{
    public interface IDrawableListItem<T>
    {
        //Selects all items of all items in connected lists, if the param is true.
        //Else unselects them
        public Action<bool> SelectAll { get; set; }
        public Func<T, LocalisableString> GetName { get; set; }
        public Action OnDragAction { get; set; }
        public bool EnableSelection => typeof(T).GetInterfaces().Contains(typeof(IStateful<SelectionState>));

        public void UpdateItem();

        /// <summary>
        /// Selects or Deselects this element. This will also update the referenced item, that is connected to this element.
        /// </summary>
        /// <param name="value">if this List Item should be selected or not</param>
        public void Select(bool value);

        public LocalisableString GetDefaultText(Drawable target)
        {
            Logger.Log("GetDefaultText with" + target + " target.");
            return target.Name.Equals(string.Empty) ? (target.GetType().DeclaringType ?? target.GetType()).Name : target.Name;
        }

        /// <summary>
        /// Selects or Deselects this element. This will not update the referenced item, in order to prevent call cycles.
        /// </summary>
        /// <param name="value">if this List Item should be selected or not</param>
        public void SelectInternal(bool value);
    }
}
