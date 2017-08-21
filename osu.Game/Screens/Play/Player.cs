// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Beatmaps.Timing;
using osu.Framework.Audio.Sample;
using osu.Game.Beatmaps;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class Player : OsuScreen
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION_FOR_EFFECT / 2;
        private const int letterbox_height = 80;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        internal override bool HasLocalCursorDisplayed => !pauseContainer.IsPaused && !HasFailed && RulesetContainer.ProvidingUserCursor;

        public Action RestartRequested;

        internal override bool AllowBeatmapRulesetChange => false;

        public bool HasFailed { get; private set; }

        public int RestartCount;

        private IAdjustableClock adjustableSourceClock;
        private FramedOffsetClock offsetClock;
        private DecoupleableInterpolatingFramedClock decoupledClock;

        private PauseContainer pauseContainer;

        private Container letterboxInBreaksContainer;

        private RulesetInfo ruleset;

        private ScoreProcessor scoreProcessor;
        protected RulesetContainer RulesetContainer;

        #region User Settings

        private Bindable<double> dimLevel;
        private Bindable<bool> mouseWheelDisabled;
        private Bindable<double> userAudioOffset;

        private SampleChannel sampleRestart;

        #endregion

        private HUDOverlay hudOverlay;
        private FailOverlay failOverlay;
        private BreakPeriodsTrackOverlay breakPeriodsTrackOverlay;

        private bool loadedSuccessfully => RulesetContainer?.Objects.Any() == true;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(AudioManager audio, OsuConfigManager config, OsuGame osu)
        {
            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            sampleRestart = audio.Sample.Get(@"Gameplay/restart");

            WorkingBeatmap working = Beatmap.Value;
            Beatmap beatmap;

            try
            {
                beatmap = working.Beatmap;

                if (beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = osu?.Ruleset.Value ?? beatmap.BeatmapInfo.Ruleset;
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
                        letterboxInBreaksContainer = new Container
                        {
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = letterbox_height,
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = letterbox_height,
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Clock = offsetClock,
                            Children = new Drawable[]
                            {
                                RulesetContainer,
                            }
                        },
                        hudOverlay = new HUDOverlay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new SkipButton(firstObjectTime) { AudioClock = decoupledClock },
                        breakPeriodsTrackOverlay = new BreakPeriodsTrackOverlay(firstObjectTime, Beatmap.Value.Beatmap.Breaks)
                        {
                            AudioClock = decoupledClock,
                            OnBreakIn = () =>
                            {
                                Background?.FadeTo(1, fade_duration);

                                if(!RulesetContainer.HasReplayLoaded)
                                    hudOverlay?.FadeTo(0, fade_duration);

                                hudOverlay.KeyCounter.IsCounting = false;

                                if(Beatmap.Value.Beatmap.BeatmapInfo.LetterboxInBreaks)
                                    letterboxInBreaksContainer.FadeTo(1, fade_duration);
                            },
                            OnBreakOut = () =>
                            {
                                Background?.FadeTo(1 - (float)dimLevel, fade_duration);

                                if(!RulesetContainer.HasReplayLoaded)
                                    hudOverlay?.FadeTo(1, fade_duration);

                                hudOverlay.KeyCounter.IsCounting = true;

                                if(Beatmap.Value.Beatmap.BeatmapInfo.LetterboxInBreaks)
                                    letterboxInBreaksContainer.FadeTo(0, fade_duration);
                            }
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

            hudOverlay.BindProcessor(scoreProcessor);
            hudOverlay.BindRulesetContainer(RulesetContainer);

            hudOverlay.Progress.Objects = RulesetContainer.Objects;
            hudOverlay.Progress.AudioClock = decoupledClock;
            hudOverlay.Progress.AllowSeeking = RulesetContainer.HasReplayLoaded;
            hudOverlay.Progress.OnSeek = pos => decoupledClock.Seek(pos);

            hudOverlay.ModDisplay.Current.BindTo(working.Mods);

            breakPeriodsTrackOverlay.BindHealth(scoreProcessor.Health);

            //bind RulesetContainer to ScoreProcessor and ourselves (for a pass situation)
            RulesetContainer.OnAllJudged += onCompletion;

            //bind ScoreProcessor to ourselves (for a fail situation)
            scoreProcessor.Failed += onFail;
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
                    score.User = RulesetContainer.Replay?.User ?? (Game as OsuGame)?.API?.LocalUser?.Value;
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
            Background?.FadeTo(1 - (float)dimLevel, 1500, Easing.OutQuint);

            Content.Alpha = 0;

            dimLevel.ValueChanged += newDim => Background?.FadeTo(1 - (float)newDim, 800);

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

        private void fadeOut()
        {
            const float fade_out_duration = 250;

            RulesetContainer?.FadeOut(fade_out_duration);
            Content.FadeOut(fade_out_duration);

            hudOverlay?.ScaleTo(0.7f, fade_out_duration * 3, Easing.In);

            Background?.FadeTo(1f, fade_out_duration);
        }

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value && !pauseContainer.IsPaused;
    }
}
