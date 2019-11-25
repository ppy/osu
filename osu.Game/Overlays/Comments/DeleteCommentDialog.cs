// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Comments
{
    public class DeleteCommentDialog : PopupDialog
    {
        public DeleteCommentDialog(Action deleteAction, Action onDismiss)
        {
            BodyText = "Just checking...";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = @"Are you sure?";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Go for it.",
                    Action = deleteAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"No! Abort mission!",
                    Action = onDismiss
                },
            };
        }
    }
}
