// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public partial class ConfirmExitDialog : PopupDialog
    {
        private readonly Action onConfirm;
        private readonly Action? onCancel;

        /// <summary>
        /// Construct a new exit confirmation dialog.
        /// </summary>
        /// <param name="onConfirm">An action to perform on confirmation.</param>
        /// <param name="onCancel">An optional action to perform on cancel.</param>
        public ConfirmExitDialog(Action onConfirm, Action? onCancel = null)
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
        }

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay notifications)
        {
            HeaderText = "Are you sure you want to exit osu!?";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            if (notifications.HasOngoingOperations)
            {
                string text = "There are currently some background operations which will be aborted if you continue:\n\n";

                foreach (var n in notifications.OngoingOperations)
                    text += $"{n.Text} ({n.Progress:0%})\n";

                text += "\nLast chance to turn back";

                BodyText = text;

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogDangerousButton
                    {
                        Text = @"Let me out!",
                        Action = onConfirm
                    },
                    new PopupDialogCancelButton
                    {
                        Text = CommonStrings.Back,
                        Action = onCancel
                    },
                };
            }
            else
            {
                BodyText = "Last chance to turn back";

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
}
