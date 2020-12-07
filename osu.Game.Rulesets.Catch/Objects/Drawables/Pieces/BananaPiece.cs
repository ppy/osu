// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class BananaPiece : CatchHitObjectPiece
    {
        public BananaPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new BananaPulpFormation
                {
                    AccentColour = { BindTarget = AccentColour },
                },
                BorderPiece = new BorderPiece(),
            };
        }
    }
}
