// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Game.Overlays.Mods;

namespace osu.Game
{
    public class DifficultyReductionSection : ModSection
    {
        public ModButton EasyButton => Buttons[0];
        public ModButton NoFailButton => Buttons[1];
        public ModButton HalfTimeButton => Buttons[2];

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Green;
            SelectedColour = colours.GreenLight;
        }

        public DifficultyReductionSection()
        {
            Header = @"Gameplay Difficulty Reduction";
            Buttons = new ModButton[]
            {
                new ModButton
                {
                    ToggleKey = Key.Q,
                    Mods = new Mod[]
                    {
                        new ModEasy(),
                    },
                },
                new ModButton
                {
                    ToggleKey = Key.W,
                    Mods = new Mod[]
                    {
                        new ModNoFail(),
                    },
                },
                new ModButton
                {
                    ToggleKey = Key.E,
                    Mods = new Mod[]
                    {
                        new ModHalfTime(),
                    },
                },
            };
        }
    }
}
