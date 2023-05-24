// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public partial class ConfirmDiscardChangesDialog : PopupDialog
    {
        /// <summary>
        /// Construct a new discard changes confirmation dialog.
        /// </summary>
        /// <param name="onConfirm">An action to perform on confirmation.</param>
        /// <param name="onCancel">An optional action to perform on cancel.</param>
        public ConfirmDiscardChangesDialog(Action onConfirm, Action? onCancel = null)
        {
            HeaderText = "Are you sure you want to go back?";
            BodyText = "This will discard any unsaved changes";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = @"Yes",
                    Action = onConfirm
                },
                new PopupDialogCancelButton
                {
                    Text = @"No I didn't mean to",
                    Action = onCancel
                },
            };
        }
    }
}
