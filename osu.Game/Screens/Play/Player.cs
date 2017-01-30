//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Screens.Backgrounds;
using OpenTK.Input;
using MouseState = osu.Framework.Input.MouseState;
using OpenTK;
using osu.Framework.GameModes;
using osu.Game.Modes.UI;
using osu.Game.Screens.Ranking;
using osu.Game.Configuration;
using osu.Game.Overlays.Pause;
using osu.Framework.Configuration;
using System;
using OpenTK.Graphics;

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

        private IAdjustableClock sourceClock;

        private Ruleset ruleset;

        private ScoreProcessor scoreProcessor;
        private HitRenderer hitRenderer;
        private Bindable<int> dimLevel;

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
                    Beatmap = beatmaps.GetWorkingBeatmap(BeatmapInfo);
            }
            catch
            {
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

            pauseOverlay = new PauseOverlay { Depth = -1 };
            pauseOverlay.OnResume = Resume;
            pauseOverlay.OnRetry = Restart;
            pauseOverlay.OnQuit = Exit;

            hitRenderer = ruleset.CreateHitRendererWith(beatmap.HitObjects);

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
                    }
                },
                scoreOverlay,
                pauseOverlay
            };
        }

        public void Pause(bool force = false)
        {
            if (Time.Current >= (lastPauseActionTime + pauseCooldown) || force)
            {
                lastPauseActionTime = Time.Current;
                playerInputManager.PassThrough = true;
                scoreOverlay.KeyCounter.IsCounting = false;
                pauseOverlay.SetRetries(RestartCount);
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
            sourceClock.Stop(); // If the clock is not stopped and Restart is called alot of lag will happen until the game is relaunched

            var newPlayer = new Player();

            newPlayer.Preload(Game, delegate
            {
                RestartCount++;
                newPlayer.RestartCount = RestartCount;
                Exit();

                if (!(last?.Push(newPlayer) ?? false))
                {
                    // Error(?)
                }

                Dispose();
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Delay(250, true);
            Content.FadeIn(250);

            Delay(500, true);

            Schedule(() =>
            {
                sourceClock.Start();
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

        private GameMode last;

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);

            (Background as BackgroundModeBeatmap)?.BlurTo(Vector2.Zero, 1000);
            Background?.FadeTo((100f - dimLevel) / 100, 1000);

            Content.Alpha = 0;
            dimLevel.ValueChanged += dimChanged;

            this.last = last;
        }

        protected override bool OnExiting(GameMode next)
        {
            dimLevel.ValueChanged -= dimChanged;
            Background?.FadeTo(1f, 200);
            return base.OnExiting(next);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (!IsPaused)
                    {
                        Pause();
                        return true;
                    }
                    return false;
            }
            return base.OnKeyDown(state, args);
        }

        private void dimChanged(object sender, EventArgs e)
        {
            Background?.FadeTo((100f - dimLevel) / 100, 800);
        }
    }
}