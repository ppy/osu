// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components
{
    public partial class ResolutionConfirmationDialog : PopupDialog
    {
        public ResolutionConfirmationDialog(Action onCancel)
        {
            Icon = FontAwesome.Solid.QuestionCircle;
            HeaderText = "Keep the current display setting?";
            BodyText = "Revert to the previous one if it doesn't fit.";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Keep changes",
                },
                new PopupDialogCancelButton
                {
                    Text = @"Revert",
                    Action = onCancel,
                },
            };
        }
    }
}
