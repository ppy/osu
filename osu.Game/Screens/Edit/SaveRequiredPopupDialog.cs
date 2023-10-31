// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class SaveRequiredPopupDialog : PopupDialog
    {
        public SaveRequiredPopupDialog(string headerText, Action saveAndAction)
        {
            HeaderText = headerText;

            Icon = FontAwesome.Regular.Save;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "Sounds good, let's go!",
                    Action = saveAndAction
                },
                new PopupDialogCancelButton
                {
                    Text = "Oops, continue editing",
                },
            };
        }
    }
}
