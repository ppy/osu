// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components
{
    public class IPCNotFoundDialog : PopupDialog
    {
        public IPCNotFoundDialog()
        {
            BodyText = "Select a directory that contains an osu! Cutting Edge installation";

            Icon = FontAwesome.Regular.Angry;
            HeaderText = @"This is an invalid IPC Directory!";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Alright.",
                    Action = () => { Expire(); }
                }
            };
        }
    }
}
