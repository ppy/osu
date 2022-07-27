// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public class PromptForDifficultyDeleteDialog : PopupDialog
    {
        public PromptForDifficultyDeleteDialog(Action hide, Action delete, Action cancel)
        {
            HeaderText = "Are you sure you want to delete this difficulty?";

            Icon = FontAwesome.Regular.TrashAlt;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Hide this difficulty instead (recommended)",
                    Action = hide
                },
                new PopupDialogDangerousButton
                {
                    Text = @"Yes, DELETE this difficulty!",
                    Action = delete
                },
                new PopupDialogCancelButton
                {
                    Text = @"Oops, continue editing",
                    Action = cancel
                },
            };
        }
    }
}
