// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Sprites
{
    public class EquilateralTriangle : Triangle
    {
        // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
        protected override Vector2 DrawScale => new Vector2(base.DrawScale.X, base.DrawScale.Y * 0.866f);
    }
}
