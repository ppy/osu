// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    [Cached]
    public class Player : ScreenWithBeatmapBackground
    {
        public override bool AllowBackButton => false; // handled by HoldForMenuButton

        protected override UserActivity InitialActivity => new UserActivity.SoloGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public override float BackgroundParallaxAmount => 0.1f;

        public override bool HideOverlaysOnEnter => true;

        public override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        /// <summary>
        /// Whether gameplay should pause when the game window focus is lost.
        /// </summary>
        protected virtual bool PauseOnFocusLost => true;

        public Action RestartRequested;

        public bool HasFailed { get; private set; }

        private Bindable<bool> mouseWheelDisabled;

        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();

        public int RestartCount;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        private RulesetInfo rulesetInfo;

        private Ruleset ruleset;

        private IAPIProvider api;

        private SampleChannel sampleRestart;

        public BreakOverlay BreakOverlay;

        protected ScoreProcessor ScoreProcessor { get; private set; }

        protected HealthProcessor HealthProcessor { get; private set; }

        protected DrawableRuleset DrawableRuleset { get; private set; }

        protected HUDOverlay HUDOverlay { get; private set; }

        public bool LoadedBeatmapSuccessfully => DrawableRuleset?.Objects.Any() == true;

        protected GameplayClockContainer GameplayClockContainer { get; private set; }

        public DimmableStoryboard DimmableStoryboard { get; private set; }
        public DimmableVideo DimmableVideo { get; private set; }

        [Cached]
        [Cached(Type = typeof(IBindable<IReadOnlyList<Mod>>))]
        protected new readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// Whether failing should be allowed.
        /// By default, this checks whether all selected mods allow failing.
        /// </summary>
        protected virtual bool AllowFail => Mods.Value.OfType<IApplicableFailOverride>().All(m => m.AllowFail);

        private readonly bool allowPause;
        private readonly bool showResults;

        /// <summary>
        /// Create a new player instance.
        /// </summary>
        /// <param name="allowPause">Whether pausing should be allowed. If not allowed, attempting to pause will quit.</param>
        /// <param name="showResults">Whether results screen should be pushed on completion.</param>
        public Player(bool allowPause = true, bool showResults = true)
        {
            this.allowPause = allowPause;
            this.showResults = showResults;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IAPIProvider api, OsuConfigManager config)
        {
            this.api = api;

            Mods.Value = base.Mods.Value.Select(m => m.CreateCopy()).ToArray();

            if (Beatmap.Value is DummyWorkingBeatmap)
                return;

            IBeatmap playableBeatmap = loadPlayableBeatmap();

            if (playableBeatmap == null)
                return;

            sampleRestart = audio.Samples.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            DrawableRuleset = ruleset.CreateDrawableRulesetWith(playableBeatmap, Mods.Value);

            ScoreProcessor = ruleset.CreateScoreProcessor();
            ScoreProcessor.ApplyBeatmap(playableBeatmap);
            ScoreProcessor.Mods.BindTo(Mods);

            HealthProcessor = ruleset.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime);
            HealthProcessor.ApplyBeatmap(playableBeatmap);

            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            InternalChild = GameplayClockContainer = new GameplayClockContainer(Beatmap.Value, Mods.Value, DrawableRuleset.GameplayStartTime);

            addUnderlayComponents(GameplayClockContainer);
            addGameplayComponents(GameplayClockContainer, Beatmap.Value);
            addOverlayComponents(GameplayClockContainer, Beatmap.Value);

            DrawableRuleset.HasReplayLoaded.BindValueChanged(_ => updatePauseOnFocusLostState(), true);

            // bind clock into components that require it
            DrawableRuleset.IsPaused.BindTo(GameplayClockContainer.IsPaused);

            DrawableRuleset.OnNewResult += r =>
            {
                HealthProcessor.ApplyResult(r);
                ScoreProcessor.ApplyResult(r);
            };

            DrawableRuleset.OnRevertResult += r =>
            {
                HealthProcessor.RevertResult(r);
                ScoreProcessor.RevertResult(r);
            };

            // Bind the judgement processors to ourselves
            ScoreProcessor.AllJudged += onCompletion;
            HealthProcessor.Failed += onFail;

            foreach (var mod in Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(ScoreProcessor);

            foreach (var mod in Mods.Value.OfType<IApplicableToHealthProcessor>())
                mod.ApplyToHealthProcessor(HealthProcessor);

            BreakOverlay.IsBreakTime.ValueChanged += _ => updatePauseOnFocusLostState();
        }

        private void addUnderlayComponents(Container target)
        {
            target.Add(DimmableVideo = new DimmableVideo(Beatmap.Value.Video) { RelativeSizeAxes = Axes.Both });
            target.Add(DimmableStoryboard = new DimmableStoryboard(Beatmap.Value.Storyboard) { RelativeSizeAxes = Axes.Both });
        }

        private void addGameplayComponents(Container target, WorkingBeatmap working)
        {
            var beatmapSkinProvider = new BeatmapSkinProvidingContainer(working.Skin);

            // the beatmapSkinProvider is used as the fallback source here to allow the ruleset-specific skin implementation
            // full access to all skin sources.
            var rulesetSkinProvider = new SkinProvidingContainer(ruleset.CreateLegacySkinProvider(beatmapSkinProvider));

            // load the skinning hierarchy first.
            // this is intentionally done in two stages to ensure things are in a loaded state before exposing the ruleset to skin sources.
            target.Add(new ScalingContainer(ScalingMode.Gameplay)
                .WithChild(beatmapSkinProvider
                    .WithChild(target = rulesetSkinProvider)));

            target.AddRange(new Drawable[]
            {
                DrawableRuleset,
                new ComboEffects(ScoreProcessor)
            });
        }

        private void addOverlayComponents(Container target, WorkingBeatmap working)
        {
            target.AddRange(new[]
            {
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
                    KeyCounter = { AlwaysVisible = { BindTarget = DrawableRuleset.HasReplayLoaded } },
                    RequestSeek = GameplayClockContainer.Seek,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                new SkipOverlay(DrawableRuleset.GameplayStartTime)
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
                new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        fadeOut(true);
                        Restart();
                    },
                },
                new HotkeyExitOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        fadeOut(true);
                        performImmediateExit();
                    },
                },
                failAnimation = new FailAnimation(DrawableRuleset) { OnComplete = onFailComplete, }
            });

            DrawableRuleset.Overlays.Add(BreakOverlay = new BreakOverlay(working.Beatmap.BeatmapInfo.LetterboxInBreaks, DrawableRuleset.GameplayStartTime, ScoreProcessor)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Breaks = working.Beatmap.Breaks
            });

            DrawableRuleset.Overlays.Add(ScoreProcessor);
            DrawableRuleset.Overlays.Add(HealthProcessor);

            HealthProcessor.IsBreakTime.BindTo(BreakOverlay.IsBreakTime);
        }

        private void updatePauseOnFocusLostState() =>
            HUDOverlay.HoldToQuit.PauseOnFocusLost = PauseOnFocusLost
                                                     && !DrawableRuleset.HasReplayLoaded.Value
                                                     && !BreakOverlay.IsBreakTime.Value;

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
                Logger.Error(e, "Could not load beatmap sucessfully!");
                //couldn't load, hard abort!
                return null;
            }

            return playable;
        }

        private void performImmediateExit()
        {
            // if a restart has been requested, cancel any pending completion (user has shown intent to restart).
            completionProgressDelegate?.Cancel();

            ValidForResume = false;

            performUserRequestedExit();
        }

        private void performUserRequestedExit()
        {
            if (!this.IsCurrentScreen()) return;

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
            sampleRestart?.Play();
            RestartRequested?.Invoke();

            if (this.IsCurrentScreen())
                performImmediateExit();
            else
                this.MakeCurrent();
        }

        private ScheduledDelegate completionProgressDelegate;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (HealthProcessor.HasFailed || completionProgressDelegate != null)
                return;

            ValidForResume = false;

            if (!showResults) return;

            using (BeginDelayedSequence(1000))
                scheduleGotoRanking();
        }

        protected virtual ScoreInfo CreateScore()
        {
            var score = DrawableRuleset.ReplayScore?.ScoreInfo ?? new ScoreInfo
            {
                Beatmap = Beatmap.Value.BeatmapInfo,
                Ruleset = rulesetInfo,
                Mods = Mods.Value.ToArray(),
                User = api.LocalUser.Value,
            };

            ScoreProcessor.PopulateScore(score);

            return score;
        }

        protected override bool OnScroll(ScrollEvent e) => mouseWheelDisabled.Value && !GameplayClockContainer.IsPaused.Value;

        protected virtual Results CreateResults(ScoreInfo score) => new SoloResults(score);

        #region Fail Logic

        protected FailOverlay FailOverlay { get; private set; }

        private FailAnimation failAnimation;

        private bool onFail()
        {
            if (!AllowFail)
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
            LoadedBeatmapSuccessfully && allowPause && ValidForResume
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
            if (BreakOverlay.IsBreakTime.Value)
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
            Background.IsBreakTime.BindTo(BreakOverlay.IsBreakTime);
            DimmableStoryboard.IsBreakTime.BindTo(BreakOverlay.IsBreakTime);
            DimmableVideo.IsBreakTime.BindTo(BreakOverlay.IsBreakTime);

            Background.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
            DimmableStoryboard.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

            storyboardReplacesBackground.Value = Beatmap.Value.Storyboard.ReplacesBackground && Beatmap.Value.Storyboard.HasDrawable;

            GameplayClockContainer.Restart();
            GameplayClockContainer.FadeInFromZero(750, Easing.OutQuint);

            foreach (var mod in Mods.Value.OfType<IApplicableToPlayer>())
                mod.ApplyToPlayer(this);

            foreach (var mod in Mods.Value.OfType<IApplicableToHUD>())
                mod.ApplyToHUD(HUDOverlay);
        }

        public override void OnSuspending(IScreen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            if (completionProgressDelegate != null && !completionProgressDelegate.Cancelled && !completionProgressDelegate.Completed)
            {
                // proceed to result screen if beatmap already finished playing
                scheduleGotoRanking();
                return true;
            }

            // ValidForResume is false when restarting
            if (ValidForResume)
            {
                if (pauseCooldownActive && !GameplayClockContainer.IsPaused.Value)
                    // still want to block if we are within the cooldown period and not already paused.
                    return true;
            }

            if (canPause)
            {
                Pause();
                return true;
            }

            // GameplayClockContainer performs seeks / start / stop operations on the beatmap's track.
            // as we are no longer the current screen, we cannot guarantee the track is still usable.
            GameplayClockContainer?.StopUsingBeatmapClock();

            fadeOut();
            return base.OnExiting(next);
        }

        private void fadeOut(bool instant = false)
        {
            float fadeOutDuration = instant ? 0 : 250;
            this.FadeOut(fadeOutDuration);

            Background.EnableUserDim.Value = false;
            storyboardReplacesBackground.Value = false;
        }

        private void scheduleGotoRanking()
        {
            completionProgressDelegate?.Cancel();
            completionProgressDelegate = Schedule(delegate
            {
                var score = CreateScore();
                if (DrawableRuleset.ReplayScore == null)
                    scoreManager.Import(score).Wait();

                this.Push(CreateResults(score));
            });
        }

        #endregion
    }
}
