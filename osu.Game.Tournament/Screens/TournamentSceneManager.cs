// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osu.Game.Tournament.Screens.Drawings;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Ladder;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.Schedule;
using osu.Game.Tournament.Screens.Showcase;
using osu.Game.Tournament.Screens.TeamIntro;
using osu.Game.Tournament.Screens.TeamWin;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens
{
    public class TournamentSceneManager : OsuScreen
    {
        private ScheduleScreen schedule;
        private LadderManager bracket;
        private MapPoolScreen mapPool;
        private GameplayScreen gameplay;
        private TeamWinScreen winner;
        private TeamIntroScreen teamIntro;
        private DrawingsScreen drawings;
        private Container screens;
        private ShowcaseScreen showcase;
        private VideoSprite video;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, Storage storage)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 200,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Drawings", Action = () => setScreen(drawings) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Showcase", Action = () => setScreen(showcase) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Schedule", Action = () => setScreen(schedule) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Bracket", Action = () => setScreen(bracket) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "TeamIntro", Action = () => setScreen(teamIntro) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "MapPool", Action = () => setScreen(mapPool) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Gameplay", Action = () => setScreen(gameplay) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Win", Action = () => setScreen(winner) },
                            }
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    X = 200,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 16/9f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Size = new Vector2(0.8f, 1),
                    //Masking = true,
                    Children = new Drawable[]
                    {
                        video = new VideoSprite(storage.GetStream("BG Logoless - OWC.m4v"))
                        {
                            Loop = true,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                        },
                        screens = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                schedule = new ScheduleScreen(),
                                bracket = new LadderManager(),
                                showcase = new ShowcaseScreen(),
                                mapPool = new MapPoolScreen(),
                                teamIntro = new TeamIntroScreen(),
                                drawings = new DrawingsScreen(),
                                gameplay = new GameplayScreen(),
                                winner = new TeamWinScreen()
                            }
                        },
                    }
                },
            };

            setScreen(teamIntro);
        }

        private void setScreen(Drawable screen)
        {
            foreach (var s in screens.Children)
            {
                if (s == screen)
                {
                    s.Show();
                    if (s is IProvideVideo)
                        video.FadeOut(200);
                    else
                        video.Show();
                }
                else
                    s.Hide();
            }
        }
    }
}
