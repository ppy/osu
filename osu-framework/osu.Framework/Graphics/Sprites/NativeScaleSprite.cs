//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Textures;
using OpenTK;

namespace osu.Framework.Graphics.Sprites
{
    class NativeScaleSprite : Sprite
    {
        public NativeScaleSprite(Texture texture)
            : base(texture)
        {
        }

        public override Vector2 ActualSize
        {
            get
            {
                if (Texture == null) return Vector2.Zero;
                Vector3 comp = DrawInfo.Matrix.ExtractScale();
                return base.ActualSize * new Vector2(1 / comp.X, 1 / comp.Y);
            }
        }
    }
}
