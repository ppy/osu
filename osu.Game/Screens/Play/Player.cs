// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Backgrounds;
using System;
using System.Linq;
using osu.Framework.Threading;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public class Player : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        internal override bool HasLocalCursorDisplayed => !IsPaused && !HasFailed && HitRenderer.ProvidingUserCursor;

        public BeatmapInfo BeatmapInfo;

        public Action RestartRequested;

        public bool IsPaused => !decoupledClock.IsRunning;

        internal override bool AllowRulesetChange => false;

        public bool HasFailed { get; private set; }

        public int RestartCount;

        private const double pause_cooldown = 1000;
        private double lastPauseActionTime;

        private bool canPause => ValidForResume && !HasFailed && Time.Current >= lastPauseActionTime + pause_cooldown;

        private IAdjustableClock adjustableSourceClock;
        private FramedOffsetClock offsetClock;
        private DecoupleableInterpolatingFramedClock decoupledClock;

        private RulesetInfo ruleset;

        private ScoreProcessor scoreProcessor;
        protected HitRenderer HitRenderer;

        #region User Settings

        private Bindable<double> dimLevel;
        private Bindable<bool> mouseWheelDisabled;
        private Bindable<double> userAudioOffset;

        #endregion

        private SkipButton skipButton;

        private Container hitRendererContainer;

        private HUDOverlay hudOverlay;
        private PauseOverlay pauseOverlay;
        private FailOverlay failOverlay;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(AudioManager audio, BeatmapDatabase beatmaps, OsuConfigManager config, OsuGame osu)
        {
            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            Ruleset rulesetInstance;

            try
            {
                if (Beatmap == null)
                    Beatmap = beatmaps.GetWorkingBeatmap(BeatmapInfo, withStoryboard: true);

                if (Beatmap?.Beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = osu?.Ruleset.Value ?? Beatmap.BeatmapInfo.Ruleset;
                rulesetInstance = ruleset.CreateInstance();

                try
                {
                    HitRenderer = rulesetInstance.CreateHitRendererWith(Beatmap);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a HitRenderer if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = Beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    HitRenderer = rulesetInstance.CreateHitRendererWith(Beatmap);
                }

                if (!HitRenderer.Objects.Any())
                    throw new InvalidOperationException("Beatmap contains no hit objects!");
            }
            catch (Exception e)
            {
                Logger.Log($"Could not load this beatmap sucessfully ({e})!", LoggingTarget.Runtime, LogLevel.Error);

                //couldn't load, hard abort!
                Exit();
                return;
            }

            Track track = Beatmap.Track;

            if (track != null)
            {
                audio.Track.SetExclusive(track);
                adjustableSourceClock = track;
            }

            adjustableSourceClock = (IAdjustableClock)track ?? new StopwatchClock();

            decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            var firstObjectTime = HitRenderer.Objects.First().StartTime;
            decoupledClock.Seek(Math.Min(0, firstObjectTime - Math.Max(Beatmap.Beatmap.TimingInfo.BeatLengthAt(firstObjectTime) * 4, Beatmap.BeatmapInfo.AudioLeadIn)));
            decoupledClock.ProcessFrame();

            offsetClock = new FramedOffsetClock(decoupledClock);

            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            Schedule(() =>
            {
                adjustableSourceClock.Reset();

                foreach (var mod in Beatmap.Mods.Value.OfType<IApplicableToClock>())
                    mod.ApplyToClock(adjustableSourceClock);

                decoupledClock.ChangeSource(adjustableSourceClock);
            });

            scoreProcessor = HitRenderer.CreateScoreProcessor();

            hudOverlay = new StandardHUDOverlay()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            hudOverlay.KeyCounter.Add(rulesetInstance.CreateGameplayKeys());
            hudOverlay.BindProcessor(scoreProcessor);
            hudOverlay.BindHitRenderer(HitRenderer);

            hudOverlay.Progress.Objects = HitRenderer.Objects;
            hudOverlay.Progress.AudioClock = decoupledClock;
            hudOverlay.Progress.AllowSeeking = HitRenderer.HasReplayLoaded;
            hudOverlay.Progress.OnSeek = pos => decoupledClock.Seek(pos);

            hudOverlay.ModDisplay.Current.BindTo(Beatmap.Mods);

            //bind HitRenderer to ScoreProcessor and ourselves (for a pass situation)
            HitRenderer.OnAllJudged += onCompletion;

            //bind ScoreProcessor to ourselves (for a fail situation)
            scoreProcessor.Failed += onFail;

            Children = new Drawable[]
            {
                hitRendererContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Clock = offsetClock,
                            Children = new Drawable[]
                            {
                                HitRenderer,
                                skipButton = new SkipButton { Alpha = 0 },
                            }
                        },
                    }
                },
                hudOverlay,
                pauseOverlay = new PauseOverlay
                {
                    OnResume = delegate
                    {
                        Delay(400);
                        Schedule(Resume);
                    },
                    OnRetry = Restart,
                    OnQuit = Exit,
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
                        HitRenderer?.Hide();
                        Restart();
                    },
                }
            };
        }

        protected override void Update()
        {
            // eagerly pause when we lose window focus (if we are locally playing).
            if (!Game.IsActive && !HitRenderer.HasReplayLoaded)
                Pause();

            base.Update();
        }

        private void initializeSkipButton()
        {
            const double skip_required_cutoff = 3000;
            const double fade_time = 300;

            double firstHitObject = Beatmap.Beatmap.HitObjects.First().StartTime;

            if (firstHitObject < skip_required_cutoff)
            {
                skipButton.Alpha = 0;
                skipButton.Expire();
                return;
            }

            skipButton.FadeInFromZero(fade_time);

            skipButton.Action = () =>
            {
                decoupledClock.Seek(firstHitObject - skip_required_cutoff - fade_time);
                skipButton.Action = null;
            };

            skipButton.Delay(firstHitObject - skip_required_cutoff - fade_time);
            skipButton.FadeOut(fade_time);
            skipButton.Expire();
        }

        public void Pause(bool force = false)
        {
            if (!canPause && !force) return;

            // the actual pausing is potentially happening on a different thread.
            // we want to wait for the source clock to stop so we can be sure all components are in a stable state.
            if (!IsPaused)
            {
                decoupledClock.Stop();

                Schedule(() => Pause(force));
                return;
            }

            // we need to do a final check after all of our children have processed up to the paused clock time.
            // this is to cover cases where, for instance, the player fails in the last processed frame (which would change canPause).
            // as the scheduler runs before children updates, let's schedule for the next frame.
            Schedule(() =>
            {
                if (!canPause) return;

                lastPauseActionTime = Time.Current;
                hudOverlay.KeyCounter.IsCounting = false;
                hudOverlay.Progress.Show();
                pauseOverlay.Retries = RestartCount;
                pauseOverlay.Show();
            });
        }

        public void Resume()
        {
            lastPauseActionTime = Time.Current;
            hudOverlay.KeyCounter.IsCounting = true;
            hudOverlay.Progress.Hide();
            pauseOverlay.Hide();
            decoupledClock.Start();
        }

        public void Restart()
        {
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

            Delay(1000);
            onCompletionEvent = Schedule(delegate
            {
                var score = new Score
                {
                    Beatmap = Beatmap.BeatmapInfo,
                    Ruleset = ruleset
                };
                scoreProcessor.PopulateScore(score);
                score.User = HitRenderer.Replay?.User ?? (Game as OsuGame)?.API?.LocalUser?.Value;
                Push(new Results(score));
            });
        }

        private void onFail()
        {
            decoupledClock.Stop();

            HasFailed = true;
            failOverlay.Retries = RestartCount;
            failOverlay.Show();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            (Background as BackgroundScreenBeatmap)?.BlurTo(Vector2.Zero, 1500, EasingTypes.OutQuint);
            Background?.FadeTo(1 - (float)dimLevel, 1500, EasingTypes.OutQuint);

            Content.Alpha = 0;

            dimLevel.ValueChanged += newDim => Background?.FadeTo(1 - (float)newDim, 800);

            Content.ScaleTo(0.7f);

            Content.Delay(250);
            Content.FadeIn(250);

            Content.ScaleTo(1, 750, EasingTypes.OutQuint);

            Delay(750);
            Schedule(() =>
            {
                decoupledClock.Start();
                initializeSkipButton();
            });

            hitRendererContainer.Alpha = 0;
            hitRendererContainer.FadeIn(750, EasingTypes.OutQuint);
        }

        protected override void OnSuspending(Screen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if (!HasFailed && ValidForResume)
            {
                if (pauseOverlay != null && !HitRenderer.HasReplayLoaded)
                {
                    //pause screen override logic.
                    if (pauseOverlay?.State == Visibility.Hidden && !canPause) return true;

                    if (!IsPaused) // For if the user presses escape quickly when entering the map
                    {
                        Pause();
                        return true;
                    }
                }
            }

            fadeOut();
            return base.OnExiting(next);
        }

        private void fadeOut()
        {
            const float fade_out_duration = 250;

            HitRenderer?.FadeOut(fade_out_duration);
            Content.FadeOut(fade_out_duration);

            hudOverlay.ScaleTo(0.7f, fade_out_duration * 3, EasingTypes.In);

            Background?.FadeTo(1f, fade_out_duration);
        }

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value && !IsPaused;
    }
}
