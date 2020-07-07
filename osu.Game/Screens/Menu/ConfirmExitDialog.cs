// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public class ConfirmExitDialog : PopupDialog
    {
        public ConfirmExitDialog(Action confirm, Action cancel)
        {
            HeaderText = "真的要退出吗?";
            BodyText = "这是最后一次确认的机会了";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"再见",
                    Action = confirm
                },
                new PopupDialogCancelButton
                {
                    Text = @"再玩一会",
                    Action = cancel
                },
            };
        }
    }
}
