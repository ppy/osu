// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Chat
{
    public class ExternalLinkDialog : PopupDialog
    {
        public ExternalLinkDialog(string url, Action openExternalLinkAction)
        {
            BodyText = url;

            Icon = FontAwesome.fa_warning;
            HeaderText = "Confirm opening external link";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Go for it.",
                    Action = openExternalLinkAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"No! Abort mission!"
                },
            };
        }
    }
}
