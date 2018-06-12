// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Edit.Screens.Setup.Screens;
using OpenTK;
using osu.Framework.Configuration;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Framework.Allocation;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup
{
    public class Setup : EditorScreen
    {
        public SetupMenuBar MenuBar;
        private EditorScreen currentScreen = new GeneralScreen();
        private Header header;
        private readonly Container screenContainer;

        public Setup()
        {
            Children = new Drawable[]
            {
                screenContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 30,
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.7f, 0.8f),
                    Children = new[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    CornerRadius = 30,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = OsuColour.FromHex(@"1a2328"),

                                        },
                                        new Triangles
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            ColourLight = OsuColour.FromHex(@"1a2328"),
                                            ColourDark = OsuColour.FromHex(@"232e34"),
                                            TriangleScale = 0.75f,
                                        },
                                    }
                                }
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Margin = new MarginPadding { Top = 50 },
                            Child = MenuBar = new SetupMenuBar
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                        header = new Header(currentScreen),
                        currentScreen,
                    }
                },
            };

            MenuBar.Mode.ValueChanged += onModeChanged;
        }

        private void onModeChanged(SetupScreenMode mode)
        {
            currentScreen?.Exit();

            switch (mode)
            {
                case SetupScreenMode.General:
                    currentScreen = new GeneralScreen();
                    break;
                case SetupScreenMode.Difficulty:
                    currentScreen = new DifficultyScreen();
                    break;
                case SetupScreenMode.Audio:
                    currentScreen = new AudioScreen();
                    break;
                case SetupScreenMode.Colours:
                    currentScreen = new ColoursScreen();
                    break;
                case SetupScreenMode.Design:
                    currentScreen = new DesignScreen();
                    break;
                case SetupScreenMode.Advanced:
                    currentScreen = new AdvancedScreen();
                    break;
                default:
                    currentScreen = new EditorScreen();
                    break;
            }

            header.UpdateScreen(mode.ToString());
            currentScreen.Beatmap.BindTo(Beatmap);
            LoadComponentAsync(currentScreen, screenContainer.Add);
        }
    }
}
