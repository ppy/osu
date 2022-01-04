// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class PlayerCheckbox : OsuCheckbox
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Nub.AccentColour = colours.Yellow;
            Nub.GlowingAccentColour = colours.YellowLighter;
            Nub.GlowColour = colours.YellowDark;
        }
    }
}
