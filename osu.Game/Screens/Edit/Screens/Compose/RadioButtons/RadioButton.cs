// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Screens.Edit.Screens.Compose.RadioButtons
{
    public struct RadioButton
    {
        /// <summary>
        /// The text that should be displayed in this button.
        /// </summary>
        public string Text;

        /// <summary>
        /// The <see cref="Action"/> that should be invoked when this button is selected.
        /// </summary>
        public Action Action;

        public RadioButton(string text)
        {
            Text = text;
            Action = null;
        }

        public RadioButton(string text, Action action)
        {
            Text = text;
            Action = action;
        }
    }
}
