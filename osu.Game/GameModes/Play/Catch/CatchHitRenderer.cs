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

namespace osu.Game.GameModes.Play.Catch
{
    public class CatchHitRenderer : HitRenderer
    {
        List<CatchBaseHit> objects;
        private CatchPlayfield playfield;

        public override List<BaseHit> Objects
        {
            get
            {
                return objects.ConvertAll(o => (BaseHit)o);
            }

            set
            {
                //osu! mode requires all objects to be of CatchBaseHit type.
                objects = value.ConvertAll(convertForCatch);

                if (Parent != null)
                    Load();
            }
        }

        private CatchBaseHit convertForCatch(BaseHit input)
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

        public override void Load()
        {
            base.Load();

            if (playfield == null)
                Add(playfield = new CatchPlayfield());
            else
                playfield.Clear();

            if (objects == null) return;

            foreach (CatchBaseHit h in objects)
            {
                //render stuff!
                Sprite s = new Sprite(Game.Textures.Get(@"menu-osu"))
                {
                    Origin = Anchor.Centre,
                    Scale = 0.1f,
                    PositionMode = InheritMode.Y,
                    Position = new Vector2(h.Position, -0.1f)
                };

                s.Transformations.Add(new TransformPosition(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = new Vector2(h.Position, -0.1f), EndValue = new Vector2(h.Position, 0.9f) });
                s.Transformations.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });

                playfield.Add(s);
            }
        }
    }
}
