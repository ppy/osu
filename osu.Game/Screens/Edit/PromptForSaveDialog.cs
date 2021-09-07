// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public class PromptForSaveDialog : PopupDialog
    {
        public PromptForSaveDialog(Action exit, Action saveAndExit, Action cancel)
        {
            HeaderText = "你要保存你的更改吗?";

            Icon = FontAwesome.Regular.Save;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogCancelButton
                {
                    Text = @"是的，保存。",
                    Action = saveAndExit
                },
                new PopupDialogOkButton
                {
                    Text = @"忘了这事吧，不保存。",
                    Action = exit
                },
                new PopupDialogCancelButton
                {
                    Text = @"我点错了，继续编辑。",
                    Action = cancel
                },
            };
        }
    }
}
