// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public partial class BananaPiece : CatchHitObjectPiece
    {
        protected override Drawable BorderPiece { get; }

        public BananaPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
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
