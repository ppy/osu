// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// The symbol used for centre hit pieces.
    /// </summary>
    public class CentreHitSymbolPiece : CircularContainer
    {
        public CentreHitSymbolPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(CirclePiece.SYMBOL_INNER_SIZE);
            Masking = true;
            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}
