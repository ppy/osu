using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public class PromptRequestPermissionDialog : PopupDialog
    {
        public PromptRequestPermissionDialog(Action requestPerm)
        {
            HeaderText = "To use your own music and backgrounds, the Storage permission needs to be granted. If you choose to do so, the game will need to restart. Continue?";

            Icon = FontAwesome.Regular.Clipboard;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogCancelButton
                {
                    Text = @"Request Storage access",
                    Action = requestPerm
                },
                new PopupDialogOkButton
                {
                    Text = @"Continue without Storage access"
                }
            };
        }
    }
}
