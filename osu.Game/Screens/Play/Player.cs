// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    [Cached]
    [Cached(typeof(ISamplePlaybackDisabler))]
    public abstract class Player : ScreenWithBeatmapBackground, ISamplePlaybackDisabler, ILocalUserPlayInfo
    {
        /// <summary>
        /// The delay upon completion of the beatmap before displaying the results screen.
        /// </summary>
        public const double RESULTS_DISPLAY_DELAY = 1000.0;

        public override bool AllowBackButton => false; // handled by HoldForMenuButton

        protected override UserActivity InitialActivity => new UserActivity.InSoloGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public override float BackgroundParallaxAmount => 0.1f;

        public override bool HideOverlaysOnEnter => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        // We are managing our own adjustments (see OnEntering/OnExiting).
        public override bool? AllowTrackAdjustments => false;

        private readonly IBindable<bool> gameActive = new Bindable<bool>(true);

        private readonly Bindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        /// <summary>
        /// Whether gameplay should pause when the game window focus is lost.
        /// </summary>
        protected virtual bool PauseOnFocusLost => true;

        /// <summary>
        /// Whether gameplay has completed without the user having failed.
        /// </summary>
        public bool GameplayPassed { get; private set; }

        public Action RestartRequested;

        public bool HasFailed { get; private set; }

        private Bindable<bool> mouseWheelDisabled;

        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();

        public IBindable<bool> LocalUserPlaying => localUserPlaying;

        private readonly Bindable<bool> localUserPlaying = new Bindable<bool>();

        public int RestartCount;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        public GameplayState GameplayState { get; private set; }

        private Ruleset ruleset;

        private Sample sampleRestart;

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

        protected Score Score { get; private set; }

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

            ScoreProcessor.NewJudgement += result => ScoreProcessor.PopulateScore(Score.ScoreInfo);

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
        private void load(AudioManager audio, OsuConfigManager config, OsuGameBase game)
        {
            var gameplayMods = Mods.Value.Select(m => m.DeepClone()).ToArray();

            if (Beatmap.Value is DummyWorkingBeatmap)
                return;

            IBeatmap playableBeatmap = loadPlayableBeatmap(gameplayMods);

            if (playableBeatmap == null)
                return;

            sampleRestart = audio.Samples.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            if (game != null)
                gameActive.BindTo(game.IsActive);

            if (game is OsuGame osuGame)
                LocalUserPlaying.BindTo(osuGame.LocalUserPlaying);

            DrawableRuleset = ruleset.CreateDrawableRulesetWith(playableBeatmap, gameplayMods);
            dependencies.CacheAs(DrawableRuleset);

            ScoreProcessor = ruleset.CreateScoreProcessor();
            ScoreProcessor.ApplyBeatmap(playableBeatmap);
            ScoreProcessor.Mods.Value = gameplayMods;

            dependencies.CacheAs(ScoreProcessor);

            HealthProcessor = ruleset.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime);
            HealthProcessor.ApplyBeatmap(playableBeatmap);

            dependencies.CacheAs(HealthProcessor);

            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            InternalChild = GameplayClockContainer = CreateGameplayClockContainer(Beatmap.Value, DrawableRuleset.GameplayStartTime);

            AddInternal(screenSuspension = new ScreenSuspensionHandler(GameplayClockContainer));

            Score = CreateScore(playableBeatmap);

            // ensure the score is in a consistent state with the current player.
            Score.ScoreInfo.BeatmapInfo = Beatmap.Value.BeatmapInfo;
            Score.ScoreInfo.Ruleset = ruleset.RulesetInfo;
            Score.ScoreInfo.Mods = gameplayMods;

            dependencies.CacheAs(GameplayState = new GameplayState(playableBeatmap, ruleset, gameplayMods, Score));

            var rulesetSkinProvider = new RulesetSkinProvidingContainer(ruleset, playableBeatmap, Beatmap.Value.Skin);

            // load the skinning hierarchy first.
            // this is intentionally done in two stages to ensure things are in a loaded state before exposing the ruleset to skin sources.
            GameplayClockContainer.Add(rulesetSkinProvider);

            rulesetSkinProvider.AddRange(new Drawable[]
            {
                failAnimationLayer = new FailAnimation(DrawableRuleset)
                {
                    OnComplete = onFailComplete,
                    Children = new[]
                    {
                        // underlay and gameplay should have access to the skinning sources.
                        createUnderlayComponents(),
                        createGameplayComponents(Beatmap.Value, playableBeatmap)
                    }
                },
                FailOverlay = new FailOverlay
                {
                    OnRetry = Restart,
                    OnQuit = () => PerformExit(true),
                },
                new HotkeyExitOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        fadeOut(true);
                        PerformExit(false);
                    },
                },
            });

            if (Configuration.AllowRestart)
            {
                rulesetSkinProvider.Add(new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        fadeOut(true);
                        Restart();
                    },
                });
            }

            // add the overlay components as a separate step as they proxy some elements from the above underlay/gameplay components.
            // also give the overlays the ruleset skin provider to allow rulesets to potentially override HUD elements (used to disable combo counters etc.)
            // we may want to limit this in the future to disallow rulesets from outright replacing elements the user expects to be there.
            failAnimationLayer.Add(createOverlayComponents(Beatmap.Value));

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

            DrawableRuleset.IsPaused.BindValueChanged(paused =>
            {
                updateGameplayState();
                updateSampleDisabledState();
            });

            DrawableRuleset.FrameStableClock.IsCatchingUp.BindValueChanged(_ => updateSampleDisabledState());

            DrawableRuleset.HasReplayLoaded.BindValueChanged(_ => updateGameplayState());

            // bind clock into components that require it
            DrawableRuleset.IsPaused.BindTo(GameplayClockContainer.IsPaused);

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

            DimmableStoryboard.HasStoryboardEnded.ValueChanged += storyboardEnded =>
            {
                if (storyboardEnded.NewValue)
                    progressToResults(true);
            };

            // Bind the judgement processors to ourselves
            ScoreProcessor.HasCompleted.BindValueChanged(scoreCompletionChanged);
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
        }

        protected virtual GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart) => new MasterGameplayClockContainer(beatmap, gameplayStart);

        private Drawable createUnderlayComponents() =>
            DimmableStoryboard = new DimmableStoryboard(Beatmap.Value.Storyboard) { RelativeSizeAxes = Axes.Both };

        private Drawable createGameplayComponents(IWorkingBeatmap working, IBeatmap playableBeatmap) => new ScalingContainer(ScalingMode.Gameplay)
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
                    HUDOverlay = new HUDOverlay(DrawableRuleset, GameplayState.Mods)
                    {
                        HoldToQuit =
                        {
                            Action = () => PerformExit(true),
                            IsPaused = { BindTarget = GameplayClockContainer.IsPaused }
                        },
                        KeyCounter =
                        {
                            AlwaysVisible = { BindTarget = DrawableRuleset.HasReplayLoaded },
                            IsCounting = false
                        },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    skipIntroOverlay = new SkipOverlay(DrawableRuleset.GameplayStartTime)
                    {
                        RequestSkip = performUserRequestedSkip
                    },
                    skipOutroOverlay = new SkipOverlay(Beatmap.Value.Storyboard.LatestEventTime ?? 0)
                    {
                        RequestSkip = () => progressToResults(false),
                        Alpha = 0
                    },
                    PauseOverlay = new PauseOverlay
                    {
                        OnResume = Resume,
                        Retries = RestartCount,
                        OnRetry = Restart,
                        OnQuit = () => PerformExit(true),
                    },
                },
            };

            if (!Configuration.AllowSkipping || !DrawableRuleset.AllowGameplayOverlays)
            {
                skipIntroOverlay.Expire();
                skipOutroOverlay.Expire();
            }

            if (GameplayClockContainer is MasterGameplayClockContainer master)
                HUDOverlay.PlayerSettingsOverlay.PlaybackSettings.UserPlaybackRate.BindTarget = master.UserPlaybackRate;

            return container;
        }

        private void onBreakTimeChanged(ValueChangedEvent<bool> isBreakTime)
        {
            updateGameplayState();
            updatePauseOnFocusLostState();
            HUDOverlay.KeyCounter.IsCounting = !isBreakTime.NewValue;
        }

        private void updateGameplayState()
        {
            bool inGameplay = !DrawableRuleset.HasReplayLoaded.Value && !DrawableRuleset.IsPaused.Value && !breakTracker.IsBreakTime.Value;
            OverlayActivationMode.Value = inGameplay ? OverlayActivation.Disabled : OverlayActivation.UserTriggered;
            localUserPlaying.Value = inGameplay;
        }

        private void updateSampleDisabledState()
        {
            samplePlaybackDisabled.Value = DrawableRuleset.FrameStableClock.IsCatchingUp.Value || GameplayClockContainer.GameplayClock.IsPaused.Value;
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

        private IBeatmap loadPlayableBeatmap(Mod[] gameplayMods)
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
                    playable = Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, gameplayMods);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // A playable beatmap may not be creatable with the user's preferred ruleset, so try using the beatmap's default ruleset
                    rulesetInfo = Beatmap.Value.BeatmapInfo.Ruleset;
                    ruleset = rulesetInfo.CreateInstance();

                    playable = Beatmap.Value.GetPlayableBeatmap(rulesetInfo, gameplayMods);
                }

                if (playable.HitObjects.Count == 0)
                {
                    Logger.Log("Beatmap contains no hit objects!", level: LogLevel.Error);
                    return null;
                }
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
        protected void PerformExit(bool showDialogFirst)
        {
            // if an exit has been requested, cancel any pending completion (the user has shown intention to exit).
            resultsDisplayDelegate?.Cancel();

            // there is a chance that an exit request occurs after the transition to results has already started.
            // even in such a case, the user has shown intent, so forcefully return to this screen (to proceed with the upwards exit process).
            if (!this.IsCurrentScreen())
            {
                ValidForResume = false;

                // in the potential case that this instance has already been exited, this is required to avoid a crash.
                if (this.GetChildScreen() != null)
                    this.MakeCurrent();
                return;
            }

            bool pauseOrFailDialogVisible =
                PauseOverlay.State.Value == Visibility.Visible || FailOverlay.State.Value == Visibility.Visible;

            if (showDialogFirst && !pauseOrFailDialogVisible)
            {
                // if the fail animation is currently in progress, accelerate it (it will show the pause dialog on completion).
                if (ValidForResume && HasFailed)
                {
                    failAnimationLayer.FinishTransforms(true);
                    return;
                }

                // even if this call has requested a dialog, there is a chance the current player mode doesn't support pausing.
                if (pausingSupportedByCurrentState)
                {
                    // in the case a dialog needs to be shown, attempt to pause and show it.
                    // this may fail (see internal checks in Pause()) but the fail cases are temporary, so don't fall through to Exit().
                    Pause();
                    return;
                }
            }

            // The actual exit is performed if
            // - the pause / fail dialog was not requested
            // - the pause / fail dialog was requested but is already displayed (user showing intention to exit).
            // - the pause / fail dialog was requested but couldn't be displayed due to the type or state of this Player instance.
            this.Exit();
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
        /// Seeks to a specific time in gameplay, bypassing frame stability.
        /// </summary>
        /// <remarks>
        /// Intermediate hitobject judgements may not be applied or reverted correctly during this seek.
        /// </remarks>
        /// <param name="time">The destination time to seek to.</param>
        internal void NonFrameStableSeek(double time)
        {
            if (frameStablePlaybackResetDelegate?.Cancelled == false && !frameStablePlaybackResetDelegate.Completed)
                frameStablePlaybackResetDelegate.RunTask();

            bool wasFrameStable = DrawableRuleset.FrameStablePlayback;
            DrawableRuleset.FrameStablePlayback = false;

            Seek(time);

            // Delay resetting frame-stable playback for one frame to give the FrameStabilityContainer a chance to seek.
            frameStablePlaybackResetDelegate = ScheduleAfterChildren(() => DrawableRuleset.FrameStablePlayback = wasFrameStable);
        }

        /// <summary>
        /// Restart gameplay via a parent <see cref="PlayerLoader"/>.
        /// <remarks>This can be called from a child screen in order to trigger the restart process.</remarks>
        /// </summary>
        public void Restart()
        {
            if (!Configuration.AllowRestart)
                return;

            // at the point of restarting the track should either already be paused or the volume should be zero.
            // stopping here is to ensure music doesn't become audible after exiting back to PlayerLoader.
            musicController.Stop();

            sampleRestart?.Play();
            RestartRequested?.Invoke();

            PerformExit(false);
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
        /// <exception cref="InvalidOperationException">Thrown if this method is called more than once without changing state.</exception>
        private void scoreCompletionChanged(ValueChangedEvent<bool> completed)
        {
            // If this player instance is in the middle of an exit, don't attempt any kind of state update.
            if (!this.IsCurrentScreen())
                return;

            // Special case to handle rewinding post-completion. This is the only way already queued forward progress can be cancelled.
            // TODO: Investigate whether this can be moved to a RewindablePlayer subclass or similar.
            // Currently, even if this scenario is hit, prepareScoreForDisplay has already been queued (and potentially run).
            // In scenarios where rewinding is possible (replay, spectating) this is a non-issue as no submission/import work is done,
            // but it still doesn't feel right that this exists here.
            if (!completed.NewValue)
            {
                resultsDisplayDelegate?.Cancel();
                resultsDisplayDelegate = null;

                GameplayPassed = false;
                ValidForResume = true;
                skipOutroOverlay.Hide();
                return;
            }

            // Only show the completion screen if the player hasn't failed
            if (HealthProcessor.HasFailed)
                return;

            GameplayPassed = true;

            // Setting this early in the process means that even if something were to go wrong in the order of events following, there
            // is no chance that a user could return to the (already completed) Player instance from a child screen.
            ValidForResume = false;

            // Ensure we are not writing to the replay any more, as we are about to consume and store the score.
            DrawableRuleset.SetRecordTarget(null);

            if (!Configuration.ShowResults)
                return;

            prepareScoreForDisplayTask ??= Task.Run(prepareScoreForResults);

            bool storyboardHasOutro = DimmableStoryboard.ContentDisplayed && !DimmableStoryboard.HasStoryboardEnded.Value;

            if (storyboardHasOutro)
            {
                // if the current beatmap has a storyboard, the progression to results will be handled by the storyboard ending
                // or the user pressing the skip outro button.
                skipOutroOverlay.Show();
                return;
            }

            progressToResults(true);
        }

        /// <summary>
        /// Asynchronously run score preparation operations (database import, online submission etc.).
        /// </summary>
        /// <returns>The final score.</returns>
        private async Task<ScoreInfo> prepareScoreForResults()
        {
            var scoreCopy = Score.DeepClone();

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
        }

        /// <summary>
        /// Queue the results screen for display.
        /// </summary>
        /// <remarks>
        /// A final display will only occur once all work is completed in <see cref="PrepareScoreForResultsAsync"/>. This means that even after calling this method, the results screen will never be shown until <see cref="JudgementProcessor.HasCompleted">ScoreProcessor.HasCompleted</see> becomes <see langword="true"/>.
        ///
        /// Calling this method multiple times will have no effect.
        /// </remarks>
        /// <param name="withDelay">Whether a minimum delay (<see cref="RESULTS_DISPLAY_DELAY"/>) should be added before the screen is displayed.</param>
        private void progressToResults(bool withDelay)
        {
            if (resultsDisplayDelegate != null)
                // Note that if progressToResults is called one withDelay=true and then withDelay=false, this no-delay timing will not be
                // accounted for. shouldn't be a huge concern (a user pressing the skip button after a results progression has already been queued
                // may take x00 more milliseconds than expected in the very rare edge case).
                //
                // If required we can handle this more correctly by rescheduling here.
                return;

            double delay = withDelay ? RESULTS_DISPLAY_DELAY : 0;

            resultsDisplayDelegate = new ScheduledDelegate(() =>
            {
                if (prepareScoreForDisplayTask?.IsCompleted != true)
                    // If the asynchronous preparation has not completed, keep repeating this delegate.
                    return;

                resultsDisplayDelegate?.Cancel();

                if (!this.IsCurrentScreen())
                    // This player instance may already be in the process of exiting.
                    return;

                this.Push(CreateResults(prepareScoreForDisplayTask.GetResultSafely()));
            }, Time.Current + delay, 50);

            Scheduler.Add(resultsDisplayDelegate);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // During pause, allow global volume adjust regardless of settings.
            if (GameplayClockContainer.IsPaused.Value)
                return false;

            // Block global volume adjust if the user has asked for it (special case when holding "Alt").
            return mouseWheelDisabled.Value && !e.AltPressed;
        }

        #region Fail Logic

        protected FailOverlay FailOverlay { get; private set; }

        private FailAnimation failAnimationLayer;

        private bool onFail()
        {
            if (!CheckModsAllowFailure())
                return false;

            HasFailed = true;
            Score.ScoreInfo.Passed = false;

            // There is a chance that we could be in a paused state as the ruleset's internal clock (see FrameStabilityContainer)
            // could process an extra frame after the GameplayClock is stopped.
            // In such cases we want the fail state to precede a user triggered pause.
            if (PauseOverlay.State.Value == Visibility.Visible)
                PauseOverlay.Hide();

            failAnimationLayer.Start();

            if (GameplayState.Mods.OfType<IApplicableFailOverride>().Any(m => m.RestartOnFail))
                Restart();

            return true;
        }

        // Called back when the transform finishes
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
            lastPauseActionTime.HasValue && GameplayClockContainer.GameplayClock.CurrentTime < lastPauseActionTime + pause_cooldown;

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
            && !HasFailed;

        private bool canResume =>
            // cannot resume from a non-paused state
            GameplayClockContainer.IsPaused.Value
            // cannot resume if we are already in a fail state
            && !HasFailed
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
            lastPauseActionTime = GameplayClockContainer.GameplayClock.CurrentTime;
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

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

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
                b.IsBreakTime.BindTo(breakTracker.IsBreakTime);

                b.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

                failAnimationLayer.Background = b;
            });

            HUDOverlay.IsBreakTime.BindTo(breakTracker.IsBreakTime);
            DimmableStoryboard.IsBreakTime.BindTo(breakTracker.IsBreakTime);

            DimmableStoryboard.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

            storyboardReplacesBackground.Value = Beatmap.Value.Storyboard.ReplacesBackground && Beatmap.Value.Storyboard.HasDrawable;

            foreach (var mod in GameplayState.Mods.OfType<IApplicableToPlayer>())
                mod.ApplyToPlayer(this);

            foreach (var mod in GameplayState.Mods.OfType<IApplicableToHUD>())
                mod.ApplyToHUD(HUDOverlay);

            // Our mods are local copies of the global mods so they need to be re-applied to the track.
            // This is done through the music controller (for now), because resetting speed adjustments on the beatmap track also removes adjustments provided by DrawableTrack.
            // Todo: In the future, player will receive in a track and will probably not have to worry about this...
            musicController.ResetTrackAdjustments();
            foreach (var mod in GameplayState.Mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(musicController.CurrentTrack);

            updateGameplayState();

            GameplayClockContainer.FadeInFromZero(750, Easing.OutQuint);
            StartGameplay();
        }

        /// <summary>
        /// Called to trigger the starting of the gameplay clock and underlying gameplay.
        /// This will be called on entering the player screen once. A derived class may block the first call to this to delay the start of gameplay.
        /// </summary>
        protected virtual void StartGameplay()
        {
            if (GameplayClockContainer.GameplayClock.IsRunning)
                throw new InvalidOperationException($"{nameof(StartGameplay)} should not be called when the gameplay clock is already running");

            GameplayClockContainer.Reset();
        }

        public override void OnSuspending(IScreen next)
        {
            screenSuspension?.RemoveAndDisposeImmediately();

            fadeOut();
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            screenSuspension?.RemoveAndDisposeImmediately();
            failAnimationLayer?.RemoveFilters();

            // if arriving here and the results screen preparation task hasn't run, it's safe to say the user has not completed the beatmap.
            if (prepareScoreForDisplayTask == null)
            {
                Score.ScoreInfo.Passed = false;
                // potentially should be ScoreRank.F instead? this is the best alternative for now.
                Score.ScoreInfo.Rank = ScoreRank.D;
            }

            // EndPlaying() is typically called from ReplayRecorder.Dispose(). Disposal is currently asynchronous.
            // To resolve test failures, forcefully end playing synchronously when this screen exits.
            // Todo: Replace this with a more permanent solution once osu-framework has a synchronous cleanup method.
            spectatorClient.EndPlaying();

            // GameplayClockContainer performs seeks / start / stop operations on the beatmap's track.
            // as we are no longer the current screen, we cannot guarantee the track is still usable.
            (GameplayClockContainer as MasterGameplayClockContainer)?.StopUsingBeatmapClock();

            musicController.ResetTrackAdjustments();

            fadeOut();
            return base.OnExiting(next);
        }

        /// <summary>
        /// Creates the player's <see cref="Scoring.Score"/>.
        /// </summary>
        /// <param name="beatmap"></param>
        /// <returns>The <see cref="Scoring.Score"/>.</returns>
        protected virtual Score CreateScore(IBeatmap beatmap) => new Score
        {
            ScoreInfo = new ScoreInfo { User = api.LocalUser.Value },
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

            LegacyByteArrayReader replayReader;

            using (var stream = new MemoryStream())
            {
                new LegacyScoreEncoder(score, GameplayState.Beatmap).Encode(stream);
                replayReader = new LegacyByteArrayReader(stream.ToArray(), "replay.osr");
            }

            // the import process will re-attach managed beatmap/rulesets to this score. we don't want this for now, so create a temporary copy to import.
            var importableScore = score.ScoreInfo.DeepClone();

            // For the time being, online ID responses are not really useful for anything.
            // In addition, the IDs provided via new (lazer) endpoints are based on a different autoincrement from legacy (stable) scores.
            //
            // Until we better define the server-side logic behind this, let's not store the online ID to avoid potential unique constraint
            // conflicts across various systems (ie. solo and multiplayer).
            importableScore.OnlineID = -1;

            var imported = scoreManager.Import(importableScore, replayReader);

            imported.PerformRead(s =>
            {
                // because of the clone above, it's required that we copy back the post-import hash/ID to use for availability matching.
                score.ScoreInfo.Hash = s.Hash;
                score.ScoreInfo.ID = s.ID;
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
        protected virtual ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score, true);

        private void fadeOut(bool instant = false)
        {
            float fadeOutDuration = instant ? 0 : 250;
            this.FadeOut(fadeOutDuration);

            ApplyToBackground(b => b.IgnoreUserSettings.Value = true);
            storyboardReplacesBackground.Value = false;
        }

        #endregion

        IBindable<bool> ISamplePlaybackDisabler.SamplePlaybackDisabled => samplePlaybackDisabled;

        IBindable<bool> ILocalUserPlayInfo.IsPlaying => LocalUserPlaying;
    }
}
