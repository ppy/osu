// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Dialog
{
    /// <summary>
    /// A dialog which confirms a user action.
    /// </summary>
    public class ConfirmDialog : PopupDialog
    {
        protected PopupDialogOkButton ButtonConfirm;
        protected PopupDialogCancelButton ButtonCancel;

        /// <summary>
        /// Construct a new dialog.
        /// </summary>
        /// <param name="description">The description of the action to be displayed to the user.</param>
        /// <param name="onConfirm">An action to perform on confirmation.</param>
        /// <param name="onCancel">An optional action to perform on cancel.</param>
        public ConfirmDialog(string description, Action onConfirm, Action onCancel = null)
        {
            HeaderText = $"Are you sure you want to {description}?";
            BodyText = "Last chance to back out.";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                ButtonConfirm = new PopupDialogOkButton
                {
                    Text = @"Yes",
                    Action = onConfirm
                },
                ButtonCancel = new PopupDialogCancelButton
                {
                    Text = @"Cancel",
                    Action = onCancel
                },
            };
        }
    }
}
