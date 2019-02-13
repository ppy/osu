// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
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
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class Player : ScreenWithBeatmapBackground, IProvideCursor
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
        private Bindable<double> userAudioOffset;

        public int RestartCount;

        public CursorContainer Cursor => RulesetContainer.Cursor;
        public bool ProvidingUserCursor => RulesetContainer?.Cursor != null && !RulesetContainer.HasReplayLoaded.Value;

        private IAdjustableClock sourceClock;

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        private DecoupleableInterpolatingFramedClock adjustableClock;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        private PauseContainer pauseContainer;

        private RulesetInfo ruleset;

        private APIAccess api;

        private SampleChannel sampleRestart;

        protected ScoreProcessor ScoreProcessor;
        protected RulesetContainer RulesetContainer;

        protected HUDOverlay HUDOverlay;
        private FailOverlay failOverlay;

        private DrawableStoryboard storyboard;
        private Container storyboardContainer;

        public bool LoadedBeatmapSuccessfully => RulesetContainer?.Objects.Any() == true;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, APIAccess api, OsuConfigManager config)
        {
            this.api = api;

            WorkingBeatmap working = Beatmap.Value;
            if (working is DummyWorkingBeatmap)
                return;

            sampleRestart = audio.Sample.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);

            IBeatmap beatmap;

            try
            {
                beatmap = working.Beatmap;

                if (beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = Ruleset.Value ?? beatmap.BeatmapInfo.Ruleset;
                var rulesetInstance = ruleset.CreateInstance();

                try
                {
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(working);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a RulesetContainer if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(Beatmap.Value);
                }

                if (!RulesetContainer.Objects.Any())
                {
                    Logger.Log("Beatmap contains no hit objects!", level: LogLevel.Error);
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                //couldn't load, hard abort!
                return;
            }

            sourceClock = (IAdjustableClock)working.Track ?? new StopwatchClock();
            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            adjustableClock.Seek(AllowLeadIn
                ? Math.Min(0, RulesetContainer.GameplayStartTime - beatmap.BeatmapInfo.AudioLeadIn)
                : RulesetContainer.GameplayStartTime);

            adjustableClock.ProcessFrame();

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            var platformOffsetClock = new FramedOffsetClock(adjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 22 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            var offsetClock = new FramedOffsetClock(platformOffsetClock);

            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            ScoreProcessor = RulesetContainer.CreateScoreProcessor();
            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            InternalChildren = new Drawable[]
            {
                pauseContainer = new PauseContainer(offsetClock, adjustableClock)
                {
                    Retries = RestartCount,
                    OnRetry = Restart,
                    OnQuit = performUserRequestedExit,
                    CheckCanPause = () => AllowPause && ValidForResume && !HasFailed && !RulesetContainer.HasReplayLoaded,
                    Children = new[]
                    {
                        storyboardContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        },
                        new ScalingContainer(ScalingMode.Gameplay)
                        {
                            Child = new LocalSkinOverrideContainer(working.Skin)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = RulesetContainer
                            }
                        },
                        new BreakOverlay(beatmap.BeatmapInfo.LetterboxInBreaks, ScoreProcessor)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            ProcessCustomClock = false,
                            Breaks = beatmap.Breaks
                        },
                        new ScalingContainer(ScalingMode.Gameplay)
                        {
                            Child = RulesetContainer.Cursor?.CreateProxy() ?? new Container(),
                        },
                        HUDOverlay = new HUDOverlay(ScoreProcessor, RulesetContainer, working, offsetClock, adjustableClock)
                        {
                            Clock = Clock, // hud overlay doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new SkipOverlay(RulesetContainer.GameplayStartTime)
                        {
                            Clock = Clock, // skip button doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            AdjustableClock = adjustableClock,
                            FramedClock = offsetClock,
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

            HUDOverlay.HoldToQuit.Action = performUserRequestedExit;
            HUDOverlay.KeyCounter.Visible.BindTo(RulesetContainer.HasReplayLoaded);

            RulesetContainer.IsPaused.BindTo(pauseContainer.IsPaused);

            if (ShowStoryboard)
                initializeStoryboard(false);

            // Bind ScoreProcessor to ourselves
            ScoreProcessor.AllJudged += onCompletion;
            ScoreProcessor.Failed += onFail;

            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(ScoreProcessor);
        }

        private void applyRateFromMods()
        {
            if (sourceClock == null) return;

            sourceClock.Rate = 1;
            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(sourceClock);
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
                    if (RulesetContainer.ReplayScore == null)
                        scoreManager.Import(score, true);

                   this.Push(CreateResults(score));

                    onCompletionEvent = null;
                });
            }
        }

        protected virtual ScoreInfo CreateScore()
        {
            var score = RulesetContainer.ReplayScore?.ScoreInfo ?? new ScoreInfo
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

            adjustableClock.Stop();

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

            Task.Run(() =>
            {
                sourceClock.Reset();

                Schedule(() =>
                {
                    adjustableClock.ChangeSource(sourceClock);
                    applyRateFromMods();

                    this.Delay(750).Schedule(() =>
                    {
                        if (!pauseContainer.IsPaused)
                        {
                            adjustableClock.Start();
                        }
                    });
                });
            });

            pauseContainer.Alpha = 0;
            pauseContainer.FadeIn(750, Easing.OutQuint);
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

            if ((!AllowPause || HasFailed || !ValidForResume || pauseContainer?.IsPaused.Value != false || RulesetContainer?.HasReplayLoaded.Value != false) && (!pauseContainer?.IsResuming ?? true))
            {
                // In the case of replays, we may have changed the playback rate.
                applyRateFromMods();

                fadeOut();
                return base.OnExiting(next);
            }

            if (LoadedBeatmapSuccessfully)
                pauseContainer?.Pause();

            return true;
        }

        private void fadeOut(bool instant = false)
        {
            float fadeOutDuration = instant ? 0 : 250;
            this.FadeOut(fadeOutDuration);
            Background?.FadeColour(Color4.White, fadeOutDuration, Easing.OutQuint);
        }

        protected override bool OnScroll(ScrollEvent e) => mouseWheelDisabled.Value && !pauseContainer.IsPaused;

        private void initializeStoryboard(bool asyncLoad)
        {
            if (storyboardContainer == null)
                return;

            var beatmap = Beatmap.Value;

            storyboard = beatmap.Storyboard.CreateDrawable();
            storyboard.Masking = true;

            if (asyncLoad)
                LoadComponentAsync(storyboard, storyboardContainer.Add);
            else
                storyboardContainer.Add(storyboard);
        }

        protected override void UpdateBackgroundElements()
        {
            if (!this.IsCurrentScreen()) return;

            base.UpdateBackgroundElements();

            if (ShowStoryboard && storyboard == null)
                initializeStoryboard(true);

            var beatmap = Beatmap.Value;
            var storyboardVisible = ShowStoryboard && beatmap.Storyboard.HasDrawable;

            storyboardContainer?
                .FadeColour(OsuColour.Gray(BackgroundOpacity), BACKGROUND_FADE_DURATION, Easing.OutQuint)
                .FadeTo(storyboardVisible && BackgroundOpacity > 0 ? 1 : 0, BACKGROUND_FADE_DURATION, Easing.OutQuint);

            if (storyboardVisible && beatmap.Storyboard.ReplacesBackground)
                Background?.FadeColour(Color4.Black, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        protected virtual Results CreateResults(ScoreInfo score) => new SoloResults(score);
    }
}
