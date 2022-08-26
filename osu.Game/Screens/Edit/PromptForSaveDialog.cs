// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public class PromptForSaveDialog : PopupDialog
    {
        public PromptForSaveDialog(Action exit, Action saveAndExit, Action cancel)
        {
            HeaderText = "Did you want to save your changes?";

            Icon = FontAwesome.Regular.Save;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Save my masterpiece!",
                    Action = saveAndExit
                },
                new PopupDialogDangerousButton
                {
                    Text = @"Forget all changes",
                    Action = exit
                },
                new PopupDialogCancelButton
                {
                    Text = @"Oops, continue editing",
                    Action = cancel
                },
            };
        }
    }
}
