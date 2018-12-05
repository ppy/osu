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
            
            if (isOsuWebSite(url)) BodyText = $"You are about to open an osu! link in a web browser:\n\n{url}";
            else BodyText = $"Warning! You are about to leave osu! and open the following link in a web browser:\n\n{url}";

            HeaderText = "Just checking...";
            Icon = FontAwesome.fa_warning;

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

        public Boolean isOsuWebSite(string url)
        {
            if (url.StartsWith("http://osu.ppy.sh") ||
            url.StartsWith("https://osu.ppy.sh") ||
            url.StartsWith("osu.ppy.sh"))
                return true;
            else
            {
                return false;
            }
        }
    }
}
