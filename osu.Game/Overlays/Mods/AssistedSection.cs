// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Game.Overlays.Mods;

namespace osu.Game
{
    public class AssistedSection : ModSection
    {
        public ModButton RelaxButton { get; private set; }
        public ModButton AutopilotButton { get; private set; }
        public ModButton TargetPracticeButton { get; private set; }
        public ModButton SpunOutButton { get; private set; }
        public ModButton AutoplayCinemaButton { get; private set; }

        public ModButton KeyButton { get; private set; }
        public ModButton CoopButton { get; private set; }
        public ModButton RandomButton { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Blue;
            SelectedColour = colours.BlueLight;
        }

        public AssistedSection(PlayMode mode)
        {
            Header = @"Assisted";
            //switch (mode)
            //{
            //    case PlayMode.Osu:
            //        Buttons = new ModButton[]
            //        {
            //            RelaxButton = new ModButton
            //            {
            //                ToggleKey = Key.Z,
            //                Mods = new Mod[]
            //                {
            //                    new ModRelax(),
            //                },
            //            },
            //            AutopilotButton = new ModButton
            //            {
            //                ToggleKey = Key.X,
            //                Mods = new Mod[]
            //                {
            //                    new OsuModAutopilot(),
            //                },
            //            },
            //            TargetPracticeButton = new ModButton
            //            {
            //                ToggleKey = Key.C,
            //                Mods = new Mod[]
            //                {
            //                    new OsuModTarget(),
            //                },
            //            },
            //            SpunOutButton = new ModButton
            //            {
            //                ToggleKey = Key.V,
            //                Mods = new Mod[]
            //                {
            //                    new OsuModSpunOut(),
            //                },
            //            },
            //            AutoplayCinemaButton = new ModButton
            //            {
            //                ToggleKey = Key.B,
            //                Mods = new Mod[]
            //                {
            //                    new ModAutoplay(),
            //                    new ModCinema(),
            //                },
            //            },
            //        };
            //        break;

            //    case PlayMode.Taiko:
            //    case PlayMode.Catch:
            //        Buttons = new ModButton[]
            //        {
            //            RelaxButton = new ModButton
            //            {
            //                ToggleKey = Key.Z,
            //                Mods = new Mod[]
            //                {
            //                    new ModRelax(),
            //                },
            //            },
            //            AutoplayCinemaButton = new ModButton
            //            {
            //                ToggleKey = Key.X,
            //                Mods = new Mod[]
            //                {
            //                    new ModAutoplay(),
            //                    new ModCinema(),
            //                },
            //            },
            //        };
            //        break;

            //    case PlayMode.Mania:
            //        Buttons = new ModButton[]
            //        {
            //            KeyButton = new ModButton
            //            {
            //                ToggleKey = Key.Z,
            //                Mods = new Mod[]
            //                {
            //                    new ManiaModKey4(),
            //                    new ManiaModKey5(),
            //                    new ManiaModKey6(),
            //                    new ManiaModKey7(),
            //                    new ManiaModKey8(),
            //                    new ManiaModKey9(),
            //                    new ManiaModKey1(),
            //                    new ManiaModKey2(),
            //                    new ManiaModKey3(),
            //                },
            //            },
            //            CoopButton = new ModButton
            //            {
            //                ToggleKey = Key.X,
            //                Mods = new Mod[]
            //                {
            //                    new ManiaModKeyCoop(),
            //                },
            //            },
            //            RandomButton = new ModButton
            //            {
            //                ToggleKey = Key.C,
            //                Mods = new Mod[]
            //                {
            //                    new ManiaModRandom(),
            //                },
            //            },
            //            AutoplayCinemaButton = new ModButton
            //            {
            //                ToggleKey = Key.V,
            //                Mods = new Mod[]
            //                {
            //                    new ModAutoplay(),
            //                    new ModCinema(),
            //                },
            //            },
            //        };
            //        break;

            //    default:
            //        throw new NotSupportedException();
            //}
        }
    }
}
