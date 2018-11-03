// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Screens.Drawings;
using osu.Game.Tournament.Screens.Ladder;
using osu.Game.Tournament.Screens.TeamIntro;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseSceneManager : LadderTestCase
    {
        private LadderManager bracket;
        private MapPoolScreen mapPool;
        private TeamIntroScreen teamIntro;
        private DrawingsScreen drawings;
        private Container screens;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.2f,
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
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "TeamIntro", Action = () => setScreen(teamIntro) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "MapPool", Action = () => setScreen(mapPool) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Bracket", Action = () => setScreen(bracket) },
                            }
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    X = 0.2f,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 16/9f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Size = new Vector2(0.8f, 1),
                    //Masking = true,
                    Children = new Drawable[]
                    {
                        new VideoSprite(@"C:\Users\Dean\BG Side Logo - OWC.m4v")
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
                                bracket = new LadderManager(Ladder),
                                mapPool = new MapPoolScreen(Ladder.Groupings.First(g => g.Name == "Finals")),
                                teamIntro = new TeamIntroScreen(Ladder.Teams.First(t => t.Acronym == "USA"), Ladder.Teams.First(t => t.Acronym == "JPN"),
                                    Ladder.Groupings.First(g => g.Name == "Finals")),
                                drawings = new DrawingsScreen()
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
                    s.FadeIn(100);
                else
                    s.FadeOut(100);
            }
        }
    }
}
