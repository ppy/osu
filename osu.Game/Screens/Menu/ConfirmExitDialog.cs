// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu
{
    public class ConfirmExitDialog : PopupDialog
    {
        public ConfirmExitDialog(Action confirm, Action cancel)
        {
            HeaderText = "Are you sure you want to exit?";
            BodyText = "Last chance to back out.";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Goodbye",
                    Action = confirm
                },
                new PopupDialogCancelButton
                {
                    Text = @"Just a little more",
                    Action = cancel
                },
            };
        }
    }
}
