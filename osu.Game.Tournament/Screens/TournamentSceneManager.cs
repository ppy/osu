// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
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
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.Showcase;
using osu.Game.Tournament.Screens.TeamIntro;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens
{
    public class TournamentSceneManager : OsuScreen
    {
        private LadderManager bracket;
        private MapPoolScreen mapPool;
        private GameplayScreen gameplay;
        private TeamIntroScreen teamIntro;
        private DrawingsScreen drawings;
        private Container screens;
        private ShowcaseScreen showcase;

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
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Bracket", Action = () => setScreen(bracket) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "TeamIntro", Action = () => setScreen(teamIntro) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "MapPool", Action = () => setScreen(mapPool) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Gameplay", Action = () => setScreen(gameplay) },
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
                        new VideoSprite(storage.GetStream("BG Logoless - OWC.m4v"))
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
                                bracket = new LadderManager(ladder),
                                showcase = new ShowcaseScreen(),
                                mapPool = new MapPoolScreen(ladder.Groupings.First(g => g.Name == "Finals")),
                                teamIntro = new TeamIntroScreen(ladder.Teams.First(t => t.Acronym == "USA"), ladder.Teams.First(t => t.Acronym == "JPN"),
                                    ladder.Groupings.First(g => g.Name == "Finals")),
                                drawings = new DrawingsScreen(),
                                gameplay = new GameplayScreen()
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
