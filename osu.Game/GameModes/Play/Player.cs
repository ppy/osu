//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
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

namespace osu.Game.GameModes.Play
{
    public class Player : OsuGameMode
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        public BeatmapInfo BeatmapInfo;
        public Beatmap Beatmap;

        public PlayMode PlayMode;

        public override void Load(BaseGame game)
        {
            base.Load(game);

            try
            {
                if (Beatmap == null)
                    Beatmap = ((OsuGame)game).Beatmaps.GetBeatmap(BeatmapInfo);
            }
            catch
            {
                //couldn't load, hard abort!
                Exit();
                return;
            }

            HitRenderer hitRenderer;
            ScoreOverlay scoreOverlay;

            switch (PlayMode)
            {
                default:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new OsuHitRenderer
                    {
                        Objects = Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Taiko:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new TaikoHitRenderer
                    {
                        Objects = Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Catch:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new CatchHitRenderer
                    {
                        Objects = Beatmap.HitObjects,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    break;
                case PlayMode.Mania:
                    scoreOverlay = new ScoreOverlayOsu();

                    hitRenderer = new ManiaHitRenderer
                    {
                        Objects = Beatmap.HitObjects,
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
    }
}