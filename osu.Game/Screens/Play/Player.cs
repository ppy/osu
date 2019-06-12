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
using osu.Game.Storyboards.Drawables;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public class Player : ScreenWithBeatmapBackground
    {
        protected override bool AllowBackButton => false; // handled by HoldForMenuButton

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

        private RulesetInfo ruleset;

        private IAPIProvider api;

        private SampleChannel sampleRestart;

        protected ScoreProcessor ScoreProcessor { get; private set; }
        protected DrawableRuleset DrawableRuleset { get; private set; }

        protected HUDOverlay HUDOverlay { get; private set; }

        public bool LoadedBeatmapSuccessfully => DrawableRuleset?.Objects.Any() == true;

        protected GameplayClockContainer GameplayClockContainer { get; private set; }

        [Cached]
        [Cached(Type = typeof(IBindable<IReadOnlyList<Mod>>))]
        protected new readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

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

            WorkingBeatmap working = loadBeatmap();

            if (working == null)
                return;

            sampleRestart = audio.Samples.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            ScoreProcessor = DrawableRuleset.CreateScoreProcessor();
            ScoreProcessor.Mods.BindTo(Mods);

            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            InternalChild = GameplayClockContainer = new GameplayClockContainer(working, Mods.Value, DrawableRuleset.GameplayStartTime);

            GameplayClockContainer.Children = new[]
            {
                StoryboardContainer = CreateStoryboardContainer(),
                new ScalingContainer(ScalingMode.Gameplay)
                {
                    Child = new LocalSkinOverrideContainer(working.Skin)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = DrawableRuleset
                    }
                },
                new BreakOverlay(working.Beatmap.BeatmapInfo.LetterboxInBreaks, ScoreProcessor)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Breaks = working.Beatmap.Breaks
                },
                // display the cursor above some HUD elements.
                DrawableRuleset.Cursor?.CreateProxy() ?? new Container(),
                HUDOverlay = new HUDOverlay(ScoreProcessor, DrawableRuleset, Mods.Value)
                {
                    HoldToQuit =
                    {
                        Action = performUserRequestedExit,
                        IsPaused = { BindTarget = GameplayClockContainer.IsPaused }
                    },
                    PlayerSettingsOverlay = { PlaybackSettings = { UserPlaybackRate = { BindTarget = GameplayClockContainer.UserPlaybackRate } } },
                    KeyCounter = { Visible = { BindTarget = DrawableRuleset.HasReplayLoaded } },
                    RequestSeek = GameplayClockContainer.Seek,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                new SkipOverlay(DrawableRuleset.GameplayStartTime)
                {
                    RequestSeek = GameplayClockContainer.Seek
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
                failAnimation = new FailAnimation(DrawableRuleset) { OnComplete = onFailComplete, }
            };

            DrawableRuleset.HasReplayLoaded.BindValueChanged(e => HUDOverlay.HoldToQuit.PauseOnFocusLost = !e.NewValue && PauseOnFocusLost, true);

            // bind clock into components that require it
            DrawableRuleset.IsPaused.BindTo(GameplayClockContainer.IsPaused);

            // load storyboard as part of player's load if we can
            initializeStoryboard(false);

            // Bind ScoreProcessor to ourselves
            ScoreProcessor.AllJudged += onCompletion;
            ScoreProcessor.Failed += onFail;

            foreach (var mod in Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(ScoreProcessor);
        }

        private WorkingBeatmap loadBeatmap()
        {
            WorkingBeatmap working = Beatmap.Value;
            if (working is DummyWorkingBeatmap)
                return null;

            try
            {
                var beatmap = working.Beatmap;

                if (beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = Ruleset.Value ?? beatmap.BeatmapInfo.Ruleset;
                var rulesetInstance = ruleset.CreateInstance();

                try
                {
                    DrawableRuleset = rulesetInstance.CreateDrawableRulesetWith(working, Mods.Value);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a DrawableRuleset if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    DrawableRuleset = rulesetInstance.CreateDrawableRulesetWith(Beatmap.Value, Mods.Value);
                }

                if (!DrawableRuleset.Objects.Any())
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

            return working;
        }

        private void performUserRequestedExit()
        {
            if (!this.IsCurrentScreen()) return;

            this.Exit();
        }

        public void Restart()
        {
            if (!this.IsCurrentScreen()) return;

            sampleRestart?.Play();

            // if a restart has been requested, cancel any pending completion (user has shown intent to restart).
            onCompletionEvent = null;

            ValidForResume = false;
            RestartRequested?.Invoke();
            this.Exit();
        }

        private ScheduledDelegate onCompletionEvent;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (ScoreProcessor.HasFailed || onCompletionEvent != null)
                return;

            ValidForResume = false;

            if (!showResults) return;

            using (BeginDelayedSequence(1000))
            {
                onCompletionEvent = Schedule(delegate
                {
                    if (!this.IsCurrentScreen()) return;

                    var score = CreateScore();
                    if (DrawableRuleset.ReplayScore == null)
                        scoreManager.Import(score).Wait();

                    this.Push(CreateResults(score));

                    onCompletionEvent = null;
                });
            }
        }

        protected virtual ScoreInfo CreateScore()
        {
            var score = DrawableRuleset.ReplayScore?.ScoreInfo ?? new ScoreInfo
            {
                Beatmap = Beatmap.Value.BeatmapInfo,
                Ruleset = ruleset,
                Mods = Mods.Value.ToArray(),
                User = api.LocalUser.Value,
            };

            ScoreProcessor.PopulateScore(score);

            return score;
        }

        protected override bool OnScroll(ScrollEvent e) => mouseWheelDisabled.Value && !GameplayClockContainer.IsPaused.Value;

        protected virtual Results CreateResults(ScoreInfo score) => new SoloResults(score);

        #region Storyboard

        private DrawableStoryboard storyboard;
        protected UserDimContainer StoryboardContainer { get; private set; }

        protected virtual UserDimContainer CreateStoryboardContainer() => new UserDimContainer(true)
        {
            RelativeSizeAxes = Axes.Both,
            Alpha = 1,
            EnableUserDim = { Value = true }
        };

        private Bindable<bool> showStoryboard;

        private void initializeStoryboard(bool asyncLoad)
        {
            if (StoryboardContainer == null || storyboard != null)
                return;

            if (!showStoryboard.Value)
                return;

            var beatmap = Beatmap.Value;

            storyboard = beatmap.Storyboard.CreateDrawable();
            storyboard.Masking = true;

            if (asyncLoad)
                LoadComponentAsync(storyboard, StoryboardContainer.Add);
            else
                StoryboardContainer.Add(storyboard);
        }

        #endregion

        #region Fail Logic

        protected FailOverlay FailOverlay { get; private set; }

        private FailAnimation failAnimation;

        private bool onFail()
        {
            if (Mods.Value.OfType<IApplicableFailOverride>().Any(m => !m.AllowFail))
                return false;

            HasFailed = true;

            // There is a chance that we could be in a paused state as the ruleset's internal clock (see FrameStabilityContainer)
            // could process an extra frame after the GameplayClock is stopped.
            // In such cases we want the fail state to precede a user triggered pause.
            if (PauseOverlay.State.Value == Visibility.Visible)
                PauseOverlay.Hide();

            failAnimation.Start();
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

            IsResuming = false;
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
            double time = GameplayClockContainer.GameplayClock.CurrentTime;
            if (Beatmap.Value.Beatmap.Breaks.Any(b => b.Contains(time)) || time < Beatmap.Value.Beatmap.HitObjects.First().StartTime)
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

            showStoryboard.ValueChanged += _ => initializeStoryboard(true);

            Background.EnableUserDim.Value = true;
            Background.BlurAmount.Value = 0;

            Background.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
            StoryboardContainer.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

            storyboardReplacesBackground.Value = Beatmap.Value.Storyboard.ReplacesBackground && Beatmap.Value.Storyboard.HasDrawable;

            GameplayClockContainer.Restart();
            GameplayClockContainer.FadeInFromZero(750, Easing.OutQuint);
        }

        public override void OnSuspending(IScreen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            if (onCompletionEvent != null)
            {
                // Proceed to result screen if beatmap already finished playing
                onCompletionEvent.RunTask();
                return true;
            }

            if (canPause)
            {
                Pause();
                return true;
            }

            if (pauseCooldownActive && !GameplayClockContainer.IsPaused.Value)
                // still want to block if we are within the cooldown period and not already paused.
                return true;

            if (HasFailed && ValidForResume && !FailOverlay.IsPresent)
                // ValidForResume is false when restarting
            {
                failAnimation.FinishTransforms(true);
                return true;
            }

            GameplayClockContainer.ResetLocalAdjustments();

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

        #endregion
    }
}
