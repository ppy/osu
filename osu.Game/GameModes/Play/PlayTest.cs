//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.GameModes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;
using OpenTK;

namespace osu.Game.GameModes.Play
{
    public class PlayTest : GameMode
    {
        public override void Load()
        {
            base.Load();

            Beatmap beatmap = new Beatmap
            {
                HitObjects = new List<BaseHit>()
                {
                    new Circle()
                    {
                        StartTime = 1500,
                        Position = new Vector2(0, 0)
                    },
                    new Circle()
                    {
                        StartTime = 2000,
                        Position = new Vector2(512, 0)
                    },
                    new Circle()
                    {
                        StartTime = 2500,
                        Position = new Vector2(512, 384)
                    },
                    new Circle()
                    {
                        StartTime = 3000,
                        Position = new Vector2(0, 384)
                    },
                }
            };

            Add(new OsuHitRenderer() { Objects = beatmap.HitObjects });
            Add(new TaikoHitRenderer() { Objects = beatmap.HitObjects });
        }
    }
}
