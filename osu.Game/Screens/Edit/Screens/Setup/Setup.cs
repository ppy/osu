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
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Screens.Setup
{
    public class Setup : EditorScreen
    {
        public SetupMenuBar MenuBar;
        public EditorScreen CurrentScreen;
        private Header header;
        private readonly Container screenContainer;

        public const float SIZE_X = 1080;
        public const float SIZE_Y = 650;

        public Setup(WorkingBeatmap workingBeatmap)
        {
            Beatmap.Value = workingBeatmap;
            CurrentScreen = new GeneralScreen(workingBeatmap);

            Children = new Drawable[]
            {
                screenContainer = new Container
                {
                    RelativeSizeAxes = Axes.None,
                    CornerRadius = 30,
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(SIZE_X, SIZE_Y),
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
                        header = new Header(CurrentScreen),
                        CurrentScreen,
                    }
                },
            };

            MenuBar.Mode.ValueChanged += onModeChanged;
        }

        private void onModeChanged(SetupScreenMode mode)
        {
            CurrentScreen?.Exit();

            switch (mode)
            {
                case SetupScreenMode.General:
                    CurrentScreen = new GeneralScreen(Beatmap.Value);
                    break;
                case SetupScreenMode.Difficulty:
                    CurrentScreen = new DifficultyScreen();
                    break;
                case SetupScreenMode.Audio:
                    CurrentScreen = new AudioScreen();
                    break;
                case SetupScreenMode.Colours:
                    CurrentScreen = new ColoursScreen();
                    break;
                case SetupScreenMode.Design:
                    CurrentScreen = new DesignScreen();
                    break;
                case SetupScreenMode.Advanced:
                    CurrentScreen = new AdvancedScreen();
                    break;
                default:
                    CurrentScreen = new EditorScreen();
                    break;
            }

            header.UpdateScreen(mode.ToString());
            CurrentScreen.Beatmap.BindTo(Beatmap);
            LoadComponentAsync(CurrentScreen, screenContainer.Add);
        }
    }
}
