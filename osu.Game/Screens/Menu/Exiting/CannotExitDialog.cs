// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Menu.Exiting
{
    public class CannotExitDialog : PopupDialog
    {
        public CannotExitDialog(string reason, Action onAcknowledge)
        {
            HeaderText = "Cannot exit from the game!";
            BodyText = reason + "\nPlease do not attempt force quitting to avoid harming your game data.";

            Icon = FontAwesome.Solid.ExclamationCircle;

            Buttons = new[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Alright.",
                    Action = onAcknowledge
                },
            };
        }
    }
}
