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
            HeaderText = DialogStrings.ConfirmExitHeaderText;

            Icon = FontAwesome.Solid.ExclamationTriangle;

            if (notifications.HasOngoingOperations)
            {
                var ongoingOperations = notifications.OngoingOperations.ToArray();
                string ongoingOperationsText = ongoingOperations.Take(10).Aggregate(string.Empty, (current, n) => current + $"{n.Text} ({n.Progress:0%})\n");

                LocalisableString ongoingOperationsLocalisableString;

                if (ongoingOperations.Length > 10)
                    ongoingOperationsLocalisableString = DialogStrings.ConfirmExitBodyTextOtherOngoingOperations(ongoingOperationsText, ongoingOperations.Length - 10);
                else
                    ongoingOperationsLocalisableString = DialogStrings.ConfirmExitBodyTextOngoingOperations(ongoingOperationsText);

                BodyText = LocalisableString.Interpolate($"{ongoingOperationsLocalisableString}\n\n{DialogStrings.ConfirmDialogBodyText}");

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
