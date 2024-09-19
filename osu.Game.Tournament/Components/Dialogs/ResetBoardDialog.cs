// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components
{
    public partial class ResetBoardDialog : PopupDialog
    {
        public ResetBoardDialog(Action revertAction, Action resetAction)
        {
            HeaderText = @"Warning: Revert";
            BodyText = @"This would reset the board to the initial state, including swaps and chats. Are you sure?";
            Icon = FontAwesome.Solid.Undo;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = @"Yes, revert to the initial state.",
                    Action = () => revertAction()
                },
                new PopupDialogDangerousButton
                {
                    Text = @"Yes, but keep the swaps.",
                    Action = () => resetAction()
                },
                new PopupDialogCancelButton
                {
                    Text = @"I'd rather stay the same.",
                },
            };
        }
    }
}
