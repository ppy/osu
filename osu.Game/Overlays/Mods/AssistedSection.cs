// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Game.Overlays.Mods;
namespace osu.Game
{
    public class AssistedSection : ModSection
    {
        public ModButton RelaxButton => Buttons[0];
        public ModButton AutopilotButton => Buttons[1];
        public ModButton TargetPracticeButton => Buttons[2];
        public ModButton SpunOutButton => Buttons[3];
        public ModButton AutoplayCinemaButton => Buttons[4];

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Blue;
        }

        public AssistedSection()
        {
            Header = @"Assisted";
            Buttons = new ModButton[]
            {
                new ModButton
                {
                    Mods = new Mod[]
                    {
                        new ModRelax(),
                    },
                },
                new ModButton
                {
                    Mods = new Mod[]
                    {
                        new ModRelax2(),
                    },
                },
                new ModButton
                {
                    Mods = new Mod[]
                    {
                        new ModTarget(),
                    },
                },
                new ModButton
                {
                    Mods = new Mod[]
                    {
                        new ModSpunOut(),
                    },
                },
                new ModButton
                {
                    Mods = new Mod[]
                    {
                        new ModAutoplay(),
                        new ModCinema(),
                    },
                },
            };
        }
    }
}
