// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    public class SwellCirclePiece : Container
    {
        private readonly CirclePiece circle;

        public SwellCirclePiece(CirclePiece piece)
        {
            Add(circle = piece);

            circle.Add(new TextAwesome
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = CirclePiece.SYMBOL_INNER_SIZE,
                Icon = FontAwesome.fa_asterisk,
                Shadow = false
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circle.AccentColour = colours.YellowDark;
        }
    }
}
