// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class ImportFromStablePopup : PopupDialog
    {
        public ImportFromStablePopup(Action importFromStable)
        {
            HeaderText = @"You have no beatmaps!";
            BodyText = "An existing copy of osu! was found, though.\nWould you like to import your beatmaps (and skins)?";

            Icon = FontAwesome.fa_plane;

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
