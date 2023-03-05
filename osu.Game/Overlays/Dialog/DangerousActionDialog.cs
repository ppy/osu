// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Dialog
{
    /// <summary>
    /// Base class for various confirmation dialogs that concern deletion actions.
    /// Differs from <see cref="ConfirmDialog"/> in that the confirmation button is a "dangerous" one
    /// (requires the confirm button to be held).
    /// </summary>
    public abstract partial class DangerousActionDialog : PopupDialog
    {
        /// <summary>
        /// The action which performs the deletion.
        /// </summary>
        protected Action? DangerousAction { get; set; }

        protected DangerousActionDialog()
        {
            HeaderText = DeleteConfirmationDialogStrings.HeaderText;

            Icon = FontAwesome.Regular.TrashAlt;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = DeleteConfirmationDialogStrings.Confirm,
                    Action = () => DangerousAction?.Invoke()
                },
                new PopupDialogCancelButton
                {
                    Text = DeleteConfirmationDialogStrings.Cancel
                }
            };
        }
    }
}
