﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Modes;
using osu.Game.Modes.UI;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Ranking;
using System;
using System.Linq;
using osu.Framework.Threading;
using osu.Game.Modes.Scoring;

namespace osu.Game.Screens.Play
{
    public class Player : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        internal override bool HasLocalCursorDisplayed => !IsPaused && !HasFailed && HitRenderer.ProvidingUserCursor;

        public BeatmapInfo BeatmapInfo;

        public bool IsPaused { get; private set; }

        public bool HasFailed { get; private set; }

        public int RestartCount;

        private const double pause_cooldown = 1000;
        private double lastPauseActionTime;

        private bool canPause => Time.Current >= lastPauseActionTime + pause_cooldown;

        private IAdjustableClock sourceClock;
        private IFrameBasedClock interpolatedSourceClock;

        private Ruleset ruleset;

        private ScoreProcessor scoreProcessor;
        protected HitRenderer HitRenderer;
        private Bindable<int> dimLevel;
        private SkipButton skipButton;

        private HudOverlay hudOverlay;
        private PauseOverlay pauseOverlay;
        private FailOverlay failOverlay;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, BeatmapDatabase beatmaps, OsuConfigManager config)
        {
            var beatmap = Beatmap.Beatmap;

            if (beatmap.BeatmapInfo?.Mode > PlayMode.Taiko)
            {
                //we only support osu! mode for now because the hitobject parsing is crappy and needs a refactor.
                Exit();
                return;
            }

            dimLevel = config.GetBindable<int>(OsuConfig.DimLevel);
            mouseWheelDisabled = config.GetBindable<bool>(OsuConfig.MouseDisableWheel);

            try
            {
                if (Beatmap == null)
                    Beatmap = beatmaps.GetWorkingBeatmap(BeatmapInfo, withStoryboard: true);

                if ((Beatmap?.Beatmap?.HitObjects.Count ?? 0) == 0)
                    throw new Exception("No valid objects were found!");

                if (Beatmap == null)
                    throw new Exception("Beatmap was not loaded");
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
                sourceClock = track;
            }

            sourceClock = (IAdjustableClock)track ?? new StopwatchClock();
            interpolatedSourceClock = new InterpolatingFramedClock(sourceClock);

            Schedule(() =>
            {
                sourceClock.Reset();
            });

            ruleset = Ruleset.GetRuleset(Beatmap.PlayMode);
            HitRenderer = ruleset.CreateHitRendererWith(Beatmap);

            scoreProcessor = HitRenderer.CreateScoreProcessor();

            hudOverlay = new StandardHudOverlay()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            hudOverlay.KeyCounter.Add(ruleset.CreateGameplayKeys());
            hudOverlay.BindProcessor(scoreProcessor);
            hudOverlay.BindHitRenderer(HitRenderer);

            //bind HitRenderer to ScoreProcessor and ourselves (for a pass situation)
            HitRenderer.OnAllJudged += onCompletion;

            //bind ScoreProcessor to ourselves (for a fail situation)
            scoreProcessor.Failed += onFail;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = interpolatedSourceClock,
                    Children = new Drawable[]
                    {
                        HitRenderer,
                        skipButton = new SkipButton
                        {
                            Alpha = 0
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
                sourceClock.Seek(firstHitObject - skip_required_cutoff - fade_time);
                skipButton.Action = null;
            };

            skipButton.Delay(firstHitObject - skip_required_cutoff - fade_time);
            skipButton.FadeOut(fade_time);
            skipButton.Expire();
        }

        public void Pause(bool force = false)
        {
            if (canPause || force)
            {
                lastPauseActionTime = Time.Current;
                hudOverlay.KeyCounter.IsCounting = false;
                pauseOverlay.Retries = RestartCount;
                pauseOverlay.Show();
                sourceClock.Stop();
                IsPaused = true;
            }
            else
            {
                IsPaused = false;
            }
        }

        public void Resume()
        {
            lastPauseActionTime = Time.Current;
            hudOverlay.KeyCounter.IsCounting = true;
            pauseOverlay.Hide();
            sourceClock.Start();
            IsPaused = false;
        }

        public void TogglePaused()
        {
            IsPaused = !IsPaused;
            if (IsPaused) Pause(); else Resume();
        }

        public void Restart()
        {
            sourceClock.Stop(); // If the clock is running and Restart is called the game will lag until relaunch

            var newPlayer = new Player();

            LoadComponentAsync(newPlayer, delegate
            {
                newPlayer.RestartCount = RestartCount + 1;
                ValidForResume = false;

                if (!Push(newPlayer))
                {
                    // Error(?)
                }
            });
        }

        private ScheduledDelegate onCompletionEvent;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (scoreProcessor.HasFailed || onCompletionEvent != null)
                return;

            Delay(1000);
            onCompletionEvent = Schedule(delegate
            {
                ValidForResume = false;
                Push(new Results
                {
                    Score = scoreProcessor.CreateScore()
                });
            });
        }

        private void onFail()
        {
            sourceClock.Stop();

            Delay(500);

            HasFailed = true;
            failOverlay.Retries = RestartCount;
            failOverlay.Show();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            (Background as BackgroundScreenBeatmap)?.BlurTo(Vector2.Zero, 1500, EasingTypes.OutQuint);
            Background?.FadeTo((100f - dimLevel) / 100, 1500, EasingTypes.OutQuint);

            Content.Alpha = 0;

            dimLevel.ValueChanged += newDim => Background?.FadeTo((100f - newDim) / 100, 800);

            Content.ScaleTo(0.7f);

            Content.Delay(250);
            Content.FadeIn(250);

            Content.ScaleTo(1, 750, EasingTypes.OutQuint);

            Delay(750);
            Schedule(() =>
            {
                sourceClock.Start();
                initializeSkipButton();
            });

            //keep in mind this is using the interpolatedSourceClock so won't be run as early as we may expect.
            HitRenderer.Alpha = 0;
            HitRenderer.FadeIn(750, EasingTypes.OutQuint);
        }

        protected override void OnSuspending(Screen next)
        {
            fadeOut();

            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if (pauseOverlay != null && !HitRenderer.HasReplayLoaded)
            {
                //pause screen override logic.
                if (pauseOverlay?.State == Visibility.Hidden && !canPause) return true;

                if (!IsPaused && sourceClock.IsRunning) // For if the user presses escape quickly when entering the map
                {
                    Pause();
                    return true;
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

        private Bindable<bool> mouseWheelDisabled;

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value && !IsPaused;
    }
}
