// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Edit.Components.RadioButtons
{
    public class RadioButton
    {
        /// <summary>
        /// Whether this <see cref="RadioButton"/> is selected.
        /// </summary>
        public readonly BindableBool Selected;

        /// <summary>
        /// The item related to this button.
        /// </summary>
        public string Label;

        /// <summary>
        /// A function which creates a drawable icon to represent this item. If null, a sane default should be used.
        /// </summary>
        public readonly Func<Drawable>? CreateIcon;

        private readonly Action? action;

        public RadioButton(string label, Action? action, Func<Drawable>? createIcon = null)
        {
            Label = label;
            CreateIcon = createIcon;
            this.action = action;
            Selected = new BindableBool();
        }

        /// <summary>
        /// Selects this <see cref="RadioButton"/>.
        /// </summary>
        public void Select()
        {
            if (!Selected.Value)
            {
                Selected.Value = true;
                action?.Invoke();
            }
        }

        /// <summary>
        /// Deselects this <see cref="RadioButton"/>.
        /// </summary>
        public void Deselect() => Selected.Value = false;
    }
}
