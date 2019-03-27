// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogButton : DialogButton
    {
        public PopupDialogButton()
        {
            Height = 50;
            BackgroundColour = OsuColour.FromHex(@"150e14");
            TextSize = 18;
        }
    }
}
