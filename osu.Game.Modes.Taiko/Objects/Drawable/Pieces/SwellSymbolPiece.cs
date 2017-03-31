﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Framework.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// The symbol used for swell pieces.
    /// </summary>
    public class SwellSymbolPiece : TextAwesome
    {
        public SwellSymbolPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            UseFullGlyphHeight = true;
            TextSize = CirclePiece.SYMBOL_INNER_SIZE;
            Icon = FontAwesome.fa_asterisk;
            Shadow = false;
        }
    }
}
