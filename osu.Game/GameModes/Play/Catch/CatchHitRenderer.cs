//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Objects.Catch;
using OpenTK;
using osu.Framework;

namespace osu.Game.GameModes.Play.Catch
{
    public class CatchHitRenderer : HitRenderer
    {
        List<CatchBaseHit> objects;
        private CatchPlayfield playfield;

        public override List<HitObject> Objects
        {
            set
            {
                //osu! mode requires all objects to be of CatchBaseHit type.
                objects = value.ConvertAll(convertForCatch);
            }
        }

        private CatchBaseHit convertForCatch(HitObject input)
        {
            CatchBaseHit h = input as CatchBaseHit;

            if (h == null)
            {
                OsuBaseHit o = input as OsuBaseHit;

                if (o == null) throw new Exception(@"Can't convert!");

                h = new Fruit()
                {
                    StartTime = o.StartTime,
                    Position = o.Position.X
                };
            }

            return h;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            if (playfield == null)
                Add(playfield = new CatchPlayfield());
            else
                playfield.Clear();

            if (objects == null) return;

            foreach (CatchBaseHit h in objects)
            {
                //render stuff!
                Sprite s = new Sprite
                {
                    Texture = game.Textures.Get(@"Menu/logo"),
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.1f),
                    RelativePositionAxes = Axes.Y,
                    Position = new Vector2(h.Position, -0.1f)
                };

                s.Transforms.Add(new TransformPosition(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = new Vector2(h.Position, -0.1f), EndValue = new Vector2(h.Position, 0.9f) });
                s.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });
                s.Expire(true);

                playfield.Add(s);
            }
        }
    }
}
