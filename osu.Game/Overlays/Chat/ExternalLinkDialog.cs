// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Chat
{
    public class ExternalLinkDialog : PopupDialog
    {
        public ExternalLinkDialog(string url, Action openExternalLinkAction)
        {
            HeaderText = "你确定吗?";
            BodyText = $"你将要离开osu!并在浏览器中打开下面的链接:\n\n{url}";

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"是的,我确定",
                    Action = openExternalLinkAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"让我再想想> <"
                },
            };
        }
    }
}
