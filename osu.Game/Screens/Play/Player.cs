// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using osu.Framework.GameModes;
using osu.Game.Modes.UI;
using osu.Game.Screens.Ranking;
using osu.Game.Configuration;
using osu.Game.Overlays.Pause;
using osu.Framework.Configuration;
using System;
using System.Linq;
using osu.Game.Beatmaps;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;

namespace osu.Game.Screens.Play
{
    public class Player : OsuGameMode
    {
        public bool Autoplay;

        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        public BeatmapInfo BeatmapInfo;

        public PlayMode PreferredPlayMode;

        private bool isPaused;
        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
        }

        public int RestartCount;

        private double pauseCooldown = 1000;
        private double lastPauseActionTime = 0;

        private bool canPause => Time.Current >= (lastPauseActionTime + pauseCooldown);

        private IAdjustableClock sourceClock;

        private Ruleset ruleset;

        private ScoreProcessor scoreProcessor;
        private HitRenderer hitRenderer;
        private Bindable<int> dimLevel;
        private SkipButton skipButton;

        private ScoreOverlay scoreOverlay;
        private PauseOverlay pauseOverlay;
        private PlayerInputManager playerInputManager;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, BeatmapDatabase beatmaps, OsuGameBase game, OsuConfigManager config)
        {
            dimLevel = config.GetBindable<int>(OsuConfig.DimLevel);
            try
            {
                if (Beatmap == null)
                    Beatmap = beatmaps.GetWorkingBeatmap(BeatmapInfo, withStoryboard: true);

                if ((Beatmap?.Beatmap?.HitObjects.Count ?? 0) == 0)
                    throw new Exception("No valid objects were found!");
            }
            catch (Exception e)
            {
                Logger.Log($"Could not load this beatmap sucessfully ({e})!", LoggingTarget.Runtime, LogLevel.Error);

                //couldn't load, hard abort!
                Exit();
                return;
            }

            AudioTrack track = Beatmap.Track;

            if (track != null)
            {
                audio.Track.SetExclusive(track);
                sourceClock = track;
            }

            sourceClock = (IAdjustableClock)track ?? new StopwatchClock();

            Schedule(() =>
            {
                sourceClock.Reset();
            });

            var beatmap = Beatmap.Beatmap;

            if (beatmap.BeatmapInfo?.Mode > PlayMode.Osu)
            {
                //we only support osu! mode for now because the hitobject parsing is crappy and needs a refactor.
                Exit();
                return;
            }

            PlayMode usablePlayMode = beatmap.BeatmapInfo?.Mode > PlayMode.Osu ? beatmap.BeatmapInfo.Mode : PreferredPlayMode;

            ruleset = Ruleset.GetRuleset(usablePlayMode);

            scoreOverlay = ruleset.CreateScoreOverlay();
            scoreOverlay.BindProcessor(scoreProcessor = ruleset.CreateScoreProcessor(beatmap.HitObjects.Count));

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

            //bind HitRenderer to ScoreProcessor and ourselves (for a pass situation)
            hitRenderer.OnJudgement += scoreProcessor.AddJudgement;
            hitRenderer.OnAllJudged += onPass;

            //bind ScoreProcessor to ourselves (for a fail situation)
            scoreProcessor.Failed += onFail;

            if (Autoplay)
                hitRenderer.Schedule(() => hitRenderer.DrawableObjects.ForEach(h => h.State = ArmedState.Hit));

            Children = new Drawable[]
            {
                playerInputManager = new PlayerInputManager(game.Host)
                {
                    Clock = new InterpolatingFramedClock(sourceClock),
                    PassThrough = false,
                    Children = new Drawable[]
                    {
                        hitRenderer,
                        skipButton = new SkipButton { Alpha = 0 },
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
                playerInputManager.PassThrough = true;
                scoreOverlay.KeyCounter.IsCounting = false;
                pauseOverlay.Retries = RestartCount;
                pauseOverlay.Show();
                sourceClock.Stop();
                isPaused = true;
            }
            else
            {
                isPaused = false;
            }
        }

        public void Resume()
        {
            lastPauseActionTime = Time.Current;
            playerInputManager.PassThrough = false;
            scoreOverlay.KeyCounter.IsCounting = true;
            pauseOverlay.Hide();
            sourceClock.Start();
            isPaused = false;
        }

        public void TogglePaused()
        {
            isPaused = !IsPaused;
            if (IsPaused) Pause(); else Resume();
        }

        public void Restart()
        {
            sourceClock.Stop(); // If the clock is running and Restart is called the game will lag until relaunch

            var newPlayer = new Player();

            newPlayer.Preload(Game, delegate
            {
                newPlayer.RestartCount = RestartCount + 1;
                ValidForResume = false;

                if (!Push(newPlayer))
                {
                    // Error(?)
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Content.Delay(250);
            Content.FadeIn(250);

            Delay(750);
            Schedule(() =>
            {
                sourceClock.Start();
                initializeSkipButton();
            });
        }

        private void onPass()
        {
            Delay(1000);
            Schedule(delegate
            {
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

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);

            (Background as BackgroundModeBeatmap)?.BlurTo(Vector2.Zero, 1000);
            Background?.FadeTo((100f - dimLevel) / 100, 1000);

            Content.Alpha = 0;
            dimLevel.ValueChanged += dimChanged;
        }

        protected override bool OnExiting(GameMode next)
        {
            if (pauseOverlay == null) return false;

            if (pauseOverlay.State != Visibility.Visible && !canPause) return true;

            if (!IsPaused && sourceClock.IsRunning) // For if the user presses escape quickly when entering the map
            {
                Pause();
                return true;
            }
            else
            {
                dimLevel.ValueChanged -= dimChanged;
                Background?.FadeTo(1f, 200);
                return base.OnExiting(next);
            }
        }

        private void dimChanged(object sender, EventArgs e)
        {
            Background?.FadeTo((100f - dimLevel) / 100, 800);
        }
    }
}