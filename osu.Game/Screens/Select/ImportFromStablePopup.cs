// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class ImportFromStablePopup : PopupDialog
    {
        public ImportFromStablePopup(Action importFromStable)
        {
            HeaderText = @"You have no beatmaps!";
            BodyText = "Would you like to import your beatmaps, skins, collections and scores from an existing osu!stable installation?\nThis will create a second copy of all files on disk.";

            Icon = FontAwesome.Solid.Plane;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes please!",
                    Action = importFromStable
                },
                new PopupDialogCancelButton
                {
                    Text = @"No, I'd like to start from scratch",
                },
            };
        }
    }
}
