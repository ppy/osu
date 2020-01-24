// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Screens.Edit.Components.RadioButtons
{
    public class RadioButton
    {
        /// <summary>
        /// Whether this <see cref="RadioButton"/> is selected.
        /// </summary>
        /// <returns></returns>
        public readonly BindableBool Selected;

        /// <summary>
        /// The text that should be displayed in this button.
        /// </summary>
        public string Text;

        /// <summary>
        /// The <see cref="Action"/> that should be invoked when this button is selected.
        /// </summary>
        public Action Action;

        public RadioButton(string text, Action action)
        {
            Text = text;
            Action = action;
            Selected = new BindableBool();
        }

        public RadioButton(string text)
            : this(text, null)
        {
            Text = text;
            Action = null;
        }

        /// <summary>
        /// Selects this <see cref="RadioButton"/>.
        /// </summary>
        public void Select() => Selected.Value = true;

        /// <summary>
        /// Deselects this <see cref="RadioButton"/>.
        /// </summary>
        public void Deselect() => Selected.Value = false;
    }
}
