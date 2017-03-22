// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    internal class DrawableTaikoHit : Sprite
    {
        private TaikoHitObject h;

        public DrawableTaikoHit(TaikoHitObject h)
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

            double duration = 0;

            Transforms.Add(new TransformPositionX { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = 1.1f, EndValue = 0.1f });
            Transforms.Add(new TransformAlpha { StartTime = h.StartTime + duration + 200, EndTime = h.StartTime + duration + 400, StartValue = 1, EndValue = 0 });
            Expire(true);
        }
    }
}
