// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupOsuTextBox : OsuTextBox
    {
        protected override float LeftRightPadding => 15;

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            BorderColour = osuColour.Blue;
        }

        protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText { Text = c.ToString(), Colour = BorderColour, TextSize = CalculatedTextSize };
    }
}
