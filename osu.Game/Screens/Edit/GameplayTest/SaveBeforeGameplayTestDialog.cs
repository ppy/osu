// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public partial class SaveBeforeGameplayTestDialog : PopupDialog
    {
        public SaveBeforeGameplayTestDialog(Action saveAndPreview)
        {
            HeaderText = "The beatmap will be saved in order to test it.";

            Icon = FontAwesome.Regular.Save;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = "Sounds good, let's go!",
                    Action = saveAndPreview
                },
                new PopupDialogCancelButton
                {
                    Text = "Oops, continue editing",
                },
            };
        }
    }
}
