// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tournament.Components
{
    public partial class BoardUpdateSuccessDialog : PopupDialog
    {
        public BoardUpdateSuccessDialog(string headerText, string bodyText)
        {
            Icon = FontAwesome.Regular.Sun;
            HeaderText = headerText;
            BodyText = bodyText;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Got that!",
                }
            };
        }
    }
}
