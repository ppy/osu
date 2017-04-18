// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    internal class DrawableFruit : Sprite
    {
        private readonly CatchBaseHit h;

        public DrawableFruit(CatchBaseHit h)
        {
            this.h = h;

            Origin = Anchor.Centre;
            Scale = new Vector2(0.1f);
            RelativePositionAxes = Axes.Y;
            Position = new Vector2(h.Position, -0.1f);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get(@"Menu/logo");

            const double duration = 0;

            Transforms.Add(new TransformPosition { StartTime = h.StartTime - 200, EndTime = h.StartTime, StartValue = new Vector2(h.Position, -0.1f), EndValue = new Vector2(h.Position, 0.9f) });
            Transforms.Add(new TransformAlpha { StartTime = h.StartTime + duration + 200, EndTime = h.StartTime + duration + 400, StartValue = 1, EndValue = 0 });
            Expire(true);
        }
    }
}
