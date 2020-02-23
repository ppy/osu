// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.Menus;
using osu.Game.Screens.Edit.Design;
using osuTK.Input;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.Edit
{
    [Cached(typeof(IBeatSnapProvider))]
    public class Editor : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>, IBeatSnapProvider
    {
        public override float BackgroundParallaxAmount => 0.1f;

        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool AllowRateAdjustments => false;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private Box bottomBackground;
        private Container screenContainer;

        private EditorScreen currentScreen;

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();
        private EditorClock clock;

        private IBeatmap playableBeatmap;
        private EditorBeatmap editorBeatmap;

        private DependencyContainer dependencies;

        protected override UserActivity InitialActivity => new UserActivity.Editing(Beatmap.Value.BeatmapInfo);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameHost host)
        {
            beatDivisor.Value = Beatmap.Value.BeatmapInfo.BeatDivisor;
            beatDivisor.BindValueChanged(divisor => Beatmap.Value.BeatmapInfo.BeatDivisor = divisor.NewValue);

            // Todo: should probably be done at a DrawableRuleset level to share logic with Player.
            var sourceClock = (IAdjustableClock)Beatmap.Value.Track ?? new StopwatchClock();
            clock = new EditorClock(Beatmap.Value, beatDivisor) { IsCoupled = false };
            clock.ChangeSource(sourceClock);

            dependencies.CacheAs<IFrameBasedClock>(clock);
            dependencies.CacheAs<IAdjustableClock>(clock);

            // todo: remove caching of this and consume via editorBeatmap?
            dependencies.Cache(beatDivisor);

            playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Beatmap.Value.BeatmapInfo.Ruleset);
            AddInternal(editorBeatmap = new EditorBeatmap(playableBeatmap));

            dependencies.CacheAs(editorBeatmap);

            EditorMenuBar menuBar;

            var fileMenuItems = new List<MenuItem>();

            if (RuntimeInfo.IsDesktop)
            {
                fileMenuItems.Add(new EditorMenuItem("Save", MenuItemType.Standard, saveBeatmap));
                fileMenuItems.Add(new EditorMenuItem("Export package", MenuItemType.Standard, exportBeatmap));
                fileMenuItems.Add(new EditorMenuItemSpacer());
            }

            fileMenuItems.Add(new EditorMenuItem("Exit", MenuItemType.Standard, this.Exit));

            AddInternal(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
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
                                    Items = fileMenuItems
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
                }
            });

            menuBar.Mode.ValueChanged += onModeChanged;

            bottomBackground.Colour = colours.Gray2;
        }

        protected override void Update()
        {
            base.Update();
            clock.ProcessFrame();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    seek(e, -1);
                    return true;

                case Key.Right:
                    seek(e, 1);
                    return true;

                case Key.S:
                    if (e.ControlPressed)
                    {
                        saveBeatmap();
                        return true;
                    }

                    break;
            }

            return base.OnKeyDown(e);
        }

        private double scrollAccumulation;

        protected override bool OnScroll(ScrollEvent e)
        {
            scrollAccumulation += (e.ScrollDelta.X + e.ScrollDelta.Y) * (e.IsPrecise ? 0.1 : 1);

            const int precision = 1;

            while (Math.Abs(scrollAccumulation) > precision)
            {
                if (scrollAccumulation > 0)
                    seek(e, -1);
                else
                    seek(e, 1);

                scrollAccumulation = scrollAccumulation < 0 ? Math.Min(0, scrollAccumulation + precision) : Math.Max(0, scrollAccumulation - precision);
            }

            return true;
        }

        public bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                // as we don't want to display the back button, manual handling of exit action is required.
                this.Exit();
                return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            // todo: temporary. we want to be applying dim using the UserDimContainer eventually.
            Background.FadeColour(Color4.DarkGray, 500);

            Background.EnableUserDim.Value = false;
            Background.BlurAmount.Value = 0;

            resetTrack(true);
        }

        public override bool OnExiting(IScreen next)
        {
            Background.FadeColour(Color4.White, 500);
            resetTrack();

            return base.OnExiting(next);
        }

        private void resetTrack(bool seekToStart = false)
        {
            Beatmap.Value.Track?.Stop();

            if (seekToStart)
            {
                double targetTime = 0;

                if (Beatmap.Value.Beatmap.HitObjects.Count > 0)
                {
                    // seek to one beat length before the first hitobject
                    targetTime = Beatmap.Value.Beatmap.HitObjects[0].StartTime;
                    targetTime -= Beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(targetTime).BeatLength;
                }

                clock.Seek(Math.Max(0, targetTime));
            }
        }

        private void onModeChanged(ValueChangedEvent<EditorScreenMode> e)
        {
            currentScreen?.Exit();

            switch (e.NewValue)
            {
                case EditorScreenMode.SongSetup:
                    currentScreen = new SetupScreen();
                    break;

                case EditorScreenMode.Compose:
                    currentScreen = new ComposeScreen();
                    break;

                case EditorScreenMode.Design:
                    currentScreen = new DesignScreen();
                    break;

                case EditorScreenMode.Timing:
                    currentScreen = new TimingScreen();
                    break;
            }

            LoadComponentAsync(currentScreen, screenContainer.Add);
        }

        private void seek(UIEvent e, int direction)
        {
            double amount = e.ShiftPressed ? 2 : 1;

            if (direction < 1)
                clock.SeekBackward(!clock.IsRunning, amount);
            else
                clock.SeekForward(!clock.IsRunning, amount);
        }

        private void saveBeatmap() => beatmapManager.Save(playableBeatmap.BeatmapInfo, editorBeatmap);

        private void exportBeatmap()
        {
            saveBeatmap();
            beatmapManager.Export(Beatmap.Value.BeatmapSetInfo);
        }

        public double SnapTime(double time, double? referenceTime) => editorBeatmap.SnapTime(time, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => editorBeatmap.GetBeatLengthAtTime(referenceTime);

        public int BeatDivisor => beatDivisor.Value;
    }
}
