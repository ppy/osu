// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class ImportBeatmapsConfirmationDialog : PopupDialog
    {
        public ImportBeatmapsConfirmationDialog(Action deleteAction)
        {
            BodyText = "This will create new copies of all your beatmaps.";

            Icon = FontAwesome.Solid.Download;
            HeaderText = @"Confirm import of all beatmaps?";
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
                },
            };
        }
    }
}
