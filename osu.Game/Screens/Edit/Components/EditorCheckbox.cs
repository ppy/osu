// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public class EditorCheckbox : OsuCheckbox
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Nub.Margin = new MarginPadding { };
            Nub.AccentColour = colours.Yellow;
            Nub.GlowingAccentColour = colours.YellowLighter;
            Nub.GlowColour = colours.YellowDarker;
        }
    }
}
