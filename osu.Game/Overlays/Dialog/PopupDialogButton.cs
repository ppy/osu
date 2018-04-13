// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
