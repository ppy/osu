// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// The symbol used for swell pieces.
    /// </summary>
    public class SwellSymbolPiece : Container
    {
        private readonly TextAwesome symbol;

        public SwellSymbolPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(CirclePiece.SYMBOL_SIZE);
            Padding = new MarginPadding(CirclePiece.SYMBOL_BORDER);

            Children = new[]
            {
                symbol = new TextAwesome
                {
                    Icon = FontAwesome.fa_asterisk,
                    UseFullGlyphHeight = true,
                    Shadow = false
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            symbol.TextSize = Math.Min(ChildSize.X, ChildSize.Y);
        }
    }
}
