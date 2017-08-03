// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// The symbol used for swell pieces.
    /// </summary>
    public class SwellSymbolPiece : SpriteIcon
    {
        public SwellSymbolPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(CirclePiece.SYMBOL_INNER_SIZE);
            Icon = FontAwesome.fa_asterisk;
            Shadow = false;
        }
    }
}
