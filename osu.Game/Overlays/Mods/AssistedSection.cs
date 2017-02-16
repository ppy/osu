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
        private ModButton relaxButton;
        public ModButton RelaxButton
        {
            get
            {                return relaxButton;
            }
        }

        private ModButton autopilotButton;
        public ModButton AutopilotButton
        {
            get
            {
                return autopilotButton;
            }
        }

        private ModButton targetPracticeButton;
        public ModButton TargetPracticeButton
        {
            get
            {
                return targetPracticeButton;
            }
        }

        private ModButton spunOutButton;
        public ModButton SpunOutButton
        {
            get
            {
                return spunOutButton;
            }
        }

        private ModButton autoplayCinemaButton;
        public ModButton AutoplayCinemaButton
        {
            get
            {
                return autoplayCinemaButton;
            }
        }

        private ModButton keyButton;
        public ModButton KeyButton
        {
            get
            {
                return keyButton;
            }
        }

        private ModButton coopButton;
        public ModButton CoopButton
        {
            get
            {
                return coopButton;
            }
        }

        private ModButton randomButton;
        public ModButton RandomButton
        {
            get
            {
                return randomButton;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Blue;
        }

        public AssistedSection(PlayMode mode)
        {
            Header = @"Assisted";

            switch (mode)
            {
                case PlayMode.Osu:
                    Buttons = new ModButton[]
                    {
                        relaxButton = new ModButton
                        {
                            ToggleKey = Key.Z,
                            Mods = new Mod[]
                            {
                                new ModRelax(),
                            },
                        },
                        autopilotButton = new ModButton
                        {
                            ToggleKey = Key.X,
                            Mods = new Mod[]
                            {
                                new ModAutopilot(),
                            },
                        },
                        targetPracticeButton = new ModButton
                        {
                            ToggleKey = Key.C,
                            Mods = new Mod[]
                            {
                                new ModTarget(),
                            },
                        },
                        spunOutButton = new ModButton
                        {
                            ToggleKey = Key.V,
                            Mods = new Mod[]
                            {
                                new ModSpunOut(),
                            },
                        },
                        autoplayCinemaButton = new ModButton
                        {
                            ToggleKey = Key.B,
                            Mods = new Mod[]
                            {
                                new ModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };
                    break;

                case PlayMode.Taiko:
                case PlayMode.Catch:
                    Buttons = new ModButton[]
                    {
                        relaxButton = new ModButton
                        {
                            ToggleKey = Key.Z,
                            Mods = new Mod[]
                            {
                                new ModRelax(),
                            },
                        },
                        autoplayCinemaButton = new ModButton
                        {
                            ToggleKey = Key.X,
                            Mods = new Mod[]
                            {
                                new ModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };
                    break;

                case PlayMode.Mania:
                    Buttons = new ModButton[]
                    {
                        keyButton = new ModButton
                        {
                            ToggleKey = Key.Z,
                            Mods = new Mod[]
                            {
                                new ModKey4(),
                                new ModKey5(),
                                new ModKey6(),
                                new ModKey7(),
                                new ModKey8(),
                                new ModKey9(),
                                new ModKey1(),
                                new ModKey2(),
                                new ModKey3(),
                            },
                        },
                        coopButton = new ModButton
                        {
                            ToggleKey = Key.X,
                            Mods = new Mod[]
                            {
                                new ModKeyCoop(),
                            },
                        },
                        randomButton = new ModButton
                        {
                            ToggleKey = Key.C,
                            Mods = new Mod[]
                            {
                                new ModRandom(),
                            },
                        },
                        autoplayCinemaButton = new ModButton
                        {
                            ToggleKey = Key.V,
                            Mods = new Mod[]
                            {
                                new ModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
