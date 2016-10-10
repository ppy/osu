//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using OpenTK;
using System.Diagnostics;

namespace osu.Game.GameModes.Play.Osu
{
    public class OsuHitRenderer : HitRenderer
    {
        List<OsuBaseHit> objects;
        private OsuPlayfield playfield;

        public override List<HitObject> Objects
        {
            set
            {
                Debug.Assert(objects == null);

                //osu! mode requires all objects to be of OsuBaseHit type.
                objects = value.ConvertAll(o => (OsuBaseHit)o);
            }
        }

        public override void Load(Framework.BaseGame game)
        {
            base.Load(game);

            if (playfield == null)
                Add(playfield = new OsuPlayfield());
            else
                playfield.Clear();

            if (objects == null) return;

            foreach (OsuBaseHit h in objects)
            {
                //render stuff!
                Sprite s = new Sprite
                {
                    Texture = game.Textures.Get(@"Menu/logo"),
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.1f),
                    Alpha = 0,
                    Position = h.Position
                };

                s.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = 0, EndValue = 1 });
                s.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });
                s.Expire(true);

                playfield.Add(s);
            }
        }
    }
}
