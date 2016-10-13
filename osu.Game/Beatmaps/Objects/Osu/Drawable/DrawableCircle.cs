//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using OpenTK;

namespace osu.Game.Beatmaps.Objects.Osu.Drawable
{
    class DrawableCircle : Sprite
    {
        private OsuBaseHit h;

        public DrawableCircle(OsuBaseHit h)
        {
            this.h = h;

            Origin = Anchor.Centre;
            Scale = new Vector2(0.1f);
            Alpha = 0;
            Position = h.Position;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Texture = game.Textures.Get(@"Menu/logo");

            Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = 0, EndValue = 1 });
            Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });
            Expire(true);
        }
    }
}
