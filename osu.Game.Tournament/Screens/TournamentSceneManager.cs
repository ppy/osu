// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Drawings;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Groupings;
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
    [Cached]
    public class TournamentSceneManager : OsuScreen
    {
        private Container screens;
        private VideoSprite video;

        //todo: make less temporary
        [Cached]
        private MatchChatDisplay chat = new MatchChatDisplay
        {
            RelativeSizeAxes = Axes.X,
            Y = 100,
            Size = new Vector2(0.45f, 112),
            Margin = new MarginPadding(10),
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
        };

        private Container chatContainer;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, Storage storage)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    X = 200,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 16 / 9f,
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
                                new ScheduleScreen(),
                                new LadderScreen(),
                                new LadderEditorScreen(),
                                new GroupingsEditorScreen(),
                                new ShowcaseScreen(),
                                new MapPoolScreen(),
                                new TeamIntroScreen(),
                                new DrawingsScreen(),
                                new GameplayScreen(),
                                new TeamWinScreen()
                            }
                        },
                        chatContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = chat
                        },
                    }
                },
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
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Bracket Editor", Action = () => SetScreen(typeof(LadderEditorScreen)) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Groupings Editor", Action = () => SetScreen(typeof(GroupingsEditorScreen)) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Drawings", Action = () => SetScreen(typeof(DrawingsScreen)) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Showcase", Action = () => SetScreen(typeof(ShowcaseScreen)) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Schedule", Action = () => SetScreen(typeof(ScheduleScreen)) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Bracket", Action = () => SetScreen(typeof(LadderScreen)) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "TeamIntro", Action = () => SetScreen(typeof(TeamIntroScreen)) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "MapPool", Action = () => SetScreen(typeof(MapPoolScreen)) },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Gameplay", Action = () => SetScreen(typeof(GameplayScreen)) },
                                new Container { RelativeSizeAxes = Axes.X, Height = 50 },
                                new OsuButton { RelativeSizeAxes = Axes.X, Text = "Win", Action = () => SetScreen(typeof(TeamWinScreen)) },
                            }
                        },
                    },
                },
            };

            SetScreen(typeof(ScheduleScreen));
        }

        public void SetScreen(Type screenType)
        {
            var screen = screens.FirstOrDefault(s => s.GetType() == screenType);
            if (screen == null) return;

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

            switch (screen)
            {
                case GameplayScreen _:
                case MapPoolScreen _:
                    chatContainer.FadeIn(100);
                    break;
                default:
                    chatContainer.FadeOut(100);
                    break;
            }
        }
    }
}
