// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    public partial class PromptForSaveDialog : PopupDialog
    {
        public PromptForSaveDialog(Action exit, Action saveAndExit, Action cancel)
        {
            HeaderText = EditorDialogsStrings.SaveDialogHeader;

            Icon = FontAwesome.Regular.Save;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = EditorDialogsStrings.Save,
                    Action = saveAndExit
                },
                new PopupDialogDangerousButton
                {
                    Text = EditorDialogsStrings.ForgetAllChanges,
                    Action = exit
                },
                new PopupDialogCancelButton
                {
                    Text = EditorDialogsStrings.ContinueEditing,
                    Action = cancel
                },
            };
        }
    }
}
