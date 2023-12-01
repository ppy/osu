// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class ConfirmAbortDialog : PopupDialog
    {
        public ConfirmAbortDialog(Action onConfirm, Action onCancel)
        {
            HeaderText = "Are you sure you want to abort the match?";

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
