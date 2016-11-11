//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps.Objects;
using osu.Game.GameModes.Backgrounds;
using osu.Game.GameModes.Play.Catch;
using osu.Game.GameModes.Play.Mania;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Timing;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.GameModes.Play
{
    public class Player : OsuGameMode
    {
        const bool autoplay = false;

        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        internal override bool ShowToolbar => false;

        public BeatmapInfo BeatmapInfo;

        public PlayMode PreferredPlayMode;

        protected override IFrameBasedClock Clock => playerClock;

        private InterpolatingFramedClock playerClock;
        private IAdjustableClock sourceClock;

        protected override void Load(BaseGame game)
        {
            base.Load(game);

            try
            {
                if (Beatmap == null)
                    Beatmap = ((OsuGame)game).Beatmaps.GetWorkingBeatmap(BeatmapInfo);
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

            sourceClock = (IAdjustableClock)track ?? new StopwatchClock();
            playerClock = new InterpolatingFramedClock(sourceClock);

            Schedule(() =>
            {
                sourceClock.Reset();
                sourceClock.Start();
            });

            HitRenderer hitRenderer;
            ScoreOverlay scoreOverlay;

            var beatmap = Beatmap.Beatmap;

            if (beatmap.BeatmapInfo?.Mode > PlayMode.Osu)
            {
                //we only support osu! mode for now because the hitobject parsing is crappy and needs a refactor.
                Exit();
                return;
            }

            PlayMode usablePlayMode = beatmap.BeatmapInfo?.Mode > PlayMode.Osu ? beatmap.BeatmapInfo.Mode : PreferredPlayMode;

            switch (usablePlayMode)
            {
                default:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new OsuHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Taiko:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new TaikoHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Catch:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new CatchHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Mania:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new ManiaHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
            }

            hitRenderer.OnHit += delegate (HitObject h) { scoreOverlay.OnHit(h); };
            hitRenderer.OnMiss += delegate (HitObject h) { scoreOverlay.OnMiss(h); };

            if (autoplay)
                hitRenderer.Schedule(() => hitRenderer.DrawableObjects.ForEach(h => h.State = ArmedState.Armed));

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