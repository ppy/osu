// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Ring
{
    public class BashRingPiece : RingPiece
    {
        protected override Drawable CreateInnerPiece()
        {
            return new TextAwesome
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                TextSize = 45f,
                Icon = FontAwesome.fa_asterisk
            };
        }
    }

}
