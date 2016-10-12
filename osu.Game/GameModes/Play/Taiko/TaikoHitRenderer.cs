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
using osu.Framework;

namespace osu.Game.GameModes.Play.Taiko
{
    public class TaikoHitRenderer : HitRenderer
    {
        List<TaikoBaseHit> objects;
        private TaikoPlayfield playfield;

        public override List<HitObject> Objects
        {
            set
            {
                //osu! mode requires all objects to be of TaikoBaseHit type.
                objects = value.ConvertAll(convertForTaiko);
            }
        }

        private TaikoBaseHit convertForTaiko(HitObject input)
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

        public override void Load(BaseGame game)
        {
            base.Load(game);

            if (playfield == null)
                Add(playfield = new TaikoPlayfield());
            else
                playfield.Clear();

            if (objects == null) return;

            foreach (TaikoBaseHit h in objects)
            {
                //render stuff!
                Sprite s = new Sprite
                {
                    Texture = game.Textures.Get(@"Menu/logo"),
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.2f),
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(1.1f, 0.5f)
                };

                s.Transforms.Add(new TransformPositionX(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = 1.1f, EndValue = 0.1f });
                s.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });
                s.Expire(true);

                playfield.Add(s);
            }
        }
    }
}
