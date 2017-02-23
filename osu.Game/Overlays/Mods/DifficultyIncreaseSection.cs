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
        public ModButton HardRockButton => Buttons[0];
        public ModButton SuddenDeathButton => Buttons[1];
        public ModButton DoubleTimeNightcoreButton => Buttons[2];
        public ModButton HiddenButton => Buttons[3];
        public ModButton FlashlightButton => Buttons[4];

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
            SelectedColour = colours.YellowLight;
        }

        public DifficultyIncreaseSection()
        {
            Header = @"Gameplay Difficulty Increase";
            Buttons = new ModButton[]
            {
                new ModButton
                {
                    ToggleKey = Key.A,
                    Mods = new Mod[]
                    {
                        new ModHardRock(),
                    },
                },
                new ModButton
                {
                    ToggleKey = Key.S,
                    Mods = new Mod[]
                    {
                        new ModSuddenDeath(),
                        new ModPerfect(),
                    },
                },
                new ModButton
                {
                    ToggleKey = Key.D,
                    Mods = new Mod[]
                    {
                        new ModDoubleTime(),
                        new ModNightcore(),
                    },
                },
                new ModButton
                {
                    ToggleKey = Key.F,
                    Mods = new Mod[]
                    {
                        new ModHidden(),
                    },
                },
                new ModButton
                {
                    ToggleKey = Key.G,
                    Mods = new Mod[]
                    {
                        new ModFlashlight(),
                    },
                },
            };
        }
    }
}
