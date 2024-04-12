// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    [Cached]
    public abstract partial class Player : ScreenWithBeatmapBackground, ISamplePlaybackDisabler, ILocalUserPlayInfo
    {
        /// <summary>
        /// The delay upon completion of the beatmap before displaying the results screen.
        /// </summary>
        public const double RESULTS_DISPLAY_DELAY = 1000.0;

        /// <summary>
        /// Raised after <see cref="StartGameplay"/> is called.
        /// </summary>
        public event Action OnGameplayStarted;

        public override bool AllowBackButton => false; // handled by HoldForMenuButton

        protected override bool PlayExitSound => !isRestarting;

        protected override UserActivity InitialActivity => new UserActivity.InSoloGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public override float BackgroundParallaxAmount => 0.1f;

        public override bool HideOverlaysOnEnter => true;

        public override bool HideMenuCursorOnNonMouseInput => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        // We are managing our own adjustments (see OnEntering/OnExiting).
        public override bool? ApplyModTrackAdjustments => false;

        private readonly IBindable<bool> gameActive = new Bindable<bool>(true);

        private readonly Bindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        /// <summary>
        /// Whether gameplay should pause when the game window focus is lost.
        /// </summary>
        protected virtual bool PauseOnFocusLost => true;

        public Action<bool> RestartRequested;

        private bool isRestarting;

        private Bindable<bool> mouseWheelDisabled;

        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();

        public IBindable<bool> LocalUserPlaying => localUserPlaying;

        private readonly Bindable<bool> localUserPlaying = new Bindable<bool>();

        public int RestartCount;

        /// <summary>
        /// Whether the <see cref="HUDOverlay"/> is currently visible.
        /// </summary>
        public IBindable<bool> ShowingOverlayComponents = new Bindable<bool>();

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        public GameplayState GameplayState { get; private set; }

        private Ruleset ruleset;

        public BreakOverlay BreakOverlay;

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public readonly IBindable<bool> IsBreakTime = new BindableBool();

        private BreakTracker breakTracker;

        private SkipOverlay skipIntroOverlay;
        private SkipOverlay skipOutroOverlay;

        protected ScoreProcessor ScoreProcessor { get; private set; }

        protected HealthProcessor HealthProcessor { get; private set; }

        protected DrawableRuleset DrawableRuleset { get; private set; }

        protected HUDOverlay HUDOverlay { get; private set; }

        public bool LoadedBeatmapSuccessfully => DrawableRuleset?.Objects.Any() == true;

        protected GameplayClockContainer GameplayClockContainer { get; private set; }

        public DimmableStoryboard DimmableStoryboard { get; private set; }

        /// <summary>
        /// Whether failing should be allowed.
        /// By default, this checks whether all selected mods allow failing.
        /// </summary>
        protected virtual bool CheckModsAllowFailure() => GameplayState.Mods.OfType<IApplicableFailOverride>().All(m => m.PerformFail());

        public readonly PlayerConfiguration Configuration;

        /// <summary>
        /// The score for the current play session.
        /// Available only after the player is loaded.
        /// </summary>
        public Score Score { get; private set; }

        /// <summary>
        /// Create a new player instance.
        /// </summary>
        protected Player(PlayerConfiguration configuration = null)
        {
            Configuration = configuration ?? new PlayerConfiguration();
        }

        private ScreenSuspensionHandler screenSuspension;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!LoadedBeatmapSuccessfully)
                return;

            PrepareReplay();

            ScoreProcessor.NewJudgement += _ => ScoreProcessor.PopulateScore(Score.ScoreInfo);
            ScoreProcessor.OnResetFromReplayFrame += () => ScoreProcessor.PopulateScore(Score.ScoreInfo);

            gameActive.BindValueChanged(_ => updatePauseOnFocusLostState(), true);
        }

        /// <summary>
        /// Run any recording / playback setup for replays.
        /// </summary>
        protected virtual void PrepareReplay()
        {
            DrawableRuleset.SetRecordTarget(Score);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, OsuGameBase game, CancellationToken cancellationToken)
        {
            var gameplayMods = Mods.Value.Select(m => m.DeepClone()).ToArray();

            if (gameplayMods.Any(m => m is UnknownMod))
            {
                Logger.Log("Gameplay was started with an unknown mod applied.", level: LogLevel.Important);
                return;
            }

            if (Beatmap.Value is DummyWorkingBeatmap)
                return;

            IBeatmap playableBeatmap = loadPlayableBeatmap(gameplayMods, cancellationToken);

            if (playableBeatmap == null)
                return;

            if (!ModUtils.CheckModsBelongToRuleset(ruleset, gameplayMods))
            {
                Logger.Log($@"Gameplay was started with a mod belonging to a ruleset different than '{ruleset.Description}'.", level: LogLevel.Important);
                return;
            }

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            if (game != null)
                gameActive.BindTo(game.IsActive);

            if (game is OsuGame osuGame)
                LocalUserPlaying.BindTo(osuGame.LocalUserPlaying);

            DrawableRuleset = ruleset.CreateDrawableRulesetWith(playableBeatmap, gameplayMods);
            dependencies.CacheAs(DrawableRuleset);

            ScoreProcessor = ruleset.CreateScoreProcessor();
            ScoreProcessor.Mods.Value = gameplayMods;
            ScoreProcessor.ApplyBeatmap(playableBeatmap);

            dependencies.CacheAs(ScoreProcessor);

            HealthProcessor = gameplayMods.OfType<IApplicableHealthProcessor>().FirstOrDefault()?.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime);
            HealthProcessor ??= ruleset.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime);
            HealthProcessor.ApplyBeatmap(playableBeatmap);

            dependencies.CacheAs(HealthProcessor);

            InternalChild = GameplayClockContainer = CreateGameplayClockContainer(Beatmap.Value, DrawableRuleset.GameplayStartTime);

            AddInternal(screenSuspension = new ScreenSuspensionHandler(GameplayClockContainer));

            Score = CreateScore(playableBeatmap);

            // ensure the score is in a consistent state with the current player.
            Score.ScoreInfo.BeatmapInfo = Beatmap.Value.BeatmapInfo;
            Score.ScoreInfo.BeatmapHash = Beatmap.Value.BeatmapInfo.Hash;
            Score.ScoreInfo.Ruleset = ruleset.RulesetInfo;
            Score.ScoreInfo.Mods = gameplayMods;

            dependencies.CacheAs(GameplayState = new GameplayState(playableBeatmap, ruleset, gameplayMods, Score, ScoreProcessor, Beatmap.Value.Storyboard));

            var rulesetSkinProvider = new RulesetSkinProvidingContainer(ruleset, playableBeatmap, Beatmap.Value.Skin);

            // load the skinning hierarchy first.
            // this is intentionally done in two stages to ensure things are in a loaded state before exposing the ruleset to skin sources.
            GameplayClockContainer.Add(rulesetSkinProvider);

            if (cancellationToken.IsCancellationRequested)
                return;

            rulesetSkinProvider.AddRange(new Drawable[]
            {
                failAnimationContainer = new FailAnimationContainer(DrawableRuleset)
                {
                    OnComplete = onFailComplete,
                    Children = new[]
                    {
                        // underlay and gameplay should have access to the skinning sources.
                        createUnderlayComponents(),
                        createGameplayComponents(Beatmap.Value)
                    }
                },
                FailOverlay = new FailOverlay
                {
                    SaveReplay = async () => await prepareAndImportScoreAsync(true).ConfigureAwait(false),
                    OnRetry = Configuration.AllowUserInteraction ? () => Restart() : null,
                    OnQuit = () => PerformExit(true),
                },
                new HotkeyExitOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        if (PerformExit(false))
                            // The hotkey overlay dims the screen.
                            // If the operation succeeds, we want to make sure we stay dimmed to keep continuity.
                            fadeOut(true);
                    },
                },
            });

            if (cancellationToken.IsCancellationRequested)
                return;

            if (Configuration.AllowRestart)
            {
                rulesetSkinProvider.AddRange(new Drawable[]
                {
                    new HotkeyRetryOverlay
                    {
                        Action = () =>
                        {
                            if (!this.IsCurrentScreen()) return;

                            if (Restart(true))
                                // The hotkey overlay dims the screen.
                                // If the operation succeeds, we want to make sure we stay dimmed to keep continuity.
                                fadeOut(true);
                        },
                    },
                });
            }

            dependencies.CacheAs(DrawableRuleset.FrameStableClock);

            // add the overlay components as a separate step as they proxy some elements from the above underlay/gameplay components.
            // also give the overlays the ruleset skin provider to allow rulesets to potentially override HUD elements (used to disable combo counters etc.)
            // we may want to limit this in the future to disallow rulesets from outright replacing elements the user expects to be there.
            failAnimationContainer.Add(createOverlayComponents(Beatmap.Value));

            if (!DrawableRuleset.AllowGameplayOverlays)
            {
                HUDOverlay.ShowHud.Value = false;
                HUDOverlay.ShowHud.Disabled = true;
                BreakOverlay.Hide();
            }

            DrawableRuleset.FrameStableClock.WaitingOnFrames.BindValueChanged(waiting =>
            {
                if (waiting.NewValue)
                    GameplayClockContainer.Stop();
                else
                    GameplayClockContainer.Start();
            });

            DrawableRuleset.IsPaused.BindValueChanged(_ =>
            {
                updateGameplayState();
                updateSampleDisabledState();
            });

            DrawableRuleset.FrameStableClock.IsCatchingUp.BindValueChanged(_ => updateSampleDisabledState());

            DrawableRuleset.HasReplayLoaded.BindValueChanged(_ => updateGameplayState());

            // bind clock into components that require it
            ((IBindable<bool>)DrawableRuleset.IsPaused).BindTo(GameplayClockContainer.IsPaused);

            DrawableRuleset.NewResult += r =>
            {
                HealthProcessor.ApplyResult(r);
                ScoreProcessor.ApplyResult(r);
                GameplayState.ApplyResult(r);
            };

            DrawableRuleset.RevertResult += r =>
            {
                HealthProcessor.RevertResult(r);
                ScoreProcessor.RevertResult(r);
            };

            DimmableStoryboard.HasStoryboardEnded.ValueChanged += _ => checkScoreCompleted();

            // Bind the judgement processors to ourselves
            ScoreProcessor.HasCompleted.BindValueChanged(_ => checkScoreCompleted());
            HealthProcessor.Failed += onFail;

            // Provide judgement processors to mods after they're loaded so that they're on the gameplay clock,
            // this is required for mods that apply transforms to these processors.
            ScoreProcessor.OnLoadComplete += _ =>
            {
                foreach (var mod in gameplayMods.OfType<IApplicableToScoreProcessor>())
                    mod.ApplyToScoreProcessor(ScoreProcessor);
            };

            HealthProcessor.OnLoadComplete += _ =>
            {
                foreach (var mod in gameplayMods.OfType<IApplicableToHealthProcessor>())
                    mod.ApplyToHealthProcessor(HealthProcessor);
            };

            IsBreakTime.BindTo(breakTracker.IsBreakTime);
            IsBreakTime.BindValueChanged(onBreakTimeChanged, true);

            loadLeaderboard();
        }

        protected virtual GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart) => new MasterGameplayClockContainer(beatmap, gameplayStart);

        private Drawable createUnderlayComponents() =>
            DimmableStoryboard = new DimmableStoryboard(GameplayState.Storyboard, GameplayState.Mods) { RelativeSizeAxes = Axes.Both };

        private Drawable createGameplayComponents(IWorkingBeatmap working) => new ScalingContainer(ScalingMode.Gameplay)
        {
            Children = new Drawable[]
            {
                DrawableRuleset.With(r =>
                    r.FrameStableComponents.Children = new Drawable[]
                    {
                        ScoreProcessor,
                        HealthProcessor,
                        new ComboEffects(ScoreProcessor),
                        breakTracker = new BreakTracker(DrawableRuleset.GameplayStartTime, ScoreProcessor)
                        {
                            Breaks = working.Beatmap.Breaks
                        }
                    }),
            }
        };

        private Drawable createOverlayComponents(IWorkingBeatmap working)
        {
            var container = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    DimmableStoryboard.OverlayLayerContainer.CreateProxy(),
                    BreakOverlay = new BreakOverlay(working.Beatmap.BeatmapInfo.LetterboxInBreaks, ScoreProcessor)
                    {
                        Clock = DrawableRuleset.FrameStableClock,
                        ProcessCustomClock = false,
                        Breaks = working.Beatmap.Breaks
                    },
                    // display the cursor above some HUD elements.
                    DrawableRuleset.Cursor?.CreateProxy() ?? new Container(),
                    DrawableRuleset.ResumeOverlay?.CreateProxy() ?? new Container(),
                    HUDOverlay = new HUDOverlay(DrawableRuleset, GameplayState.Mods, Configuration.AlwaysShowLeaderboard)
                    {
                        HoldToQuit =
                        {
                            Action = () => PerformExit(true),
                            IsPaused = { BindTarget = GameplayClockContainer.IsPaused },
                            ReplayLoaded = { BindTarget = DrawableRuleset.HasReplayLoaded },
                        },
                        InputCountController =
                        {
                            IsCounting =
                            {
                                Value = false
                            },
                        },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    skipIntroOverlay = new SkipOverlay(DrawableRuleset.GameplayStartTime)
                    {
                        RequestSkip = performUserRequestedSkip
                    },
                    skipOutroOverlay = new SkipOverlay(GameplayState.Storyboard.LatestEventTime ?? 0)
                    {
                        RequestSkip = () => progressToResults(false),
                        Alpha = 0
                    },
                    PauseOverlay = new PauseOverlay
                    {
                        OnResume = Resume,
                        Retries = RestartCount,
                        OnRetry = () => Restart(),
                        OnQuit = () => PerformExit(true),
                    },
                },
            };

            if (!Configuration.AllowSkipping || !DrawableRuleset.AllowGameplayOverlays)
            {
                skipIntroOverlay.Expire();
                skipOutroOverlay.Expire();
            }

            return container;
        }

        private void onBreakTimeChanged(ValueChangedEvent<bool> isBreakTime)
        {
            updateGameplayState();
            updatePauseOnFocusLostState();
            HUDOverlay.InputCountController.IsCounting.Value = !isBreakTime.NewValue;
        }

        private void updateGameplayState()
        {
            bool inGameplay = !DrawableRuleset.HasReplayLoaded.Value && !DrawableRuleset.IsPaused.Value && !breakTracker.IsBreakTime.Value && !GameplayState.HasFailed;
            OverlayActivationMode.Value = inGameplay ? OverlayActivation.Disabled : OverlayActivation.UserTriggered;
            localUserPlaying.Value = inGameplay;
        }

        private void updateSampleDisabledState()
        {
            samplePlaybackDisabled.Value = DrawableRuleset.FrameStableClock.IsCatchingUp.Value || GameplayClockContainer.IsPaused.Value;
        }

        private void updatePauseOnFocusLostState()
        {
            if (!PauseOnFocusLost || !pausingSupportedByCurrentState || breakTracker.IsBreakTime.Value)
                return;

            if (gameActive.Value == false)
            {
                bool paused = Pause();

                // if the initial pause could not be satisfied, the pause cooldown may be active.
                // reschedule the pause attempt until it can be achieved.
                if (!paused)
                    Scheduler.AddOnce(updatePauseOnFocusLostState);
            }
        }

        private IBeatmap loadPlayableBeatmap(Mod[] gameplayMods, CancellationToken cancellationToken)
        {
            IBeatmap playable;

            try
            {
                if (Beatmap.Value.Beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                var rulesetInfo = Ruleset.Value ?? Beatmap.Value.BeatmapInfo.Ruleset;
                ruleset = rulesetInfo.CreateInstance();

                if (ruleset == null)
                    throw new RulesetLoadException("Instantiation failure");

                try
                {
                    playable = Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, gameplayMods, cancellationToken);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // A playable beatmap may not be creatable with the user's preferred ruleset, so try using the beatmap's default ruleset
                    rulesetInfo = Beatmap.Value.BeatmapInfo.Ruleset;
                    ruleset = rulesetInfo.CreateInstance();

                    playable = Beatmap.Value.GetPlayableBeatmap(rulesetInfo, gameplayMods, cancellationToken);
                }

                if (playable.HitObjects.Count == 0)
                {
                    Logger.Log("Beatmap contains no hit objects!", level: LogLevel.Important);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                // Load has been cancelled. No logging is required.
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap successfully!");
                //couldn't load, hard abort!
                return null;
            }

            return playable;
        }

        /// <summary>
        /// Attempts to complete a user request to exit gameplay.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>This should only be called in response to a user interaction. Exiting is not guaranteed.</item>
        /// <item>This will interrupt any pending progression to the results screen, even if the transition has begun.</item>
        /// </list>
        /// </remarks>
        /// <param name="showDialogFirst">
        /// Whether the pause or fail dialog should be shown before performing an exit.
        /// If <see langword="true"/> and a dialog is not yet displayed, the exit will be blocked and the relevant dialog will display instead.
        /// </param>
        /// <returns>Whether this call resulted in a final exit.</returns>
        protected bool PerformExit(bool showDialogFirst)
        {
            bool pauseOrFailDialogVisible =
                PauseOverlay.State.Value == Visibility.Visible || FailOverlay.State.Value == Visibility.Visible;

            if (showDialogFirst && !pauseOrFailDialogVisible)
            {
                // if the fail animation is currently in progress, accelerate it (it will show the pause dialog on completion).
                if (ValidForResume && GameplayState.HasFailed)
                {
                    failAnimationContainer.FinishTransforms(true);
                    return false;
                }

                // even if this call has requested a dialog, there is a chance the current player mode doesn't support pausing.
                if (pausingSupportedByCurrentState)
                {
                    // in the case a dialog needs to be shown, attempt to pause and show it.
                    // this may fail (see internal checks in Pause()) but the fail cases are temporary, so don't fall through to Exit().
                    Pause();
                    return false;
                }
            }

            // Matching osu!stable behaviour, if the results screen is pending and the user requests an exit,
            // show the results instead.
            if (GameplayState.HasPassed && !isRestarting)
            {
                progressToResults(false);
                return false;
            }

            // import current score if possible.
            prepareAndImportScoreAsync();

            // Screen may not be current if a restart has been performed.
            if (this.IsCurrentScreen())
            {
                // The actual exit is performed if
                // - the pause / fail dialog was not requested
                // - the pause / fail dialog was requested but is already displayed (user showing intention to exit).
                // - the pause / fail dialog was requested but couldn't be displayed due to the type or state of this Player instance.
                this.Exit();
            }

            return true;
        }

        private void performUserRequestedSkip()
        {
            // user requested skip
            // disable sample playback to stop currently playing samples and perform skip
            samplePlaybackDisabled.Value = true;

            (GameplayClockContainer as MasterGameplayClockContainer)?.Skip();

            // return samplePlaybackDisabled.Value to what is defined by the beatmap's current state
            updateSampleDisabledState();
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public void Seek(double time) => GameplayClockContainer.Seek(time);

        private ScheduledDelegate frameStablePlaybackResetDelegate;

        /// <summary>
        /// Specify and seek to a custom start time from which gameplay should be observed.
        /// </summary>
        /// <remarks>
        /// This performs a non-frame-stable seek. Intermediate hitobject judgements may not be applied or reverted correctly during this seek.
        /// </remarks>
        /// <param name="time">The destination time to seek to.</param>
        protected void SetGameplayStartTime(double time)
        {
            if (frameStablePlaybackResetDelegate?.Cancelled == false && !frameStablePlaybackResetDelegate.Completed)
                frameStablePlaybackResetDelegate.RunTask();

            bool wasFrameStable = DrawableRuleset.FrameStablePlayback;
            DrawableRuleset.FrameStablePlayback = false;

            GameplayClockContainer.Reset(time);

            // Delay resetting frame-stable playback for one frame to give the FrameStabilityContainer a chance to seek.
            frameStablePlaybackResetDelegate = ScheduleAfterChildren(() => DrawableRuleset.FrameStablePlayback = wasFrameStable);
        }

        /// <summary>
        /// Restart gameplay via a parent <see cref="PlayerLoader"/>.
        /// <remarks>This can be called from a child screen in order to trigger the restart process.</remarks>
        /// </summary>
        /// <param name="quickRestart">Whether a quick restart was requested (skipping intro etc.).</param>
        /// <returns>Whether this call resulted in a restart.</returns>
        public bool Restart(bool quickRestart = false)
        {
            if (!Configuration.AllowRestart)
                return false;

            isRestarting = true;

            // at the point of restarting the track should either already be paused or the volume should be zero.
            // stopping here is to ensure music doesn't become audible after exiting back to PlayerLoader.
            musicController.Stop();

            RestartRequested?.Invoke(quickRestart);

            return PerformExit(false);
        }

        /// <summary>
        /// This delegate, when set, means the results screen has been queued to appear.
        /// The display of the results screen may be delayed by any work being done in <see cref="PrepareScoreForResultsAsync"/>.
        /// </summary>
        /// <remarks>
        /// Once set, this can *only* be cancelled by rewinding, ie. if <see cref="JudgementProcessor.HasCompleted">ScoreProcessor.HasCompleted</see> becomes <see langword="false"/>.
        /// Even if the user requests an exit, it will forcefully proceed to the results screen (see special case in <see cref="OnExiting"/>).
        /// </remarks>
        private ScheduledDelegate resultsDisplayDelegate;

        /// <summary>
        /// A task which asynchronously prepares a completed score for display at results.
        /// This may include performing net requests or importing the score into the database, generally to ensure things are in a sane state for the play session.
        /// </summary>
        private Task<ScoreInfo> prepareScoreForDisplayTask;

        /// <summary>
        /// Handles changes in player state which may progress the completion of gameplay / this screen's lifetime.
        /// </summary>
        private void checkScoreCompleted()
        {
            // If this player instance is in the middle of an exit, don't attempt any kind of state update.
            if (!this.IsCurrentScreen())
                return;

            // Handle cases of arriving at this method when not in a completed state.
            // - When a storyboard completion triggered this call earlier than gameplay finishes.
            // - When a replay has been rewound before a queued resultsDisplayDelegate has run.
            //
            // Currently, even if this scenario is hit, prepareAndImportScoreAsync has already been queued (and potentially run).
            // In the scenarios above, this is a non-issue, but it still feels a bit convoluted to have to cancel in this method.
            // Maybe this can be improved with further refactoring.
            if (!ScoreProcessor.HasCompleted.Value)
            {
                resultsDisplayDelegate?.Cancel();
                resultsDisplayDelegate = null;

                GameplayState.HasPassed = false;
                ValidForResume = true;
                skipOutroOverlay.Hide();
                return;
            }

            // Only show the completion screen if the player hasn't failed
            if (GameplayState.HasFailed)
                return;

            GameplayState.HasPassed = true;

            // Setting this early in the process means that even if something were to go wrong in the order of events following, there
            // is no chance that a user could return to the (already completed) Player instance from a child screen.
            ValidForResume = false;

            bool storyboardStillRunning = DimmableStoryboard.ContentDisplayed && !DimmableStoryboard.HasStoryboardEnded.Value;

            // If the current beatmap has a storyboard, this method will be called again on storyboard completion.
            // Alternatively, the user may press the outro skip button, forcing immediate display of the results screen.
            if (storyboardStillRunning)
            {
                skipOutroOverlay.Show();
                return;
            }

            progressToResults(true);
        }

        /// <summary>
        /// Queue the results screen for display.
        /// </summary>
        /// <remarks>
        /// A final display will only occur once all work is completed in <see cref="PrepareScoreForResultsAsync"/>. This means that even after calling this method, the results screen will never be shown until <see cref="JudgementProcessor.HasCompleted">ScoreProcessor.HasCompleted</see> becomes <see langword="true"/>.
        /// </remarks>
        /// <param name="withDelay">Whether a minimum delay (<see cref="RESULTS_DISPLAY_DELAY"/>) should be added before the screen is displayed.</param>
        private void progressToResults(bool withDelay)
        {
            if (!Configuration.ShowResults)
                return;

            // Setting this early in the process means that even if something were to go wrong in the order of events following, there
            // is no chance that a user could return to the (already completed) Player instance from a child screen.
            ValidForResume = false;

            double delay = withDelay ? RESULTS_DISPLAY_DELAY : 0;

            resultsDisplayDelegate?.Cancel();
            resultsDisplayDelegate = new ScheduledDelegate(() =>
            {
                if (prepareScoreForDisplayTask == null)
                {
                    // Try importing score since the task hasn't been invoked yet.
                    prepareAndImportScoreAsync();
                    return;
                }

                if (!prepareScoreForDisplayTask.IsCompleted)
                    // If the asynchronous preparation has not completed, keep repeating this delegate.
                    return;

                resultsDisplayDelegate?.Cancel();

                if (prepareScoreForDisplayTask.GetResultSafely() == null)
                {
                    // If score import did not occur, we do not want to show the results screen.
                    return;
                }

                if (!this.IsCurrentScreen())
                    // This player instance may already be in the process of exiting.
                    return;

                this.Push(CreateResults(prepareScoreForDisplayTask.GetResultSafely()));
            }, Time.Current + delay, 50);

            Scheduler.Add(resultsDisplayDelegate);
        }

        /// <summary>
        /// Asynchronously run score preparation operations (database import, online submission etc.).
        /// </summary>
        /// <param name="forceImport">Whether the score should be imported even if non-passing (or the current configuration doesn't allow for it).</param>
        /// <returns>The final score.</returns>
        [ItemCanBeNull]
        private Task<ScoreInfo> prepareAndImportScoreAsync(bool forceImport = false)
        {
            // Ensure we are not writing to the replay any more, as we are about to consume and store the score.
            DrawableRuleset.SetRecordTarget(null);

            if (prepareScoreForDisplayTask != null)
                return prepareScoreForDisplayTask;

            // We do not want to import the score in cases where we don't show results
            bool canShowResults = Configuration.ShowResults && ScoreProcessor.HasCompleted.Value && GameplayState.HasPassed;
            if (!canShowResults && !forceImport)
                return Task.FromResult<ScoreInfo>(null);

            // Clone score before beginning any async processing.
            // - Must be run synchronously as the score may potentially be mutated in the background.
            // - Must be cloned for the same reason.
            Score scoreCopy = Score.DeepClone();

            return prepareScoreForDisplayTask = Task.Run(async () =>
            {
                try
                {
                    await PrepareScoreForResultsAsync(scoreCopy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, @"Score preparation failed!");
                }

                try
                {
                    await ImportScore(scoreCopy).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, @"Score import failed!");
                }

                return scoreCopy.ScoreInfo;
            });
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // During pause, allow global volume adjust regardless of settings.
            if (GameplayClockContainer.IsPaused.Value)
                return false;

            // Block global volume adjust if the user has asked for it (special case when holding "Alt").
            return mouseWheelDisabled.Value && !e.AltPressed;
        }

        #region Gameplay leaderboard

        protected readonly Bindable<bool> LeaderboardExpandedState = new BindableBool();

        private void loadLeaderboard()
        {
            HUDOverlay.HoldingForHUD.BindValueChanged(_ => updateLeaderboardExpandedState());
            LocalUserPlaying.BindValueChanged(_ => updateLeaderboardExpandedState(), true);

            var gameplayLeaderboard = CreateGameplayLeaderboard();

            if (gameplayLeaderboard != null)
            {
                LoadComponentAsync(gameplayLeaderboard, leaderboard =>
                {
                    if (!LoadedBeatmapSuccessfully)
                        return;

                    leaderboard.Expanded.BindTo(LeaderboardExpandedState);

                    AddLeaderboardToHUD(leaderboard);
                });
            }
        }

        [CanBeNull]
        protected virtual GameplayLeaderboard CreateGameplayLeaderboard() => null;

        protected virtual void AddLeaderboardToHUD(GameplayLeaderboard leaderboard) => HUDOverlay.LeaderboardFlow.Add(leaderboard);

        private void updateLeaderboardExpandedState() =>
            LeaderboardExpandedState.Value = !LocalUserPlaying.Value || HUDOverlay.HoldingForHUD.Value;

        #endregion

        #region Fail Logic

        /// <summary>
        /// Invoked when gameplay has permanently failed.
        /// </summary>
        protected virtual void OnFail()
        {
        }

        protected FailOverlay FailOverlay { get; private set; }

        private FailAnimationContainer failAnimationContainer;

        private bool onFail()
        {
            // Failing after the quit sequence has started may cause weird side effects with the fail animation / effects.
            if (GameplayState.HasQuit)
                return false;

            if (!CheckModsAllowFailure())
                return false;

            if (Configuration.AllowFailAnimation)
            {
                Debug.Assert(!GameplayState.HasFailed);
                Debug.Assert(!GameplayState.HasPassed);
                Debug.Assert(!GameplayState.HasQuit);

                GameplayState.HasFailed = true;

                updateGameplayState();

                // There is a chance that we could be in a paused state as the ruleset's internal clock (see FrameStabilityContainer)
                // could process an extra frame after the GameplayClock is stopped.
                // In such cases we want the fail state to precede a user triggered pause.
                if (PauseOverlay.State.Value == Visibility.Visible)
                    PauseOverlay.Hide();

                failAnimationContainer.Start();

                // Failures can be triggered either by a judgement, or by a mod.
                //
                // For the case of a judgement, due to ordering considerations, ScoreProcessor will not have received
                // the final judgement which triggered the failure yet (see DrawableRuleset.NewResult handling above).
                //
                // A schedule here ensures that any lingering judgements from the current frame are applied before we
                // finalise the score as "failed".
                Schedule(() =>
                {
                    ScoreProcessor.FailScore(Score.ScoreInfo);
                    OnFail();

                    if (GameplayState.Mods.OfType<IApplicableFailOverride>().Any(m => m.RestartOnFail))
                        Restart(true);
                });
            }
            else
            {
                ScoreProcessor.FailScore(Score.ScoreInfo);
            }

            return true;
        }

        /// <summary>
        /// Invoked when the fail animation has finished.
        /// </summary>
        private void onFailComplete()
        {
            GameplayClockContainer.Stop();

            FailOverlay.Retries = RestartCount;
            FailOverlay.Show();
        }

        #endregion

        #region Pause Logic

        public bool IsResuming { get; private set; }

        /// <summary>
        /// The amount of gameplay time after which a second pause is allowed.
        /// </summary>
        private const double pause_cooldown = 1000;

        protected PauseOverlay PauseOverlay { get; private set; }

        private double? lastPauseActionTime;

        protected bool PauseCooldownActive =>
            lastPauseActionTime.HasValue && GameplayClockContainer.CurrentTime < lastPauseActionTime + pause_cooldown;

        /// <summary>
        /// A set of conditionals which defines whether the current game state and configuration allows for
        /// pausing to be attempted via <see cref="Pause"/>. If false, the game should generally exit if a user pause
        /// is attempted.
        /// </summary>
        private bool pausingSupportedByCurrentState =>
            // must pass basic screen conditions (beatmap loaded, instance allows pause)
            LoadedBeatmapSuccessfully && Configuration.AllowPause && ValidForResume
            // replays cannot be paused and exit immediately
            && !DrawableRuleset.HasReplayLoaded.Value
            // cannot pause if we are already in a fail state
            && !GameplayState.HasFailed;

        private bool canResume =>
            // cannot resume from a non-paused state
            GameplayClockContainer.IsPaused.Value
            // cannot resume if we are already in a fail state
            && !GameplayState.HasFailed
            // already resuming
            && !IsResuming;

        public bool Pause()
        {
            if (!pausingSupportedByCurrentState) return false;

            if (!IsResuming && PauseCooldownActive)
                return false;

            if (IsResuming)
            {
                DrawableRuleset.CancelResume();
                IsResuming = false;
            }

            GameplayClockContainer.Stop();
            PauseOverlay.Show();
            lastPauseActionTime = GameplayClockContainer.CurrentTime;
            return true;
        }

        public void Resume()
        {
            if (!canResume) return;

            IsResuming = true;
            PauseOverlay.Hide();

            // breaks and time-based conditions may allow instant resume.
            if (breakTracker.IsBreakTime.Value)
                completeResume();
            else
                DrawableRuleset.RequestResume(completeResume);

            void completeResume()
            {
                GameplayClockContainer.Start();
                IsResuming = false;
            }
        }

        #endregion

        #region Screen Logic

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            if (!LoadedBeatmapSuccessfully)
                return;

            Alpha = 0;
            this
                .ScaleTo(0.7f)
                .ScaleTo(1, 750, Easing.OutQuint)
                .Delay(250)
                .FadeIn(250);

            ApplyToBackground(b =>
            {
                b.IgnoreUserSettings.Value = false;
                b.BlurAmount.Value = 0;
                b.FadeColour(Color4.White, 250);

                // bind component bindables.
                ((IBindable<bool>)b.IsBreakTime).BindTo(breakTracker.IsBreakTime);

                b.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

                failAnimationContainer.Background = b;
            });

            HUDOverlay.IsPlaying.BindTo(localUserPlaying);
            ShowingOverlayComponents.BindTo(HUDOverlay.ShowHud);

            DimmableStoryboard.IsBreakTime.BindTo(breakTracker.IsBreakTime);

            storyboardReplacesBackground.Value = GameplayState.Storyboard.ReplacesBackground && GameplayState.Storyboard.HasDrawable;

            foreach (var mod in GameplayState.Mods.OfType<IApplicableToPlayer>())
                mod.ApplyToPlayer(this);

            foreach (var mod in GameplayState.Mods.OfType<IApplicableToHUD>())
                mod.ApplyToHUD(HUDOverlay);

            foreach (var mod in GameplayState.Mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(GameplayClockContainer.AdjustmentsFromMods);

            updateGameplayState();

            GameplayClockContainer.FadeInFromZero(750, Easing.OutQuint);

            StartGameplay();
            OnGameplayStarted?.Invoke();
        }

        /// <summary>
        /// Called to trigger the starting of the gameplay clock and underlying gameplay.
        /// This will be called on entering the player screen once. A derived class may block the first call to this to delay the start of gameplay.
        /// </summary>
        protected virtual void StartGameplay()
        {
            if (GameplayClockContainer.IsRunning)
                Logger.Error(new InvalidOperationException($"{nameof(StartGameplay)} should not be called when the gameplay clock is already running"), "Clock failure");

            GameplayClockContainer.Reset(startClock: true);

            if (Configuration.AutomaticallySkipIntro)
                skipIntroOverlay.SkipWhenReady();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            screenSuspension?.RemoveAndDisposeImmediately();

            fadeOut();
            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            screenSuspension?.RemoveAndDisposeImmediately();

            // Eagerly clean these up as disposal of child components is asynchronous and may leave sounds playing beyond user expectations.
            failAnimationContainer?.Stop();
            PauseOverlay?.StopAllSamples();

            if (LoadedBeatmapSuccessfully && !GameplayState.HasPassed)
            {
                Debug.Assert(resultsDisplayDelegate == null);

                if (!GameplayState.HasFailed)
                    GameplayState.HasQuit = true;

                if (DrawableRuleset.ReplayScore == null)
                    ScoreProcessor.FailScore(Score.ScoreInfo);
            }

            // GameplayClockContainer performs seeks / start / stop operations on the beatmap's track.
            // as we are no longer the current screen, we cannot guarantee the track is still usable.
            (GameplayClockContainer as MasterGameplayClockContainer)?.StopUsingBeatmapClock();

            musicController.ResetTrackAdjustments();

            fadeOut();

            return base.OnExiting(e);
        }

        /// <summary>
        /// Creates the player's <see cref="Scoring.Score"/>.
        /// </summary>
        /// <param name="beatmap"></param>
        /// <returns>The <see cref="Scoring.Score"/>.</returns>
        protected virtual Score CreateScore(IBeatmap beatmap) => new Score
        {
            ScoreInfo = new ScoreInfo
            {
                User = api.LocalUser.Value,
                ClientVersion = game.Version,
            },
        };

        /// <summary>
        /// Imports the player's <see cref="Scoring.Score"/> to the local database.
        /// </summary>
        /// <param name="score">The <see cref="Scoring.Score"/> to import.</param>
        /// <returns>The imported score.</returns>
        protected virtual Task ImportScore(Score score)
        {
            // Replays are already populated and present in the game's database, so should not be re-imported.
            if (DrawableRuleset.ReplayScore != null)
                return Task.CompletedTask;

            ByteArrayArchiveReader replayReader = null;

            if (score.ScoreInfo.Ruleset.IsLegacyRuleset())
            {
                using (var stream = new MemoryStream())
                {
                    new LegacyScoreEncoder(score, GameplayState.Beatmap).Encode(stream);
                    replayReader = new ByteArrayArchiveReader(stream.ToArray(), "replay.osr");
                }
            }

            // the import process will re-attach managed beatmap/rulesets to this score. we don't want this for now, so create a temporary copy to import.
            var importableScore = score.ScoreInfo.DeepClone();

            var imported = scoreManager.Import(importableScore, replayReader);

            imported.PerformRead(s =>
            {
                // because of the clone above, it's required that we copy back the post-import hash/ID to use for availability matching.
                score.ScoreInfo.Hash = s.Hash;
                score.ScoreInfo.ID = s.ID;
                score.ScoreInfo.Files.AddRange(s.Files.Detach());
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Prepare the <see cref="Scoring.Score"/> for display at results.
        /// </summary>
        /// <param name="score">The <see cref="Scoring.Score"/> to prepare.</param>
        /// <returns>A task that prepares the provided score. On completion, the score is assumed to be ready for display.</returns>
        protected virtual Task PrepareScoreForResultsAsync(Score score) => Task.CompletedTask;

        /// <summary>
        /// Creates the <see cref="ResultsScreen"/> for a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to be displayed in the results screen.</param>
        /// <returns>The <see cref="ResultsScreen"/>.</returns>
        protected abstract ResultsScreen CreateResults(ScoreInfo score);

        private void fadeOut(bool instant = false)
        {
            float fadeOutDuration = instant ? 0 : 250;
            this.FadeOut(fadeOutDuration);

            if (this.IsCurrentScreen())
            {
                ApplyToBackground(b =>
                {
                    b.IgnoreUserSettings.Value = true;

                    // May be null if the load never completed.
                    if (breakTracker != null)
                    {
                        b.IsBreakTime.UnbindFrom(breakTracker.IsBreakTime);
                        b.IsBreakTime.Value = false;
                    }
                });

                storyboardReplacesBackground.Value = false;
            }
        }

        #endregion

        IBindable<bool> ISamplePlaybackDisabler.SamplePlaybackDisabled => samplePlaybackDisabled;

        IBindable<bool> ILocalUserPlayInfo.IsPlaying => LocalUserPlaying;
    }
}
