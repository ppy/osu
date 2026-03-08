// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class SaveRequiredPopupDialog : PopupDialog
    {
        public SaveRequiredPopupDialog(Action saveAndAction)
        {
            HeaderText = DialogStrings.SaveRequiredHeaderText;

            Icon = FontAwesome.Regular.Save;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = DialogStrings.SaveRequiredOkButton,
                    Action = saveAndAction
                },
                new PopupDialogCancelButton
                {
                    Text = DialogStrings.SaveRequiredCancelButton,
                },
            };
        }
    }
}
