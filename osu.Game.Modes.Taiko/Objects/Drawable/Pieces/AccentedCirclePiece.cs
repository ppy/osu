// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A type of circle piece which is drawn at a higher scale as an "accent".
    /// </summary>
    public class AccentedCirclePiece : CirclePiece
    {
        /// <summary>
        /// The amount to scale up the base circle to show it as an "accented" piece.
        /// </summary>
        private const float accent_scale = 1.5f;

        public AccentedCirclePiece()
        {
            SymbolContainer.Scale = new Vector2(accent_scale);
        }

        public override Vector2 Size => new Vector2(base.Size.X, base.Size.Y * accent_scale);
    }
}
