// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.IO.Serialization;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.Menus;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Design;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit
{
    [Cached(typeof(IBeatSnapProvider))]
    public class Editor : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>, IKeyBindingHandler<PlatformAction>, IBeatSnapProvider
    {
        public override float BackgroundParallaxAmount => 0.1f;

        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool AllowRateAdjustments => false;

        protected bool HasUnsavedChanges => lastSavedHash != changeHandler.CurrentStateHash;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        private bool exitConfirmed;

        private string lastSavedHash;

        private Box bottomBackground;
        private Container screenContainer;

        private EditorScreen currentScreen;

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();
        private EditorClock clock;

        private IBeatmap playableBeatmap;
        private EditorBeatmap editorBeatmap;
        private EditorChangeHandler changeHandler;

        private EditorMenuBar menuBar;

        private DependencyContainer dependencies;

        protected override UserActivity InitialActivity => new UserActivity.Editing(Beatmap.Value.BeatmapInfo);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [Resolved]
        private IAPIProvider api { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameHost host)
        {
            beatDivisor.Value = Beatmap.Value.BeatmapInfo.BeatDivisor;
            beatDivisor.BindValueChanged(divisor => Beatmap.Value.BeatmapInfo.BeatDivisor = divisor.NewValue);

            // Todo: should probably be done at a DrawableRuleset level to share logic with Player.
            var sourceClock = (IAdjustableClock)Beatmap.Value.Track ?? new StopwatchClock();
            clock = new EditorClock(Beatmap.Value, beatDivisor) { IsCoupled = false };
            clock.ChangeSource(sourceClock);

            dependencies.CacheAs(clock);
            AddInternal(clock);

            // todo: remove caching of this and consume via editorBeatmap?
            dependencies.Cache(beatDivisor);

            bool isNewBeatmap = false;

            if (Beatmap.Value is DummyWorkingBeatmap)
            {
                isNewBeatmap = true;
                Beatmap.Value = beatmapManager.CreateNew(Ruleset.Value, api.LocalUser.Value);
            }

            try
            {
                playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Beatmap.Value.BeatmapInfo.Ruleset);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap successfully!");
                // couldn't load, hard abort!
                this.Exit();
                return;
            }

            AddInternal(editorBeatmap = new EditorBeatmap(playableBeatmap, Beatmap.Value.Skin));
            dependencies.CacheAs(editorBeatmap);
            changeHandler = new EditorChangeHandler(editorBeatmap);
            dependencies.CacheAs<IEditorChangeHandler>(changeHandler);

            updateLastSavedHash();

            OsuMenuItem undoMenuItem;
            OsuMenuItem redoMenuItem;

            EditorMenuItem cutMenuItem;
            EditorMenuItem copyMenuItem;
            EditorMenuItem pasteMenuItem;

            var fileMenuItems = new List<MenuItem>
            {
                new EditorMenuItem("Save", MenuItemType.Standard, Save)
            };

            if (RuntimeInfo.IsDesktop)
                fileMenuItems.Add(new EditorMenuItem("Export package", MenuItemType.Standard, exportBeatmap));

            fileMenuItems.Add(new EditorMenuItemSpacer());
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
                            Mode = { Value = isNewBeatmap ? EditorScreenMode.SongSetup : EditorScreenMode.Compose },
                            Items = new[]
                            {
                                new MenuItem("File")
                                {
                                    Items = fileMenuItems
                                },
                                new MenuItem("Edit")
                                {
                                    Items = new[]
                                    {
                                        undoMenuItem = new EditorMenuItem("Undo", MenuItemType.Standard, Undo),
                                        redoMenuItem = new EditorMenuItem("Redo", MenuItemType.Standard, Redo),
                                        new EditorMenuItemSpacer(),
                                        cutMenuItem = new EditorMenuItem("Cut", MenuItemType.Standard, Cut),
                                        copyMenuItem = new EditorMenuItem("Copy", MenuItemType.Standard, Copy),
                                        pasteMenuItem = new EditorMenuItem("Paste", MenuItemType.Standard, Paste),
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
                }
            });

            changeHandler.CanUndo.BindValueChanged(v => undoMenuItem.Action.Disabled = !v.NewValue, true);
            changeHandler.CanRedo.BindValueChanged(v => redoMenuItem.Action.Disabled = !v.NewValue, true);

            editorBeatmap.SelectedHitObjects.BindCollectionChanged((_, __) =>
            {
                var hasObjects = editorBeatmap.SelectedHitObjects.Count > 0;

                cutMenuItem.Action.Disabled = !hasObjects;
                copyMenuItem.Action.Disabled = !hasObjects;
            }, true);

            clipboard.BindValueChanged(content => pasteMenuItem.Action.Disabled = string.IsNullOrEmpty(content.NewValue));

            menuBar.Mode.ValueChanged += onModeChanged;

            bottomBackground.Colour = colours.Gray2;
        }

        protected void Save()
        {
            // apply any set-level metadata changes.
            beatmapManager.Update(playableBeatmap.BeatmapInfo.BeatmapSet);

            // save the loaded beatmap's data stream.
            beatmapManager.Save(playableBeatmap.BeatmapInfo, editorBeatmap, editorBeatmap.BeatmapSkin);

            updateLastSavedHash();
        }

        protected override void Update()
        {
            base.Update();
            clock.ProcessFrame();
        }

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionType)
            {
                case PlatformActionType.Cut:
                    Cut();
                    return true;

                case PlatformActionType.Copy:
                    Copy();
                    return true;

                case PlatformActionType.Paste:
                    Paste();
                    return true;

                case PlatformActionType.Undo:
                    Undo();
                    return true;

                case PlatformActionType.Redo:
                    Redo();
                    return true;

                case PlatformActionType.Save:
                    Save();
                    return true;
            }

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
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
            }

            return base.OnKeyDown(e);
        }

        private double scrollAccumulation;

        protected override bool OnScroll(ScrollEvent e)
        {
            const double precision = 1;

            double scrollComponent = e.ScrollDelta.X + e.ScrollDelta.Y;

            double scrollDirection = Math.Sign(scrollComponent);

            // this is a special case to handle the "pivot" scenario.
            // if we are precise scrolling in one direction then change our mind and scroll backwards,
            // the existing accumulation should be applied in the inverse direction to maintain responsiveness.
            if (scrollAccumulation != 0 && Math.Sign(scrollAccumulation) != scrollDirection)
                scrollAccumulation = scrollDirection * (precision - Math.Abs(scrollAccumulation));

            scrollAccumulation += scrollComponent * (e.IsPrecise ? 0.1 : 1);

            // because we are doing snapped seeking, we need to add up precise scrolls until they accumulate to an arbitrary cut-off.
            while (Math.Abs(scrollAccumulation) >= precision)
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
            switch (action)
            {
                case GlobalAction.Back:
                    // as we don't want to display the back button, manual handling of exit action is required.
                    this.Exit();
                    return true;

                case GlobalAction.EditorComposeMode:
                    menuBar.Mode.Value = EditorScreenMode.Compose;
                    return true;

                case GlobalAction.EditorDesignMode:
                    menuBar.Mode.Value = EditorScreenMode.Design;
                    return true;

                case GlobalAction.EditorTimingMode:
                    menuBar.Mode.Value = EditorScreenMode.Timing;
                    return true;

                case GlobalAction.EditorSetupMode:
                    menuBar.Mode.Value = EditorScreenMode.SongSetup;
                    return true;

                default:
                    return false;
            }
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
            if (!exitConfirmed && dialogOverlay != null && HasUnsavedChanges && !(dialogOverlay.CurrentDialog is PromptForSaveDialog))
            {
                dialogOverlay?.Push(new PromptForSaveDialog(confirmExit, confirmExitWithSave));
                return true;
            }

            Background.FadeColour(Color4.White, 500);
            resetTrack();

            return base.OnExiting(next);
        }

        private void confirmExitWithSave()
        {
            exitConfirmed = true;
            Save();
            this.Exit();
        }

        private void confirmExit()
        {
            exitConfirmed = true;
            this.Exit();
        }

        private readonly Bindable<string> clipboard = new Bindable<string>();

        protected void Cut()
        {
            Copy();
            foreach (var h in editorBeatmap.SelectedHitObjects.ToArray())
                editorBeatmap.Remove(h);
        }

        protected void Copy()
        {
            if (editorBeatmap.SelectedHitObjects.Count == 0)
                return;

            clipboard.Value = new ClipboardContent(editorBeatmap).Serialize();
        }

        protected void Paste()
        {
            if (string.IsNullOrEmpty(clipboard.Value))
                return;

            var objects = clipboard.Value.Deserialize<ClipboardContent>().HitObjects;

            Debug.Assert(objects.Any());

            double timeOffset = clock.CurrentTime - objects.Min(o => o.StartTime);

            foreach (var h in objects)
                h.StartTime += timeOffset;

            changeHandler.BeginChange();

            editorBeatmap.SelectedHitObjects.Clear();

            editorBeatmap.AddRange(objects);
            editorBeatmap.SelectedHitObjects.AddRange(objects);

            changeHandler.EndChange();
        }

        protected void Undo() => changeHandler.RestoreState(-1);

        protected void Redo() => changeHandler.RestoreState(1);

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

            LoadComponentAsync(currentScreen, newScreen =>
            {
                if (newScreen == currentScreen)
                    screenContainer.Add(newScreen);
            });
        }

        private void seek(UIEvent e, int direction)
        {
            double amount = e.ShiftPressed ? 2 : 1;

            if (direction < 1)
                clock.SeekBackward(!clock.IsRunning, amount);
            else
                clock.SeekForward(!clock.IsRunning, amount);
        }

        private void exportBeatmap()
        {
            Save();
            beatmapManager.Export(Beatmap.Value.BeatmapSetInfo);
        }

        private void updateLastSavedHash()
        {
            lastSavedHash = changeHandler.CurrentStateHash;
        }

        public double SnapTime(double time, double? referenceTime) => editorBeatmap.SnapTime(time, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => editorBeatmap.GetBeatLengthAtTime(referenceTime);

        public int BeatDivisor => beatDivisor.Value;
    }
}
