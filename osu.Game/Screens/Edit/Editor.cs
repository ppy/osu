// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Menus;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Screens;
using osu.Game.Screens.Edit.Screens.Compose;
using osu.Game.Screens.Edit.Screens.Design;
using osu.Game.Screens.Edit.Components;

namespace osu.Game.Screens.Edit
{
    public class Editor : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg4");

        public override bool ShowOverlaysOnEnter => false;

        private readonly Box bottomBackground;
        private readonly Container screenContainer;

        private EditorScreen currentScreen;

        public Editor()
        {
            EditorMenuBar menuBar;
            TimeInfoContainer timeInfo;
            SummaryTimeline timeline;
            PlaybackControl playback;

            Children = new[]
            {
                new Container
                {
                    Name = "Screen container",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 40, Bottom = 60 },
                    Child = screenContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true
                    }
                },
                new Container
                {
                    Name = "Top bar",
                    RelativeSizeAxes = Axes.X,
                    Height = 40,
                    Child = menuBar = new EditorMenuBar
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Items = new[]
                        {
                            new MenuItem("File")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Export", MenuItemType.Standard, exportBeatmap),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Exit", MenuItemType.Standard, Exit)
                                }
                            }
                        }
                    }
                },
                new Container
                {
                    Name = "Bottom bar",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 60,
                    Children = new Drawable[]
                    {
                        bottomBackground = new Box { RelativeSizeAxes = Axes.Both },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 5, Horizontal = 10 },
                            Child = new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, 220),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 220)
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Right = 10 },
                                            Child = timeInfo = new TimeInfoContainer { RelativeSizeAxes = Axes.Both },
                                        },
                                        timeline = new SummaryTimeline
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Left = 10 },
                                            Child = playback = new PlaybackControl { RelativeSizeAxes = Axes.Both },
                                        }
                                    },
                                }
                            },
                        }
                    }
                },
            };

            timeInfo.Beatmap.BindTo(Beatmap);
            timeline.Beatmap.BindTo(Beatmap);
            playback.Beatmap.BindTo(Beatmap);
            menuBar.Mode.ValueChanged += onModeChanged;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bottomBackground.Colour = colours.Gray2;
        }

        private void exportBeatmap()
        {
            Beatmap.Value.Save();
        }

        private void onModeChanged(EditorScreenMode mode)
        {
            currentScreen?.Exit();

            switch (mode)
            {
                case EditorScreenMode.Compose:
                    currentScreen = new Compose();
                    break;
                case EditorScreenMode.Design:
                    currentScreen = new Design();
                    break;
                default:
                    currentScreen = new EditorScreen();
                    break;
            }

            currentScreen.Beatmap.BindTo(Beatmap);
            screenContainer.Add(currentScreen);
        }

        protected override void OnResuming(Screen last)
        {
            Beatmap.Value.Track?.Stop();
            base.OnResuming(last);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Background.FadeColour(Color4.DarkGray, 500);
            Beatmap.Value.Track?.Stop();
        }

        protected override bool OnExiting(Screen next)
        {
            Background.FadeColour(Color4.White, 500);
            if (Beatmap.Value.Track != null)
            {
                Beatmap.Value.Track.Tempo.Value = 1;
                Beatmap.Value.Track.Start();
            }
            return base.OnExiting(next);
        }
    }
}
