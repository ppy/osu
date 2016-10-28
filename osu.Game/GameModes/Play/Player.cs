//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.GameModes.Backgrounds;
using osu.Game.GameModes.Play.Catch;
using osu.Game.GameModes.Play.Mania;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Timing;
using osu.Framework.GameModes;
using osu.Framework.Audio.Track;

namespace osu.Game.GameModes.Play
{
    public class Player : OsuGameMode
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        public BeatmapInfo BeatmapInfo;
        public WorkingBeatmap Beatmap;

        public PlayMode PreferredPlayMode;

        protected override IFrameBasedClock Clock => playerClock;

        private InterpolatingFramedClock playerClock;
        private IAdjustableClock sourceClock;

        protected override void Dispose(bool isDisposing)
        {
            Beatmap?.Dispose();
            base.Dispose(isDisposing);
        }

        protected override bool OnExiting(GameMode next)
        {
            //eagerly dispose as the finalizer runs too late right now.
            Beatmap?.Dispose();

            return base.OnExiting(next);
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            try
            {
                if (Beatmap == null)
                    Beatmap = ((OsuGame)game).Beatmaps.GetBeatmapData(BeatmapInfo);
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
                game.Audio.Track.SetExclusive(track);
                sourceClock = track;
            }

            sourceClock = (IAdjustableClock)Beatmap.Track ?? new StopwatchClock();
            playerClock = new InterpolatingFramedClock(sourceClock);

            Schedule(() =>
            {
                sourceClock.Start();
            });

            HitRenderer hitRenderer;
            ScoreOverlay scoreOverlay;

            if (Beatmap.Beatmap.BeatmapInfo?.Mode > PlayMode.Osu)
            {
                //we only support osu! mode for now because the hitobject parsing is crappy and needs a refactor.
                Exit();
                return;
            }

            PlayMode usablePlayMode = Beatmap.Beatmap.BeatmapInfo?.Mode > PlayMode.Osu ? Beatmap.Beatmap.BeatmapInfo.Mode : PreferredPlayMode;



            switch (usablePlayMode)
            {
                default:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new OsuHitRenderer
                    {
                        Objects = Beatmap.Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Taiko:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new TaikoHitRenderer
                    {
                        Objects = Beatmap.Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Catch:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new CatchHitRenderer
                    {
                        Objects = Beatmap.Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Mania:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new ManiaHitRenderer
                    {
                        Objects = Beatmap.Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
            }

            hitRenderer.OnHit += delegate (HitObject h) { scoreOverlay.OnHit(h); };
            hitRenderer.OnMiss += delegate (HitObject h) { scoreOverlay.OnMiss(h); };

            Children = new Drawable[]
            {
                hitRenderer,
                scoreOverlay,
            };
        }

        protected override void Update()
        {
            base.Update();
            playerClock.ProcessFrame();
        }
    }
}