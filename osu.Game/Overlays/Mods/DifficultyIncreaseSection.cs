// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Game.Overlays.Mods;

namespace osu.Game
{
    public class DifficultyIncreaseSection : ModSection
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
            SelectedColour = colours.YellowLight;
        }

        public DifficultyIncreaseSection()
        {
            Header = @"Gameplay Difficulty Increase";
        }
    }
}
