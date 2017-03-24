// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    public class AccentedCirclePiece : CirclePiece
    {
        public AccentedCirclePiece(string symbolName)
            : base(symbolName)
        {
        }

        public override Vector2 Size => new Vector2(base.Size.X, base.Size.Y * 1.5f);
    }
}
