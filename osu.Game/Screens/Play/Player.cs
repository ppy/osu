// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Play
{
    public class Player : ScreenWithBeatmapBackground, IProvideCursor
    {
        protected override float BackgroundParallaxAmount => 0.1f;

        public override bool ShowOverlaysOnEnter => false;

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

        private PauseContainer pauseContainer;

        private RulesetInfo ruleset;

        private APIAccess api;

        private SampleChannel sampleRestart;

        private ScoreProcessor scoreProcessor;
        protected RulesetContainer RulesetContainer;

        private HUDOverlay hudOverlay;
        private FailOverlay failOverlay;

        private DrawableStoryboard storyboard;
        private Container storyboardContainer;

        private bool loadedSuccessfully => RulesetContainer?.Objects.Any() == true;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, APIAccess api, OsuConfigManager config)
        {
            this.api = api;
            sampleRestart = audio.Sample.Get(@"Gameplay/restart");

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);

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
                Logger.Error(e, "Could not load beatmap sucessfully!");

                //couldn't load, hard abort!
                Exit();
                return;
            }

            sourceClock = (IAdjustableClock)working.Track ?? new StopwatchClock();
            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            var firstObjectTime = RulesetContainer.Objects.First().StartTime;
            adjustableClock.Seek(AllowLeadIn
                ? Math.Min(0, firstObjectTime - Math.Max(beatmap.ControlPointInfo.TimingPointAt(firstObjectTime).BeatLength * 4, beatmap.BeatmapInfo.AudioLeadIn))
                : firstObjectTime);

            adjustableClock.ProcessFrame();

            // the final usable gameplay clock with user-set offsets applied.
            var offsetClock = new FramedOffsetClock(adjustableClock);

            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            scoreProcessor = RulesetContainer.CreateScoreProcessor();

            Children = new Drawable[]
            {
                pauseContainer = new PauseContainer(offsetClock, adjustableClock)
                {
                    OnRetry = Restart,
                    OnQuit = Exit,
                    CheckCanPause = () => AllowPause && ValidForResume && !HasFailed && !RulesetContainer.HasReplayLoaded,
                    OnPause = () =>
                    {
                        pauseContainer.Retries = RestartCount;
                        hudOverlay.KeyCounter.IsCounting = pauseContainer.IsPaused;
                    },
                    OnResume = () => hudOverlay.KeyCounter.IsCounting = true,
                    Children = new Drawable[]
                    {
                        storyboardContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        },
                        new LocalSkinOverrideContainer(working.Skin)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = RulesetContainer
                        },
                        new SkipOverlay(firstObjectTime)
                        {
                            Clock = Clock, // skip button doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            AdjustableClock = adjustableClock,
                            FramedClock = offsetClock,
                        },
                        hudOverlay = new HUDOverlay(scoreProcessor, RulesetContainer, working, offsetClock, adjustableClock)
                        {
                            Clock = Clock, // hud overlay doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new BreakOverlay(beatmap.BeatmapInfo.LetterboxInBreaks, scoreProcessor)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            ProcessCustomClock = false,
                            Breaks = beatmap.Breaks
                        }
                    }
                },
                failOverlay = new FailOverlay
                {
                    OnRetry = Restart,
                    OnQuit = Exit,
                },
                new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!IsCurrentScreen) return;

                        //we want to hide the hitrenderer immediately (looks better).
                        //we may be able to remove this once the mouse cursor trail is improved.
                        RulesetContainer?.Hide();
                        Restart();
                    },
                }
            };

            if (ShowStoryboard)
                initializeStoryboard(false);

            // Bind ScoreProcessor to ourselves
            scoreProcessor.AllJudged += onCompletion;
            scoreProcessor.Failed += onFail;

            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(scoreProcessor);
        }

        private void applyRateFromMods()
        {
            if (sourceClock == null) return;

            sourceClock.Rate = 1;
            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(sourceClock);
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

            if (!AllowResults) return;

            using (BeginDelayedSequence(1000))
            {
                onCompletionEvent = Schedule(delegate
                {
                    if (!IsCurrentScreen) return;

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
            if (Beatmap.Value.Mods.Value.OfType<IApplicableFailOverride>().Any(m => !m.AllowFail))
                return false;

            adjustableClock.Stop();

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

            Content.Alpha = 0;
            Content
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

        protected override void OnSuspending(Screen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if ((!AllowPause || HasFailed || !ValidForResume || pauseContainer?.IsPaused != false || RulesetContainer?.HasReplayLoaded != false) && (!pauseContainer?.IsResuming ?? true))
            {
                // In the case of replays, we may have changed the playback rate.
                applyRateFromMods();

                fadeOut();
                return base.OnExiting(next);
            }

            if (loadedSuccessfully)
                pauseContainer?.Pause();

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
            if (!IsCurrentScreen) return;

            base.UpdateBackgroundElements();

            if (ShowStoryboard && storyboard == null)
                initializeStoryboard(true);

            var beatmap = Beatmap.Value;
            var storyboardVisible = ShowStoryboard && beatmap.Storyboard.HasDrawable;

            storyboardContainer?
                .FadeColour(OsuColour.Gray(BackgroundOpacity), BACKGROUND_FADE_DURATION, Easing.OutQuint)
                .FadeTo(storyboardVisible && BackgroundOpacity > 0 ? 1 : 0, BACKGROUND_FADE_DURATION, Easing.OutQuint);

            if (storyboardVisible && beatmap.Storyboard.ReplacesBackground)
                Background?.FadeTo(0, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
