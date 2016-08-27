//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;

namespace osu.Framework.Graphics.Sprites
{
    public class SpriteCircular : Sprite
    {
        public float HoverRadius;

        public SpriteCircular(Texture texture, float radius = -1)
            : base(texture)
        {
            HoverRadius = radius > 0 ? radius : texture.DisplayWidth / 2f;
        }

        internal override bool Contains(Vector2 screenSpacePos)
        {
            Vector2 screenHoverRadius = new Vector2(HoverRadius, HoverRadius) * DrawInfo.Matrix.ExtractScale().Xy;
            return Vector2.DistanceSquared(screenSpacePos, ScreenSpaceDrawQuad.Centre) < Vector2.Dot(screenHoverRadius, screenHoverRadius);
        }
    }
}
