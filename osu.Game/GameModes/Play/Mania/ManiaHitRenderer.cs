//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Objects.Mania;
using OpenTK;

namespace osu.Game.GameModes.Play.Mania
{
    public class ManiaHitRenderer : HitRenderer
    {
        private readonly int columns;
        List<ManiaBaseHit> objects;
        private ManiaPlayfield playfield;

        public ManiaHitRenderer(int columns = 5)
        {
            this.columns = columns;
        }

        public override List<BaseHit> Objects
        {
            get
            {
                return objects.ConvertAll(o => (BaseHit)o);
            }

            set
            {
                //osu! mode requires all objects to be of ManiaBaseHit type.
                objects = value.ConvertAll(convertForMania);

                if (Parent != null)
                    Load();
            }
        }

        private ManiaBaseHit convertForMania(BaseHit input)
        {
            ManiaBaseHit h = input as ManiaBaseHit;

            if (h == null)
            {
                OsuBaseHit o = input as OsuBaseHit;

                if (o == null) throw new Exception(@"Can't convert!");

                h = new Note()
                {
                    StartTime = o.StartTime,
                    Column = (int)Math.Round(o.Position.X / 512 * columns)
                };
            }

            return h;
        }

        public override void Load()
        {
            base.Load();

            if (playfield == null)
                Add(playfield = new ManiaPlayfield(columns));
            else
                playfield.Clear();

            if (objects == null) return;

            foreach (ManiaBaseHit h in objects)
            {
                //render stuff!
                Sprite s = new Sprite(Game.Textures.Get(@"menu-osu"))
                {
                    Origin = Anchor.Centre,
                    Scale = 0.1f,
                    PositionMode = InheritMode.XY,
                    Position = new Vector2((float)(h.Column + 0.5) / columns, -0.1f)
                };

                s.Transforms.Add(new TransformPositionY(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = -0.1f, EndValue = 0.9f });
                s.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });

                playfield.Add(s);
            }
        }
    }
}
