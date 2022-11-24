// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public partial class HyperBorderPiece : BorderPiece
    {
        public HyperBorderPiece()
        {
            BorderColour = Catcher.DEFAULT_HYPER_DASH_COLOUR;
            BorderThickness = 12f * FruitPiece.RADIUS_ADJUST;

            Child.Alpha = 0.3f;
            Child.Blending = BlendingParameters.Additive;
            Child.Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR;
        }
    }
}
