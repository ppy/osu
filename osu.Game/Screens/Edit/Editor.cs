﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Timing;
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

        protected override bool HideOverlaysOnEnter => true;
        public override bool AllowBeatmapRulesetChange => false;

        private Box bottomBackground;
        private Container screenContainer;

        private EditorScreen currentScreen;

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        private EditorClock clock;

        private DependencyContainer dependencies;
        private GameHost host;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameHost host)
        {
            this.host = host;

            // TODO: should probably be done at a RulesetContainer level to share logic with Player.
            var sourceClock = (IAdjustableClock)Beatmap.Value.Track ?? new StopwatchClock();
            clock = new EditorClock(Beatmap.Value, beatDivisor) { IsCoupled = false };
            clock.ChangeSource(sourceClock);

            dependencies.CacheAs<IFrameBasedClock>(clock);
            dependencies.CacheAs<IAdjustableClock>(clock);
            dependencies.Cache(beatDivisor);

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
                                            Child = new TimeInfoContainer { RelativeSizeAxes = Axes.Both },
                                        },
                                        new SummaryTimeline
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Left = 10 },
                                            Child = new PlaybackControl { RelativeSizeAxes = Axes.Both },
                                        }
                                    },
                                }
                            },
                        }
                    }
                },
            };

            menuBar.Mode.ValueChanged += onModeChanged;

            bottomBackground.Colour = colours.Gray2;
        }

        private void exportBeatmap()
        {
            host.OpenFileExternally(Beatmap.Value.Save());
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

            LoadComponentAsync(currentScreen, screenContainer.Add);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.ScrollDelta.X + e.ScrollDelta.Y > 0)
                clock.SeekBackward(!clock.IsRunning);
            else
                clock.SeekForward(!clock.IsRunning);
            return true;
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
