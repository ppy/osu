//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Objects.Taiko;
using OpenTK;

namespace osu.Game.GameModes.Play.Taiko
{
    public class TaikoHitRenderer : HitRenderer
    {
        List<TaikoBaseHit> objects;
        private TaikoPlayfield playfield;

        public override List<BaseHit> Objects
        {
            get
            {
                return objects.ConvertAll(o => (BaseHit)o);
            }

            set
            {
                //osu! mode requires all objects to be of TaikoBaseHit type.
                objects = value.ConvertAll(convertForTaiko);

                if (Parent != null)
                    Load();
            }
        }

        private TaikoBaseHit convertForTaiko(BaseHit input)
        {
            TaikoBaseHit h = input as TaikoBaseHit;

            if (h == null)
            {
                OsuBaseHit o = input as OsuBaseHit;

                if (o == null) throw new Exception(@"Can't convert!");

                h = new TaikoBaseHit()
                {
                    StartTime = o.StartTime
                };
            }

            return h;
        }

        public override void Load()
        {
            base.Load();

            if (playfield == null)
                Add(playfield = new TaikoPlayfield());
            else
                playfield.Clear();

            if (objects == null) return;

            foreach (TaikoBaseHit h in objects)
            {
                //render stuff!
                Sprite s = new Sprite(Game.Textures.Get(@"menu-osu"))
                {
                    Origin = Anchor.Centre,
                    Scale = 0.2f,
                    PositionMode = InheritMode.XY,
                    Position = new Vector2(1.1f, 0.5f)
                };

                s.Transformations.Add(new TransformPosition(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = new Vector2(1.1f, 0.5f), EndValue = new Vector2(0.1f, 0.5f) });
                s.Transformations.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });

                playfield.Add(s);
            }
        }
    }
}
