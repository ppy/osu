// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// The symbol used for rim hit pieces.
    /// </summary>
    public class RimHitSymbolPiece : CircularContainer
    {
        public RimHitSymbolPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(CirclePiece.SYMBOL_SIZE);
            BorderThickness = CirclePiece.SYMBOL_BORDER;
            BorderColour = Color4.White;
            Masking = true;
            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true
                }
            };
        }
    }
}
