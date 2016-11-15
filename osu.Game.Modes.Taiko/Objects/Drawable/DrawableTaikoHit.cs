//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    class DrawableTaikoHit : Sprite
    {
        private TaikoBaseHit h;

        public DrawableTaikoHit(TaikoBaseHit h)
        {
            this.h = h;

            Origin = Anchor.Centre;
            Scale = new Vector2(0.2f);
            RelativePositionAxes = Axes.Both;
            Position = new Vector2(1.1f, 0.5f);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get(@"Menu/logo");

            Transforms.Add(new TransformPositionX { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = 1.1f, EndValue = 0.1f });
            Transforms.Add(new TransformAlpha { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });
            Expire(true);
        }
    }
}
