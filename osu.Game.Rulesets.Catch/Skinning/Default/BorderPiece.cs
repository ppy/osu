// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public partial class BorderPiece : Circle
    {
        public BorderPiece()
        {
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            BorderColour = Color4.White;
            BorderThickness = 6f * FruitPiece.RADIUS_ADJUST;

            // Border is drawn only when there is a child drawable.
            Child = new Box
            {
                AlwaysPresent = true,
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
            };
        }
    }
}
