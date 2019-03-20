// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

namespace osu.Game.Screens.Play
{
    public class Player : ScreenWithBeatmapBackground
    {
        protected override bool AllowBackButton => false; // handled by HoldForMenuButton

        public override float BackgroundParallaxAmount => 0.1f;

        public override bool HideOverlaysOnEnter => true;

        public override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        public Action RestartRequested;

        public bool HasFailed { get; private set; }

        public bool AllowPause { get; set; } = true;
        public bool AllowLeadIn { get; set; } = true;
        public bool AllowResults { get; set; } = true;

        private Bindable<bool> mouseWheelDisabled;

        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();

        public int RestartCount;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        protected PausableGameplayContainer PausableGameplayContainer { get; private set; }

        private RulesetInfo ruleset;

        private IAPIProvider api;

        private SampleChannel sampleRestart;

        protected ScoreProcessor ScoreProcessor { get; private set; }
        protected DrawableRuleset DrawableRuleset { get; private set; }

        protected HUDOverlay HUDOverlay { get; private set; }
        private FailOverlay failOverlay;

        private DrawableStoryboard storyboard;
        protected UserDimContainer StoryboardContainer { get; private set; }

        private Bindable<bool> showStoryboard;

        protected virtual UserDimContainer CreateStoryboardContainer() => new UserDimContainer(true)
        {
            RelativeSizeAxes = Axes.Both,
            Alpha = 1,
            EnableUserDim = { Value = true }
        };

        public bool LoadedBeatmapSuccessfully => DrawableRuleset?.Objects.Any() == true;

        private GameplayClockContainer gameplayClockContainer;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IAPIProvider api, OsuConfigManager config)
        {
            this.api = api;

            WorkingBeatmap working = loadBeatmap();

            if (working == null)
                return;

            sampleRestart = audio.Sample.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            ScoreProcessor = DrawableRuleset.CreateScoreProcessor();
            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            InternalChild = gameplayClockContainer = new GameplayClockContainer(working, AllowLeadIn, DrawableRuleset.GameplayStartTime);

            gameplayClockContainer.Children = new Drawable[]
            {
                PausableGameplayContainer = new PausableGameplayContainer
                {
                    Retries = RestartCount,
                    OnRetry = Restart,
                    OnQuit = performUserRequestedExit,
                    Start = gameplayClockContainer.Start,
                    Stop = gameplayClockContainer.Stop,
                    IsPaused = { BindTarget = gameplayClockContainer.IsPaused },
                    CheckCanPause = () => AllowPause && ValidForResume && !HasFailed && !DrawableRuleset.HasReplayLoaded.Value,
                    Children = new[]
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
                        HUDOverlay = new HUDOverlay(ScoreProcessor, DrawableRuleset, working)
                        {
                            HoldToQuit = { Action = performUserRequestedExit },
                            PlayerSettingsOverlay = { PlaybackSettings = { UserPlaybackRate = { BindTarget = gameplayClockContainer.UserPlaybackRate } } },
                            KeyCounter = { Visible = { BindTarget = DrawableRuleset.HasReplayLoaded } },
                            RequestSeek = gameplayClockContainer.Seek,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new SkipOverlay(DrawableRuleset.GameplayStartTime)
                        {
                            RequestSeek = gameplayClockContainer.Seek
                        },
                    }
                },
                failOverlay = new FailOverlay
                {
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
                }
            };

            // bind clock into components that require it
            DrawableRuleset.IsPaused.BindTo(gameplayClockContainer.IsPaused);

            if (showStoryboard.Value)
                initializeStoryboard(false);

            // Bind ScoreProcessor to ourselves
            ScoreProcessor.AllJudged += onCompletion;
            ScoreProcessor.Failed += onFail;

            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToScoreProcessor>())
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
                    DrawableRuleset = rulesetInstance.CreateDrawableRulesetWith(working);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a DrawableRuleset if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    DrawableRuleset = rulesetInstance.CreateDrawableRulesetWith(Beatmap.Value);
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

            if (!AllowResults) return;

            using (BeginDelayedSequence(1000))
            {
                onCompletionEvent = Schedule(delegate
                {
                    if (!this.IsCurrentScreen()) return;

                    var score = CreateScore();
                    if (DrawableRuleset.ReplayScore == null)
                        scoreManager.Import(score);

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
                Mods = Beatmap.Value.Mods.Value.ToArray(),
                User = api.LocalUser.Value,
            };

            ScoreProcessor.PopulateScore(score);

            return score;
        }

        private bool onFail()
        {
            if (Beatmap.Value.Mods.Value.OfType<IApplicableFailOverride>().Any(m => !m.AllowFail))
                return false;

            gameplayClockContainer.Stop();

            HasFailed = true;
            failOverlay.Retries = RestartCount;
            failOverlay.Show();
            return true;
        }

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

            showStoryboard.ValueChanged += enabled =>
            {
                if (enabled.NewValue) initializeStoryboard(true);
            };

            Background.EnableUserDim.Value = true;
            Background.BlurAmount.Value = 0;

            Background.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
            StoryboardContainer.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);

            storyboardReplacesBackground.Value = Beatmap.Value.Storyboard.ReplacesBackground && Beatmap.Value.Storyboard.HasDrawable;

            gameplayClockContainer.Restart();

            PausableGameplayContainer.Alpha = 0;
            PausableGameplayContainer.FadeIn(750, Easing.OutQuint);
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

            if ((!AllowPause || HasFailed || !ValidForResume || PausableGameplayContainer?.IsPaused.Value != false || DrawableRuleset?.HasReplayLoaded.Value != false) && (!PausableGameplayContainer?.IsResuming ?? true))
            {
                gameplayClockContainer.ResetLocalAdjustments();

                fadeOut();
                return base.OnExiting(next);
            }

            if (LoadedBeatmapSuccessfully)
                PausableGameplayContainer?.Pause();

            return true;
        }

        private void fadeOut(bool instant = false)
        {
            float fadeOutDuration = instant ? 0 : 250;
            this.FadeOut(fadeOutDuration);

            Background.EnableUserDim.Value = false;
            storyboardReplacesBackground.Value = false;
        }

        protected override bool OnScroll(ScrollEvent e) => mouseWheelDisabled.Value && !PausableGameplayContainer.IsPaused.Value;

        private void initializeStoryboard(bool asyncLoad)
        {
            if (StoryboardContainer == null || storyboard != null)
                return;

            var beatmap = Beatmap.Value;

            storyboard = beatmap.Storyboard.CreateDrawable();
            storyboard.Masking = true;

            if (asyncLoad)
                LoadComponentAsync(storyboard, StoryboardContainer.Add);
            else
                StoryboardContainer.Add(storyboard);
        }

        protected virtual Results CreateResults(ScoreInfo score) => new SoloResults(score);
    }
}
