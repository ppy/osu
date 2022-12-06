// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.List
{
    public interface IDrawableListItem<T> : IDrawableListRepresetedItem<T>, IDrawable
        where T : Drawable
    {
        public static readonly Action<T, int> DEFAULT_SET_ITEM_DEPTH = (t, d) =>
        {
            if (t is Drawable drawable)
            {
                if (drawable.Parent is Container<Drawable> container)
                {
                    container.ChangeChildDepth(drawable, d);
                    container.Invalidate();
                }
                else
                {
                    try
                    {
                        drawable.Depth = d;
                    }
                    catch (InvalidOperationException) { }
                }
            }
        };

        public DrawableListProperties<T> Properties { get; internal set; }

        public bool EnableSelection => typeof(T).GetInterfaces().Contains(typeof(IStateful<SelectionState>));

        public void UpdateItem();

        public void Select();
        public void Deselect();

        public void ApplyAction(Action<IDrawableListItem<T>> action);

        public static LocalisableString GetDefaultText(Drawable target)
        {
            // Logger.Log("GetDefaultText with" + target + " target.");
            return string.IsNullOrEmpty(target.Name) ? (target.GetType().DeclaringType ?? target.GetType()).Name : target.Name;
        }

        /// <summary>
        /// Selects this element. This will not update the referenced item, in order to prevent call cycles.
        /// </summary>
        public void SelectInternal();

        /// <summary>
        /// Deselects this element. This will not update the referenced item, in order to prevent call cycles.
        /// </summary>
        public void DeselectInternal();
    }

    public static class DrawableListItemInterfaceExtension
    {
        public static void ApplySelectionState<T>(this IDrawableListItem<T> item, SelectionState state)
            where T : Drawable
        {
            switch (state)
            {
                case SelectionState.Selected:
                    item.SelectInternal();
                    break;

                case SelectionState.NotSelected:
                    item.DeselectInternal();
                    break;
            }
        }
    }
}
