// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;

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
            Icon = FontAwesome.Solid.ExclamationTriangle;
            HeaderText = DialogStrings.ConfirmExitHeaderText;

            if (notifications.HasOngoingOperations)
            {
                var ongoingOperations = notifications.OngoingOperations.ToArray();
                LocalisableString ongoingOperationsText = ongoingOperations.Take(10).Aggregate<ProgressNotification, LocalisableString>(string.Empty, (current, n) =>
                {
                    if (n.Progress > 0)
                        return LocalisableString.Interpolate($"{current}\n{n.Text} ({n.Progress:0%})");

                    return LocalisableString.Interpolate($"{current}\n{n.Text}");
                });

                var dialogText = ongoingOperations.Length > 10
                    ? DialogStrings.ConfirmExitBodyTextOtherOngoingOperations(ongoingOperationsText, ongoingOperations.Length - 10)
                    : DialogStrings.ConfirmExitBodyTextOngoingOperations(ongoingOperationsText);

                BodyText = LocalisableString.Interpolate($"{dialogText}\n\n{DialogStrings.ConfirmDialogBodyText}");
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogDangerousButton
                    {
                        Text = DialogStrings.ConfirmExitOkButton,
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
                BodyText = DialogStrings.ConfirmDialogBodyText;
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = DialogStrings.ConfirmExitOkButton,
                        Action = onConfirm
                    },
                    new PopupDialogCancelButton
                    {
                        Text = DialogStrings.ConfirmExitCancelButton,
                        Action = onCancel
                    },
                };
            }
        }
    }
}
