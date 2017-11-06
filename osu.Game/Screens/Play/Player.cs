﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Backgrounds;
using System;
using System.Linq;
using osu.Framework.Threading;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;
using osu.Framework.Audio.Sample;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Screens.Play.BreaksOverlay;
using osu.Game.Storyboards.Drawables;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class Player : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        public override bool ShowOverlays => false;

        public override bool HasLocalCursorDisplayed => !pauseContainer.IsPaused && !HasFailed && RulesetContainer.ProvidingUserCursor;

        public Action RestartRequested;

        public override bool AllowBeatmapRulesetChange => false;

        public bool HasFailed { get; private set; }

        public int RestartCount;

        private IAdjustableClock adjustableSourceClock;
        private FramedOffsetClock offsetClock;
        private DecoupleableInterpolatingFramedClock decoupledClock;

        private PauseContainer pauseContainer;

        private RulesetInfo ruleset;

        private APIAccess api;

        private ScoreProcessor scoreProcessor;
        protected RulesetContainer RulesetContainer;

        #region User Settings

        private Bindable<double> dimLevel;
        private Bindable<bool> showStoryboard;
        private Bindable<bool> mouseWheelDisabled;
        private Bindable<double> userAudioOffset;

        private SampleChannel sampleRestart;

        #endregion

        private BreakOverlay breakOverlay;
        private Container storyboardContainer;
        private DrawableStoryboard storyboard;

        private HUDOverlay hudOverlay;
        private FailOverlay failOverlay;

        private bool loadedSuccessfully => RulesetContainer?.Objects.Any() == true;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config, APIAccess api)
        {
            this.api = api;

            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            sampleRestart = audio.Sample.Get(@"Gameplay/restart");

            WorkingBeatmap working = Beatmap.Value;
            Beatmap beatmap;

            try
            {
                beatmap = working.Beatmap;

                if (beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = Ruleset.Value ?? beatmap.BeatmapInfo.Ruleset;
                var rulesetInstance = ruleset.CreateInstance();

                try
                {
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(working, ruleset.ID == beatmap.BeatmapInfo.Ruleset.ID);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a RulesetContainer if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(Beatmap, true);
                }

                if (!RulesetContainer.Objects.Any())
                    throw new InvalidOperationException("Beatmap contains no hit objects!");
            }
            catch (Exception e)
            {
                Logger.Log($"Could not load this beatmap sucessfully ({e})!", LoggingTarget.Runtime, LogLevel.Error);

                //couldn't load, hard abort!
                Exit();
                return;
            }

            adjustableSourceClock = (IAdjustableClock)working.Track ?? new StopwatchClock();
            decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            var firstObjectTime = RulesetContainer.Objects.First().StartTime;
            decoupledClock.Seek(Math.Min(0, firstObjectTime - Math.Max(beatmap.ControlPointInfo.TimingPointAt(firstObjectTime).BeatLength * 4, beatmap.BeatmapInfo.AudioLeadIn)));
            decoupledClock.ProcessFrame();

            offsetClock = new FramedOffsetClock(decoupledClock);

            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            Schedule(() =>
            {
                adjustableSourceClock.Reset();

                foreach (var mod in working.Mods.Value.OfType<IApplicableToClock>())
                    mod.ApplyToClock(adjustableSourceClock);

                decoupledClock.ChangeSource(adjustableSourceClock);
            });

            Children = new Drawable[]
            {
                storyboardContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = offsetClock,
                    Alpha = 0,
                },
                pauseContainer = new PauseContainer
                {
                    AudioClock = decoupledClock,
                    FramedClock = offsetClock,
                    OnRetry = Restart,
                    OnQuit = Exit,
                    CheckCanPause = () => ValidForResume && !HasFailed && !RulesetContainer.HasReplayLoaded,
                    Retries = RestartCount,
                    OnPause = () => {
                        hudOverlay.KeyCounter.IsCounting = pauseContainer.IsPaused;
                    },
                    OnResume = () => {
                        hudOverlay.KeyCounter.IsCounting = true;
                    },
                    Children = new Drawable[]
                    {
                        new SkipButton(firstObjectTime) { AudioClock = decoupledClock },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Clock = offsetClock,
                            Child = RulesetContainer,
                        },
                        hudOverlay = new HUDOverlay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        breakOverlay = new BreakOverlay(beatmap.BeatmapInfo.LetterboxInBreaks)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Clock = decoupledClock,
                            Breaks = beatmap.Breaks
                        },
                    }
                },
                failOverlay = new FailOverlay
                {
                    OnRetry = Restart,
                    OnQuit = Exit,
                },
                new HotkeyRetryOverlay
                {
                    Action = () => {
                        //we want to hide the hitrenderer immediately (looks better).
                        //we may be able to remove this once the mouse cursor trail is improved.
                        RulesetContainer?.Hide();
                        Restart();
                    },
                }
            };

            scoreProcessor = RulesetContainer.CreateScoreProcessor();

            if (showStoryboard)
                initializeStoryboard(false);

            hudOverlay.BindProcessor(scoreProcessor);
            hudOverlay.BindRulesetContainer(RulesetContainer);

            hudOverlay.Progress.Objects = RulesetContainer.Objects;
            hudOverlay.Progress.AudioClock = decoupledClock;
            hudOverlay.Progress.AllowSeeking = RulesetContainer.HasReplayLoaded;
            hudOverlay.Progress.OnSeek = pos => decoupledClock.Seek(pos);

            hudOverlay.ModDisplay.Current.BindTo(working.Mods);

            breakOverlay.BindProcessor(scoreProcessor);

            // Bind ScoreProcessor to ourselves
            scoreProcessor.AllJudged += onCompletion;
            scoreProcessor.Failed += onFail;
        }

        private void initializeStoryboard(bool asyncLoad)
        {
            var beatmap = Beatmap.Value.Beatmap;

            storyboard = beatmap.Storyboard.CreateDrawable(Beatmap.Value);
            storyboard.Masking = true;

            storyboardContainer.Add(asyncLoad ? new AsyncLoadWrapper(storyboard) { RelativeSizeAxes = Axes.Both } : (Drawable)storyboard);
        }

        public void Restart()
        {
            sampleRestart?.Play();
            ValidForResume = false;
            RestartRequested?.Invoke();
            Exit();
        }

        private ScheduledDelegate onCompletionEvent;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (scoreProcessor.HasFailed || onCompletionEvent != null)
                return;

            ValidForResume = false;

            using (BeginDelayedSequence(1000))
            {
                onCompletionEvent = Schedule(delegate
                {
                    var score = new Score
                    {
                        Beatmap = Beatmap.Value.BeatmapInfo,
                        Ruleset = ruleset
                    };
                    scoreProcessor.PopulateScore(score);
                    score.User = RulesetContainer.Replay?.User ?? api.LocalUser.Value;
                    Push(new Results(score));
                });
            }
        }

        private bool onFail()
        {
            if (Beatmap.Value.Mods.Value.Any(m => !m.AllowFail))
                return false;

            decoupledClock.Stop();

            HasFailed = true;
            failOverlay.Retries = RestartCount;
            failOverlay.Show();
            return true;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            if (!loadedSuccessfully)
                return;

            (Background as BackgroundScreenBeatmap)?.BlurTo(Vector2.Zero, 1500, Easing.OutQuint);

            dimLevel.ValueChanged += dimLevel_ValueChanged;
            showStoryboard.ValueChanged += showStoryboard_ValueChanged;
            updateBackgroundElements();

            Content.Alpha = 0;
            Content
                .ScaleTo(0.7f)
                .ScaleTo(1, 750, Easing.OutQuint)
                .Delay(250)
                .FadeIn(250);

            this.Delay(750).Schedule(() =>
            {
                if (!pauseContainer.IsPaused)
                    decoupledClock.Start();
            });

            pauseContainer.Alpha = 0;
            pauseContainer.FadeIn(750, Easing.OutQuint);
        }

        protected override void OnSuspending(Screen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if (HasFailed || !ValidForResume || pauseContainer?.AllowExit != false || RulesetContainer?.HasReplayLoaded != false)
            {
                fadeOut();
                return base.OnExiting(next);
            }

            if (loadedSuccessfully)
            {
                pauseContainer.Pause();
            }

            return true;
        }

        private void dimLevel_ValueChanged(double newValue)
            => updateBackgroundElements();

        private void showStoryboard_ValueChanged(bool newValue)
            => updateBackgroundElements();

        private void updateBackgroundElements()
        {
            var opacity = 1 - (float)dimLevel;

            if (showStoryboard && storyboard == null)
                initializeStoryboard(true);

            var beatmap = Beatmap.Value;
            var storyboardVisible = showStoryboard && beatmap.Beatmap.Storyboard.HasDrawable;

            storyboardContainer.FadeColour(new Color4(opacity, opacity, opacity, 1), 800);
            storyboardContainer.FadeTo(storyboardVisible && opacity > 0 ? 1 : 0);

            Background?.FadeTo(!storyboardVisible || beatmap.Background == null ? opacity : 0, 800, Easing.OutQuint);
        }

        private void fadeOut()
        {
            dimLevel.ValueChanged -= dimLevel_ValueChanged;
            showStoryboard.ValueChanged -= showStoryboard_ValueChanged;

            const float fade_out_duration = 250;

            RulesetContainer?.FadeOut(fade_out_duration);
            Content.FadeOut(fade_out_duration);

            hudOverlay?.ScaleTo(0.7f, fade_out_duration * 3, Easing.In);

            Background?.FadeTo(1f, fade_out_duration);
        }

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value && !pauseContainer.IsPaused;
    }
}
