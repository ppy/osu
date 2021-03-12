// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public class ConfirmExitDialog : PopupDialog
    {
        /// <summary>
        /// Construct a new exit confirmation dialog.
        /// </summary>
        /// <param name="onConfirm">An action to perform on confirmation.</param>
        /// <param name="onCancel">An optional action to perform on cancel.</param>
        public ConfirmExitDialog(Action onConfirm, Action onCancel = null)
        {
            HeaderText = "真的要退出吗?";
            BodyText = "这是最后一次确认的机会了";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"再见",
                    Action = onConfirm
                },
                new PopupDialogCancelButton
                {
                    Text = @"再玩一会...",
                    Action = onCancel
                },
            };
        }
    }
}
