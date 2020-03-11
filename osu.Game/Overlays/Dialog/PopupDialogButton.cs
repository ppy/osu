// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogButton : DialogButton
    {
        public PopupDialogButton()
        {
            Height = 50;
            BackgroundColour = Color4Extensions.FromHex(@"150e14");
            TextSize = 18;
        }
    }
}
