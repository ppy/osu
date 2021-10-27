// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
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
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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
using osu.Game.Screens.Edit.Verify;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit
{
    [Cached(typeof(IBeatSnapProvider))]
    [Cached(typeof(ISamplePlaybackDisabler))]
    [Cached]
    public class Editor : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>, IKeyBindingHandler<PlatformAction>, IBeatSnapProvider, ISamplePlaybackDisabler
    {
        public override float BackgroundParallaxAmount => 0.1f;

        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? AllowTrackAdjustments => false;

        protected bool HasUnsavedChanges => lastSavedHash != changeHandler.CurrentStateHash;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        public IBindable<bool> SamplePlaybackDisabled => samplePlaybackDisabled;

        private readonly Bindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        private bool exitConfirmed;

        private string lastSavedHash;

        private Container<EditorScreen> screenContainer;

        [CanBeNull]
        private readonly EditorLoader loader;

        private EditorScreen currentScreen;

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();
        private EditorClock clock;

        private IBeatmap playableBeatmap;
        private EditorBeatmap editorBeatmap;
        private EditorChangeHandler changeHandler;

        private EditorMenuBar menuBar;

        private DependencyContainer dependencies;

        private bool isNewBeatmap;

        protected override UserActivity InitialActivity => new UserActivity.Editing(Beatmap.Value.BeatmapInfo);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MusicController music { get; set; }

        public Editor(EditorLoader loader = null)
        {
            this.loader = loader;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            var loadableBeatmap = Beatmap.Value;

            if (loadableBeatmap is DummyWorkingBeatmap)
            {
                isNewBeatmap = true;

                loadableBeatmap = beatmapManager.CreateNew(Ruleset.Value, api.LocalUser.Value);

                // required so we can get the track length in EditorClock.
                // this is safe as nothing has yet got a reference to this new beatmap.
                loadableBeatmap.LoadTrack();

                // this is a bit haphazard, but guards against setting the lease Beatmap bindable if
                // the editor has already been exited.
                if (!ValidForPush)
                    return;
            }

            try
            {
                playableBeatmap = loadableBeatmap.GetPlayableBeatmap(loadableBeatmap.BeatmapInfo.Ruleset);

                // clone these locally for now to avoid incurring overhead on GetPlayableBeatmap usages.
                // eventually we will want to improve how/where this is done as there are issues with *not* cloning it in all cases.
                playableBeatmap.ControlPointInfo = playableBeatmap.ControlPointInfo.DeepClone();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap successfully!");
                // couldn't load, hard abort!
                this.Exit();
                return;
            }

            beatDivisor.Value = playableBeatmap.BeatmapInfo.BeatDivisor;
            beatDivisor.BindValueChanged(divisor => playableBeatmap.BeatmapInfo.BeatDivisor = divisor.NewValue);

            // Todo: should probably be done at a DrawableRuleset level to share logic with Player.
            clock = new EditorClock(playableBeatmap, beatDivisor) { IsCoupled = false };
            clock.ChangeSource(loadableBeatmap.Track);

            dependencies.CacheAs(clock);
            AddInternal(clock);

            clock.SeekingOrStopped.BindValueChanged(_ => updateSampleDisabledState());

            // todo: remove caching of this and consume via editorBeatmap?
            dependencies.Cache(beatDivisor);

            AddInternal(editorBeatmap = new EditorBeatmap(playableBeatmap, loadableBeatmap.GetSkin(), loadableBeatmap.BeatmapInfo));
            dependencies.CacheAs(editorBeatmap);
            changeHandler = new EditorChangeHandler(editorBeatmap);
            dependencies.CacheAs<IEditorChangeHandler>(changeHandler);

            updateLastSavedHash();

            Schedule(() =>
            {
                // we need to avoid changing the beatmap from an asynchronous load thread. it can potentially cause weirdness including crashes.
                // this assumes that nothing during the rest of this load() method is accessing Beatmap.Value (loadableBeatmap should be preferred).
                // generally this is quite safe, as the actual load of editor content comes after menuBar.Mode.ValueChanged is fired in its own LoadComplete.
                Beatmap.Value = loadableBeatmap;
            });

            OsuMenuItem undoMenuItem;
            OsuMenuItem redoMenuItem;

            EditorMenuItem cutMenuItem;
            EditorMenuItem copyMenuItem;
            EditorMenuItem pasteMenuItem;

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
                        Child = screenContainer = new Container<EditorScreen>
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
                                    Items = createFileMenuItems()
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
                                },
                                new MenuItem("View")
                                {
                                    Items = new MenuItem[]
                                    {
                                        new WaveformOpacityMenuItem(config.GetBindable<float>(OsuSetting.EditorWaveformOpacity)),
                                        new HitAnimationsMenuItem(config.GetBindable<bool>(OsuSetting.EditorHitAnimations))
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
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colours.Gray2
                            },
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
                bool hasObjects = editorBeatmap.SelectedHitObjects.Count > 0;

                cutMenuItem.Action.Disabled = !hasObjects;
                copyMenuItem.Action.Disabled = !hasObjects;
            }, true);

            clipboard.BindValueChanged(content => pasteMenuItem.Action.Disabled = string.IsNullOrEmpty(content.NewValue));

            menuBar.Mode.ValueChanged += onModeChanged;
        }

        /// <summary>
        /// If the beatmap's track has changed, this method must be called to keep the editor in a valid state.
        /// </summary>
        public void UpdateClockSource() => clock.ChangeSource(Beatmap.Value.Track);

        /// <summary>
        /// Restore the editor to a provided state.
        /// </summary>
        /// <param name="state">The state to restore.</param>
        public void RestoreState([NotNull] EditorState state) => Schedule(() =>
        {
            clock.Seek(state.Time);
            clipboard.Value = state.ClipboardContent;
        });

        protected void Save()
        {
            // no longer new after first user-triggered save.
            isNewBeatmap = false;

            // apply any set-level metadata changes.
            beatmapManager.Update(editorBeatmap.BeatmapInfo.BeatmapSet);

            // save the loaded beatmap's data stream.
            beatmapManager.Save(editorBeatmap.BeatmapInfo, editorBeatmap.PlayableBeatmap, editorBeatmap.BeatmapSkin);

            updateLastSavedHash();
        }

        protected override void Update()
        {
            base.Update();
            clock.ProcessFrame();
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Cut:
                    Cut();
                    return true;

                case PlatformAction.Copy:
                    Copy();
                    return true;

                case PlatformAction.Paste:
                    Paste();
                    return true;

                case PlatformAction.Undo:
                    Undo();
                    return true;

                case PlatformAction.Redo:
                    Redo();
                    return true;

                case PlatformAction.Save:
                    Save();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
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
            if (e.ControlPressed || e.AltPressed || e.SuperPressed)
                return false;

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

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
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

                case GlobalAction.EditorVerifyMode:
                    menuBar.Mode.Value = EditorScreenMode.Verify;
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            ApplyToBackground(b =>
            {
                // todo: temporary. we want to be applying dim using the UserDimContainer eventually.
                b.FadeColour(Color4.DarkGray, 500);

                b.IgnoreUserSettings.Value = true;
                b.BlurAmount.Value = 0;
            });

            resetTrack(true);
        }

        public override bool OnExiting(IScreen next)
        {
            if (!exitConfirmed)
            {
                // dialog overlay may not be available in visual tests.
                if (dialogOverlay == null)
                {
                    confirmExit();
                    return true;
                }

                // if the dialog is already displayed, confirm exit with no save.
                if (dialogOverlay.CurrentDialog is PromptForSaveDialog saveDialog)
                {
                    saveDialog.PerformOkAction();
                    return true;
                }

                if (isNewBeatmap || HasUnsavedChanges)
                {
                    samplePlaybackDisabled.Value = true;
                    dialogOverlay?.Push(new PromptForSaveDialog(confirmExit, confirmExitWithSave, cancelExit));
                    return true;
                }
            }

            ApplyToBackground(b => b.FadeColour(Color4.White, 500));
            resetTrack();

            // To update the game-wide beatmap with any changes, perform a re-fetch on exit.
            // This is required as the editor makes its local changes via EditorBeatmap
            // (which are not propagated outwards to a potentially cached WorkingBeatmap).
            var refetchedBeatmap = beatmapManager.GetWorkingBeatmap(Beatmap.Value.BeatmapInfo);

            if (!(refetchedBeatmap is DummyWorkingBeatmap))
            {
                Logger.Log("Editor providing re-fetched beatmap post edit session");
                Beatmap.Value = refetchedBeatmap;
            }

            return base.OnExiting(next);
        }

        private void confirmExitWithSave()
        {
            Save();

            exitConfirmed = true;
            this.Exit();
        }

        private void confirmExit()
        {
            // stop the track if playing to allow the parent screen to choose a suitable playback mode.
            Beatmap.Value.Track.Stop();

            if (isNewBeatmap)
            {
                // confirming exit without save means we should delete the new beatmap completely.
                beatmapManager.Delete(playableBeatmap.BeatmapInfo.BeatmapSet);

                // eagerly clear contents before restoring default beatmap to prevent value change callbacks from firing.
                ClearInternal();

                // in theory this shouldn't be required but due to EF core not sharing instance states 100%
                // MusicController is unaware of the changed DeletePending state.
                Beatmap.SetDefault();
            }

            exitConfirmed = true;
            this.Exit();
        }

        private readonly Bindable<string> clipboard = new Bindable<string>();

        protected void Cut()
        {
            Copy();
            editorBeatmap.RemoveRange(editorBeatmap.SelectedHitObjects.ToArray());
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

            editorBeatmap.BeginChange();

            editorBeatmap.SelectedHitObjects.Clear();

            editorBeatmap.AddRange(objects);
            editorBeatmap.SelectedHitObjects.AddRange(objects);

            editorBeatmap.EndChange();
        }

        protected void Undo() => changeHandler.RestoreState(-1);

        protected void Redo() => changeHandler.RestoreState(1);

        private void resetTrack(bool seekToStart = false)
        {
            Beatmap.Value.Track.Stop();

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
            var lastScreen = currentScreen;

            lastScreen?.Hide();

            try
            {
                if ((currentScreen = screenContainer.SingleOrDefault(s => s.Type == e.NewValue)) != null)
                {
                    screenContainer.ChangeChildDepth(currentScreen, lastScreen?.Depth + 1 ?? 0);
                    currentScreen.Show();
                    return;
                }

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

                    case EditorScreenMode.Verify:
                        currentScreen = new VerifyScreen();
                        break;

                    default:
                        throw new InvalidOperationException("Editor menu bar switched to an unsupported mode");
                }

                LoadComponentAsync(currentScreen, newScreen =>
                {
                    if (newScreen == currentScreen)
                    {
                        screenContainer.Add(newScreen);
                        newScreen.Show();
                    }
                });
            }
            finally
            {
                updateSampleDisabledState();
            }
        }

        private void updateSampleDisabledState()
        {
            samplePlaybackDisabled.Value = clock.SeekingOrStopped.Value || !(currentScreen is ComposeScreen);
        }

        private void seek(UIEvent e, int direction)
        {
            double amount = e.ShiftPressed ? 4 : 1;

            bool trackPlaying = clock.IsRunning;

            if (trackPlaying)
            {
                // generally users are not looking to perform tiny seeks when the track is playing,
                // so seeks should always be by one full beat, bypassing the beatDivisor.
                // this multiplication undoes the division that will be applied in the underlying seek operation.
                amount *= beatDivisor.Value;
            }

            if (direction < 1)
                clock.SeekBackward(!trackPlaying, amount);
            else
                clock.SeekForward(!trackPlaying, amount);
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

        private List<MenuItem> createFileMenuItems()
        {
            var fileMenuItems = new List<MenuItem>
            {
                new EditorMenuItem("Save", MenuItemType.Standard, Save)
            };

            if (RuntimeInfo.IsDesktop)
                fileMenuItems.Add(new EditorMenuItem("Export package", MenuItemType.Standard, exportBeatmap));

            fileMenuItems.Add(new EditorMenuItemSpacer());

            var beatmapSet = beatmapManager.QueryBeatmapSet(bs => bs.ID == Beatmap.Value.BeatmapSetInfo.ID) ?? playableBeatmap.BeatmapInfo.BeatmapSet;

            var difficultyItems = new List<MenuItem>();

            foreach (var rulesetBeatmaps in beatmapSet.Beatmaps.GroupBy(b => b.RulesetID).OrderBy(group => group.Key))
            {
                if (difficultyItems.Count > 0)
                    difficultyItems.Add(new EditorMenuItemSpacer());

                foreach (var beatmap in rulesetBeatmaps.OrderBy(b => b.StarDifficulty))
                    difficultyItems.Add(createDifficultyMenuItem(beatmap));
            }

            fileMenuItems.Add(new EditorMenuItem("Change difficulty") { Items = difficultyItems });

            fileMenuItems.Add(new EditorMenuItemSpacer());
            fileMenuItems.Add(new EditorMenuItem("Exit", MenuItemType.Standard, this.Exit));
            return fileMenuItems;
        }

        private DifficultyMenuItem createDifficultyMenuItem(BeatmapInfo beatmapInfo)
        {
            bool isCurrentDifficulty = playableBeatmap.BeatmapInfo.Equals(beatmapInfo);
            return new DifficultyMenuItem(beatmapInfo, isCurrentDifficulty, SwitchToDifficulty);
        }

        protected void SwitchToDifficulty(BeatmapInfo nextBeatmap) => loader?.ScheduleDifficultySwitch(nextBeatmap, new EditorState
        {
            Time = clock.CurrentTimeAccurate,
            ClipboardContent = editorBeatmap.BeatmapInfo.RulesetID == nextBeatmap.RulesetID ? clipboard.Value : string.Empty
        });

        private void cancelExit()
        {
            samplePlaybackDisabled.Value = false;
            loader?.CancelPendingDifficultySwitch();
        }

        public double SnapTime(double time, double? referenceTime) => editorBeatmap.SnapTime(time, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => editorBeatmap.GetBeatLengthAtTime(referenceTime);

        public int BeatDivisor => beatDivisor.Value;
    }
}
