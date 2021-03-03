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
            HeaderText = "Are you sure you want to exit osu!?";
            BodyText = "Last chance to turn back";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Let me out!",
                    Action = onConfirm
                },
                new PopupDialogCancelButton
                {
                    Text = @"Just a little more...",
                    Action = onCancel
                },
            };
        }
    }
}
