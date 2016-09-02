//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.GameModes;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.GameModes.Play.Catch;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;
using osu.Framework.Graphics;
using osu.Game.GameModes.Play.Mania;
using OpenTK;

namespace osu.Game.GameModes.Play
{
    public class PlayTest : GameMode
    {
        public override void Load()
        {
            base.Load();

            List<BaseHit> objects = new List<BaseHit>();

            int time = 0;
            for (int i = 0; i < 100; i++)
            {
                objects.Add(new Circle()
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(0, 512), RNG.Next(0, 384))
                });

                time += RNG.Next(50, 500);
            }

            Beatmap beatmap = new Beatmap
            {
                HitObjects = objects
            };

            Add(new OsuHitRenderer()
            {
                Objects = beatmap.HitObjects,
                Scale = 0.5f,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft
            });

            Add(new TaikoHitRenderer()
            {
                Objects = beatmap.HitObjects,
                Scale = 0.5f,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            });

            Add(new CatchHitRenderer()
            {
                Objects = beatmap.HitObjects,
                Scale = 0.5f,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft
            });

            Add(new ManiaHitRenderer()
            {
                Objects = beatmap.HitObjects,
                Scale = 0.5f,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight
            });
        }
    }
}
