// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class DifficultyReductionSection : ModSection
    {
        protected override Key[] ToggleKeys => new Key[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Green;
            SelectedColour = colours.GreenLight;
        }

        public DifficultyReductionSection()
        {
            Header = @"Gameplay Difficulty Reduction";
        }
    }
}
