// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dialog
{
    /// <summary>
    /// A dialog which confirms a user action.
    /// </summary>
    public partial class ConfirmDialog : PopupDialog
    {
        /// <summary>
        /// Construct a new confirmation dialog.
        /// </summary>
        /// <param name="message">The description of the action to be displayed to the user.</param>
        /// <param name="onConfirm">An action to perform on confirmation.</param>
        /// <param name="onCancel">An optional action to perform on cancel.</param>
        public ConfirmDialog(LocalisableString message, Action onConfirm, Action onCancel = null)
        {
            HeaderText = message;
            BodyText = "Last chance to turn back";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes",
                    Action = onConfirm
                },
                new PopupDialogCancelButton
                {
                    Text = CommonStrings.ButtonsCancel,
                    Action = onCancel
                },
            };
        }
    }
}
