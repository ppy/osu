// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components.Dialogs
{
    public partial class ResetVideoDialog : PopupDialog
    {
        public ResetVideoDialog(Action resetOneAction, Action resetAllAction)
        {
            HeaderText = @"Reset video settings?";
            BodyText = @"Are you sure to reset these to default?";
            Icon = FontAwesome.Solid.Undo;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = @"Yes, but just reset the selected one.",
                    Action = () => resetOneAction()
                },
                new PopupDialogDangerousButton
                {
                    Text = @"Yes, reset all of them.",
                    Action = () => resetAllAction()
                },
                new PopupDialogCancelButton
                {
                    Text = @"I'd rather stay the same.",
                },
            };
        }
    }
}
