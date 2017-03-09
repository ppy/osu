﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using osu.Framework.Screens;
using osu.Game.Modes.UI;
using osu.Game.Screens.Ranking;
using osu.Game.Configuration;
using osu.Framework.Configuration;
using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Game.Input.Handlers;

namespace osu.Game.Screens.Play
{
    public class Player : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        public BeatmapInfo BeatmapInfo;

        public bool IsPaused { get; private set; }

        public int RestartCount;

        private double pauseCooldown = 1000;
        private double lastPauseActionTime;

        private bool canPause => Time.Current >= lastPauseActionTime + pauseCooldown;

        private IAdjustableClock sourceClock;
        private IFrameBasedClock interpolatedSourceClock;

        private Ruleset ruleset;

        private ScoreProcessor scoreProcessor;
        private HitRenderer hitRenderer;
        private Bindable<int> dimLevel;
        private SkipButton skipButton;

        private ScoreOverlay scoreOverlay;
        private PauseOverlay pauseOverlay;

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

            Beatmap.Mods.Value.ForEach(m => m.PlayerLoading(this));

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

            scoreProcessor = ruleset.CreateScoreProcessor(beatmap);

            scoreOverlay = ruleset.CreateScoreOverlay();
            scoreOverlay.BindProcessor(scoreProcessor);

            pauseOverlay = new PauseOverlay
            {
                Depth = -1,
                OnResume = delegate
                {
                    Delay(400);
                    Schedule(Resume);
                },
                OnRetry = Restart,
                OnQuit = Exit
            };

            hitRenderer = ruleset.CreateHitRendererWith(beatmap);

            if (ReplayInputHandler != null)
            {
                ReplayInputHandler.ToScreenSpace = hitRenderer.MapPlayfieldToScreenSpace;
                hitRenderer.InputManager.ReplayInputHandler = ReplayInputHandler;
            }

            scoreOverlay.BindHitRenderer(hitRenderer);

            //bind HitRenderer to ScoreProcessor and ourselves (for a pass situation)
            hitRenderer.OnJudgement += scoreProcessor.AddJudgement;
            hitRenderer.OnAllJudged += onCompletion;

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
                        hitRenderer,
                        skipButton = new SkipButton
                        {
                            Alpha = 0
                        },
                    }
                },
                scoreOverlay,
                pauseOverlay
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
                scoreOverlay.KeyCounter.IsCounting = false;
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
            scoreOverlay.KeyCounter.IsCounting = true;
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

            newPlayer.LoadAsync(Game, delegate
            {
                newPlayer.RestartCount = RestartCount + 1;
                ValidForResume = false;

                if (!Push(newPlayer))
                {
                    // Error(?)
                }
            });
        }

        private void onCompletion()
        {
            Delay(1000);
            Schedule(delegate
            {
                // Force a final check to see if the player has failed
                // Some game modes (e.g. taiko) fail at the end of the map
                if (scoreProcessor.CheckFailed())
                {
                    // If failed, onFail will be called that will push a new screen
                    // Let's not push the completion screen in this case
                    return;
                }

                ValidForResume = false;
                Push(new Results
                {
                    Score = scoreProcessor.GetScore()
                });
            });
        }

        private void onFail()
        {
            Content.FadeColour(Color4.Red, 500);
            sourceClock.Stop();

            Delay(500);
            Schedule(delegate
            {
                ValidForResume = false;
                Push(new FailDialog());
            });
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            (Background as BackgroundScreenBeatmap)?.BlurTo(Vector2.Zero, 1500, EasingTypes.OutQuint);
            Background?.FadeTo((100f - dimLevel) / 100, 1500, EasingTypes.OutQuint);

            Content.Alpha = 0;
            dimLevel.ValueChanged += dimChanged;

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
        }

        protected override void OnSuspending(Screen next)
        {
            Content.FadeOut(350);
            Content.ScaleTo(0.7f, 750, EasingTypes.InQuint);

            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if (pauseOverlay == null) return false;

            if (ReplayInputHandler != null) return false;

            if (pauseOverlay.State != Visibility.Visible && !canPause) return true;

            if (!IsPaused && sourceClock.IsRunning) // For if the user presses escape quickly when entering the map
            {
                Pause();
                return true;
            }
            else
            {
                FadeOut(250);
                Content.ScaleTo(0.7f, 750, EasingTypes.InQuint);

                dimLevel.ValueChanged -= dimChanged;
                Background?.FadeTo(1f, 200);
                return base.OnExiting(next);
            }
        }

        private void dimChanged(object sender, EventArgs e)
        {
            Background?.FadeTo((100f - dimLevel) / 100, 800);
        }

        private Bindable<bool> mouseWheelDisabled;

        public ReplayInputHandler ReplayInputHandler;

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value && !IsPaused;
    }
}