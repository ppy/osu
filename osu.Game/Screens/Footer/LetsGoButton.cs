// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Footer
{
    public partial class LetsGoButton : ShearedButton
    {
        public LetsGoButton()
            : base(400)
        {
            LighterColour = Colour4.FromHex("#FFFFFF");
            DarkerColour = Colour4.FromHex("#FFCC22");
            TextColour = Colour4.Black;
            TextSize = 30;
            TextWeight = FontWeight.Light;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Text = "Let's GO!";
        }
    }
}
