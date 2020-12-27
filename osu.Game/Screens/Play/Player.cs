// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
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
using osu.Game.Overlays;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    [Cached]
    [Cached(typeof(ISamplePlaybackDisabler))]
    public class Player : ScreenWithBeatmapBackground, ISamplePlaybackDisabler
    {
        /// <summary>
        /// The delay upon completion of the beatmap before displaying the results screen.
        /// </summary>
        public const double RESULTS_DISPLAY_DELAY = 1000.0;

        public override bool AllowBackButton => false; // handled by HoldForMenuButton

        protected override UserActivity InitialActivity => new UserActivity.SoloGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public override float BackgroundParallaxAmount => 0.1f;

        public override bool HideOverlaysOnEnter => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        // We are managing our own adjustments (see OnEntering/OnExiting).
        public override bool AllowRateAdjustments => false;

        private readonly Bindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        /// <summary>
        /// Whether gameplay should pause when the game window focus is lost.
        /// </summary>
        protected virtual bool PauseOnFocusLost => true;

        public Action RestartRequested;

        public bool HasFailed { get; private set; }

        private Bindable<bool> mouseWheelDisabled;

        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();

        protected readonly Bindable<bool> LocalUserPlaying = new Bindable<bool>();

        public int RestartCount;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        private RulesetInfo rulesetInfo;

        private Ruleset ruleset;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        private SampleChannel sampleRestart;

        public BreakOverlay BreakOverlay;

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public readonly IBindable<bool> IsBreakTime = new BindableBool();

        private BreakTracker breakTracker;

        private SkipOverlay skipOverlay;

        protected ScoreProcessor ScoreProcessor { get; private set; }

        protected HealthProcessor HealthProcessor { get; private set; }

        protected DrawableRuleset DrawableRuleset { get; private set; }

        protected HUDOverlay HUDOverlay { get; private set; }

        public bool LoadedBeatmapSuccessfully => DrawableRuleset?.Objects.Any() == true;

        protected GameplayClockContainer GameplayClockContainer { get; private set; }

        public DimmableStoryboard DimmableStoryboard { get; private set; }

        [Cached]
        [Cached(Type = typeof(IBindable<IReadOnlyList<Mod>>))]
        protected new readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// Whether failing should be allowed.
        /// By default, this checks whether all selected mods allow failing.
        /// </summary>
        protected virtual bool CheckModsAllowFailure() => Mods.Value.OfType<IApplicableFailOverride>().All(m => m.PerformFail());

        public readonly PlayerConfiguration Configuration;

        /// <summary>
        /// Create a new player instance.
        /// </summary>
        public Player(PlayerConfiguration configuration = null)
        {
            Configuration = configuration ?? new PlayerConfiguration();
        }

        private GameplayBeatmap gameplayBeatmap;

        private ScreenSuspensionHandler screenSuspension;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // replays should never be recorded or played back when autoplay is enabled
            if (!Mods.Value.Any(m => m is ModAutoplay))
                PrepareReplay();
        }

        [CanBeNull]
        private Score recordingScore;

        /// <summary>
        /// Run any recording / playback setup for replays.
        /// </summary>
        protected virtual void PrepareReplay()
        {
            DrawableRuleset.SetRecordTarget(recordingScore = new Score());

            ScoreProcessor.NewJudgement += result => ScoreProcessor.PopulateScore(recordingScore.ScoreInfo);
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, OsuConfigManager config, OsuGame game)
        {
            Mods.Value = base.Mods.Value.Select(m => m.CreateCopy()).ToArray();

            if (Beatmap.Value is DummyWorkingBeatmap)
                return;

            IBeatmap playableBeatmap = loadPlayableBeatmap();

            if (playableBeatmap == null)
                return;

            sampleRestart = audio.Samples.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            if (game != null)
                LocalUserPlaying.BindTo(game.LocalUserPlaying);

            DrawableRuleset = ruleset.CreateDrawableRulesetWith(playableBeatmap, Mods.Value);

            ScoreProcessor = ruleset.CreateScoreProcessor();
            ScoreProcessor.ApplyBeatmap(playableBeatmap);
            ScoreProcessor.Mods.BindTo(Mods);

            HealthProcessor = ruleset.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime);
            HealthProcessor.ApplyBeatmap(playableBeatmap);

            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            InternalChild = GameplayClockContainer = CreateGameplayClockContainer(Beatmap.Value, DrawableRuleset.GameplayStartTime);

            AddInternal(gameplayBeatmap = new GameplayBeatmap(playableBeatmap));
            AddInternal(screenSuspension = new ScreenSuspensionHandler(GameplayClockContainer));

            dependencies.CacheAs(gameplayBeatmap);

            var beatmapSkinProvider = new BeatmapSkinProvidingContainer(Beatmap.Value.Skin);

            // the beatmapSkinProvider is used as the fallback source here to allow the ruleset-specific skin implementation
            // full access to all skin sources.
            var rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, playableBeatmap));

            // load the skinning hierarchy first.
            // this is intentionally done in two stages to ensure things are in a loaded state before exposing the ruleset to skin sources.
            GameplayClockContainer.Add(beatmapSkinProvider.WithChild(rulesetSkinProvider));

            rulesetSkinProvider.AddRange(new[]
            {
                // underlay and gameplay should have access the to skinning sources.
                createUnderlayComponents(),
                createGameplayComponents(Beatmap.Value, playableBeatmap)
            });

            // also give the HUD a ruleset container to allow rulesets to potentially override HUD elements (used to disable combo counters etc.)
            // we may want to limit this in the future to disallow rulesets from outright replacing elements the user expects to be there.
            var hudRulesetContainer = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider, playableBeatmap));

            // add the overlay components as a separate step as they proxy some elements from the above underlay/gameplay components.
            GameplayClockContainer.Add(hudRulesetContainer.WithChild(createOverlayComponents(Beatmap.Value)));

            if (!DrawableRuleset.AllowGameplayOverlays)
            {
                HUDOverlay.ShowHud.Value = false;
                HUDOverlay.ShowHud.Disabled = true;
                BreakOverlay.Hide();
                skipOverlay.Hide();
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

            DrawableRuleset.HasReplayLoaded.BindValueChanged(_ => updatePauseOnFocusLostState(), true);

            // bind clock into components that require it
            DrawableRuleset.IsPaused.BindTo(GameplayClockContainer.IsPaused);

            DrawableRuleset.NewResult += r =>
            {
                HealthProcessor.ApplyResult(r);
                ScoreProcessor.ApplyResult(r);
                gameplayBeatmap.ApplyResult(r);
            };

            DrawableRuleset.RevertResult += r =>
            {
                HealthProcessor.RevertResult(r);
                ScoreProcessor.RevertResult(r);
            };

            // Bind the judgement processors to ourselves
            ScoreProcessor.HasCompleted.ValueChanged += updateCompletionState;
            HealthProcessor.Failed += onFail;

            foreach (var mod in Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(ScoreProcessor);

            foreach (var mod in Mods.Value.OfType<IApplicableToHealthProcessor>())
                mod.ApplyToHealthProcessor(HealthProcessor);

            IsBreakTime.BindTo(breakTracker.IsBreakTime);
            IsBreakTime.BindValueChanged(onBreakTimeChanged, true);
        }

        protected virtual GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart) => new GameplayClockContainer(beatmap, gameplayStart);

        private Drawable createUnderlayComponents() =>
            DimmableStoryboard = new DimmableStoryboard(Beatmap.Value.Storyboard) { RelativeSizeAxes = Axes.Both };

        private Drawable createGameplayComponents(WorkingBeatmap working, IBeatmap playableBeatmap) => new ScalingContainer(ScalingMode.Gameplay)
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

        private Drawable createOverlayComponents(WorkingBeatmap working)
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
                    HUDOverlay = new HUDOverlay(ScoreProcessor, HealthProcessor, DrawableRuleset, Mods.Value)
                    {
                        HoldToQuit =
                        {
                            Action = performUserRequestedExit,
                            IsPaused = { BindTarget = GameplayClockContainer.IsPaused }
                        },
                        PlayerSettingsOverlay = { PlaybackSettings = { UserPlaybackRate = { BindTarget = GameplayClockContainer.UserPlaybackRate } } },
                        KeyCounter =
                        {
                            AlwaysVisible = { BindTarget = DrawableRuleset.HasReplayLoaded },
                            IsCounting = false
                        },
                        RequestSeek = time =>
                        {
                            GameplayClockContainer.Seek(time);
                            GameplayClockContainer.Start();
                        },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    skipOverlay = new SkipOverlay(DrawableRuleset.GameplayStartTime)
                    {
                        RequestSkip = GameplayClockContainer.Skip
                    },
                    FailOverlay = new FailOverlay
                    {
                        OnRetry = Restart,
                        OnQuit = performUserRequestedExit,
                    },
                    PauseOverlay = new PauseOverlay
                    {
                        OnResume = Resume,
                        Retries = RestartCount,
                        OnRetry = Restart,
                        OnQuit = performUserRequestedExit,
                    },
                    new HotkeyExitOverlay
                    {
                        Action = () =>
                        {
                            if (!this.IsCurrentScreen()) return;

                            fadeOut(true);
                            PerformExit(true);
                        },
                    },
                    failAnimation = new FailAnimation(DrawableRuleset) { OnComplete = onFailComplete, },
                }
            };

            if (!Configuration.AllowSkippingIntro)
                skipOverlay.Expire();

            if (Configuration.AllowRestart)
            {
                container.Add(new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        fadeOut(true);
                        Restart();
                    },
                });
            }

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
            LocalUserPlaying.Value = inGameplay;
        }

        private void updateSampleDisabledState()
        {
            samplePlaybackDisabled.Value = DrawableRuleset.FrameStableClock.IsCatchingUp.Value || GameplayClockContainer.GameplayClock.IsPaused.Value;
        }

        private void updatePauseOnFocusLostState() =>
            HUDOverlay.HoldToQuit.PauseOnFocusLost = PauseOnFocusLost
                                                     && !DrawableRuleset.HasReplayLoaded.Value
                                                     && !breakTracker.IsBreakTime.Value;

        private IBeatmap loadPlayableBeatmap()
        {
            IBeatmap playable;

            try
            {
                if (Beatmap.Value.Beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                rulesetInfo = Ruleset.Value ?? Beatmap.Value.BeatmapInfo.Ruleset;
                ruleset = rulesetInfo.CreateInstance();

                try
                {
                    playable = Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, Mods.Value);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // A playable beatmap may not be creatable with the user's preferred ruleset, so try using the beatmap's default ruleset
                    rulesetInfo = Beatmap.Value.BeatmapInfo.Ruleset;
                    ruleset = rulesetInfo.CreateInstance();

                    playable = Beatmap.Value.GetPlayableBeatmap(rulesetInfo, Mods.Value);
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
        /// Exits the <see cref="Player"/>.
        /// </summary>
        /// <param name="userRequested">
        /// Whether the exit is requested by the user, or a higher-level game component.
        /// Pausing is allowed only in the former case.
        /// </param>
        protected void PerformExit(bool userRequested)
        {
            // if a restart has been requested, cancel any pending completion (user has shown intent to restart).
            completionProgressDelegate?.Cancel();

            ValidForResume = false;

            if (!this.IsCurrentScreen()) return;

            if (userRequested)
                performUserRequestedExit();
            else
                this.Exit();
        }

        private void performUserRequestedExit()
        {
            if (ValidForResume && HasFailed && !FailOverlay.IsPresent)
            {
                failAnimation.FinishTransforms(true);
                return;
            }

            if (canPause)
                Pause();
            else
                this.Exit();
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

            if (this.IsCurrentScreen())
                PerformExit(true);
            else
                this.MakeCurrent();
        }

        private ScheduledDelegate completionProgressDelegate;
        private Task<ScoreInfo> scoreSubmissionTask;

        private void updateCompletionState(ValueChangedEvent<bool> completionState)
        {
            // screen may be in the exiting transition phase.
            if (!this.IsCurrentScreen())
                return;

            if (!completionState.NewValue)
            {
                completionProgressDelegate?.Cancel();
                completionProgressDelegate = null;
                ValidForResume = true;
                return;
            }

            if (completionProgressDelegate != null)
                throw new InvalidOperationException($"{nameof(updateCompletionState)} was fired more than once");

            // Only show the completion screen if the player hasn't failed
            if (HealthProcessor.HasFailed)
                return;

            ValidForResume = false;

            if (!Configuration.ShowResults) return;

            scoreSubmissionTask ??= Task.Run(async () =>
            {
                var score = CreateScore();

                try
                {
                    await SubmitScore(score);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Score submission failed!");
                }

                try
                {
                    await ImportScore(score);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Score import failed!");
                }

                return score.ScoreInfo;
            });

            using (BeginDelayedSequence(RESULTS_DISPLAY_DELAY))
                scheduleCompletion();
        }

        private void scheduleCompletion() => completionProgressDelegate = Schedule(() =>
        {
            if (!scoreSubmissionTask.IsCompleted)
            {
                scheduleCompletion();
                return;
            }

            // screen may be in the exiting transition phase.
            if (this.IsCurrentScreen())
                this.Push(CreateResults(scoreSubmissionTask.Result));
        });

        protected override bool OnScroll(ScrollEvent e) => mouseWheelDisabled.Value && !GameplayClockContainer.IsPaused.Value;

        #region Fail Logic

        protected FailOverlay FailOverlay { get; private set; }

        private FailAnimation failAnimation;

        private bool onFail()
        {
            if (!CheckModsAllowFailure())
                return false;

            HasFailed = true;

            // There is a chance that we could be in a paused state as the ruleset's internal clock (see FrameStabilityContainer)
            // could process an extra frame after the GameplayClock is stopped.
            // In such cases we want the fail state to precede a user triggered pause.
            if (PauseOverlay.State.Value == Visibility.Visible)
                PauseOverlay.Hide();

            failAnimation.Start();

            if (Mods.Value.OfType<IApplicableFailOverride>().Any(m => m.RestartOnFail))
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

        private bool canPause =>
            // must pass basic screen conditions (beatmap loaded, instance allows pause)
            LoadedBeatmapSuccessfully && Configuration.AllowPause && ValidForResume
            // replays cannot be paused and exit immediately
            && !DrawableRuleset.HasReplayLoaded.Value
            // cannot pause if we are already in a fail state
            && !HasFailed
            // cannot pause if already paused (or in a cooldown state) unless we are in a resuming state.
            && (IsResuming || (GameplayClockContainer.IsPaused.Value == false && !pauseCooldownActive));

        private bool pauseCooldownActive =>
            lastPauseActionTime.HasValue && GameplayClockContainer.GameplayClock.CurrentTime < lastPauseActionTime + pause_cooldown;

        private bool canResume =>
            // cannot resume from a non-paused state
            GameplayClockContainer.IsPaused.Value
            // cannot resume if we are already in a fail state
            && !HasFailed
            // already resuming
            && !IsResuming;

        public void Pause()
        {
            if (!canPause) return;

            if (IsResuming)
            {
                DrawableRuleset.CancelResume();
                IsResuming = false;
            }

            GameplayClockContainer.Stop();
            PauseOverlay.Show();
            lastPauseActionTime = GameplayClockContainer.GameplayClock.CurrentTime;
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

            Background.EnableUserDim.Value = true;
            Background.BlurAmount.Value = 0;

            // bind component bindables.
            Background.IsBreakTime.BindTo(breakTracker.IsBreakTime);
            HUDOverlay.IsBreakTime.BindTo(breakTracker.IsBreakTime);
            DimmableStoryboard.IsBreakTime.BindTo(breakTracker.IsBreakTime);

            Background.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
            DimmableStoryboard.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

            storyboardReplacesBackground.Value = Beatmap.Value.Storyboard.ReplacesBackground && Beatmap.Value.Storyboard.HasDrawable;

            foreach (var mod in Mods.Value.OfType<IApplicableToPlayer>())
                mod.ApplyToPlayer(this);

            foreach (var mod in Mods.Value.OfType<IApplicableToHUD>())
                mod.ApplyToHUD(HUDOverlay);

            // Our mods are local copies of the global mods so they need to be re-applied to the track.
            // This is done through the music controller (for now), because resetting speed adjustments on the beatmap track also removes adjustments provided by DrawableTrack.
            // Todo: In the future, player will receive in a track and will probably not have to worry about this...
            musicController.ResetTrackAdjustments();
            foreach (var mod in Mods.Value.OfType<IApplicableToTrack>())
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

            GameplayClockContainer.Restart();
        }

        public override void OnSuspending(IScreen next)
        {
            screenSuspension?.Expire();

            fadeOut();
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            screenSuspension?.Expire();

            if (completionProgressDelegate != null && !completionProgressDelegate.Cancelled && !completionProgressDelegate.Completed)
            {
                // proceed to result screen if beatmap already finished playing
                completionProgressDelegate.RunTask();
                return true;
            }

            // ValidForResume is false when restarting
            if (ValidForResume)
            {
                if (pauseCooldownActive && !GameplayClockContainer.IsPaused.Value)
                    // still want to block if we are within the cooldown period and not already paused.
                    return true;
            }

            // GameplayClockContainer performs seeks / start / stop operations on the beatmap's track.
            // as we are no longer the current screen, we cannot guarantee the track is still usable.
            GameplayClockContainer?.StopUsingBeatmapClock();

            musicController.ResetTrackAdjustments();

            fadeOut();
            return base.OnExiting(next);
        }

        /// <summary>
        /// Creates the player's <see cref="Score"/>.
        /// </summary>
        /// <returns>The <see cref="Score"/>.</returns>
        protected virtual Score CreateScore()
        {
            var score = new Score
            {
                ScoreInfo = new ScoreInfo
                {
                    Beatmap = Beatmap.Value.BeatmapInfo,
                    Ruleset = rulesetInfo,
                    Mods = Mods.Value.ToArray(),
                }
            };

            if (DrawableRuleset.ReplayScore != null)
            {
                score.ScoreInfo.User = DrawableRuleset.ReplayScore.ScoreInfo?.User ?? new GuestUser();
                score.Replay = DrawableRuleset.ReplayScore.Replay;
            }
            else
            {
                score.ScoreInfo.User = api.LocalUser.Value;
                score.Replay = new Replay { Frames = recordingScore?.Replay.Frames.ToList() ?? new List<ReplayFrame>() };
            }

            ScoreProcessor.PopulateScore(score.ScoreInfo);

            return score;
        }

        /// <summary>
        /// Imports the player's <see cref="Score"/> to the local database.
        /// </summary>
        /// <param name="score">The <see cref="Score"/> to import.</param>
        /// <returns>The imported score.</returns>
        protected virtual Task ImportScore(Score score)
        {
            // Replays are already populated and present in the game's database, so should not be re-imported.
            if (DrawableRuleset.ReplayScore != null)
                return Task.CompletedTask;

            LegacyByteArrayReader replayReader;

            using (var stream = new MemoryStream())
            {
                new LegacyScoreEncoder(score, gameplayBeatmap.PlayableBeatmap).Encode(stream);
                replayReader = new LegacyByteArrayReader(stream.ToArray(), "replay.osr");
            }

            return scoreManager.Import(score.ScoreInfo, replayReader);
        }

        /// <summary>
        /// Submits the player's <see cref="Score"/>.
        /// </summary>
        /// <param name="score">The <see cref="Score"/> to submit.</param>
        /// <returns>The submitted score.</returns>
        protected virtual Task SubmitScore(Score score) => Task.CompletedTask;

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

            Background.EnableUserDim.Value = false;
            storyboardReplacesBackground.Value = false;
        }

        #endregion

        IBindable<bool> ISamplePlaybackDisabler.SamplePlaybackDisabled => samplePlaybackDisabled;
    }
}
