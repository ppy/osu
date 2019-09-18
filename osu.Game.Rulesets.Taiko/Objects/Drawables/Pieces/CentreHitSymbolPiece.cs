// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// The symbol used for centre hit pieces.
    /// </summary>
    public class CentreHitSymbolPiece : Container
    {
        public CentreHitSymbolPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(CirclePiece.SYMBOL_SIZE);
            Padding = new MarginPadding(CirclePiece.SYMBOL_BORDER);

            Children = new[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new[] { new Box { RelativeSizeAxes = Axes.Both } }
                }
            };
        }
    }
}
