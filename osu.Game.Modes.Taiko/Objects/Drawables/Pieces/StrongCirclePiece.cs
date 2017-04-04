// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// A type of circle piece which is drawn at a higher scale to represent a "strong" piece.
    /// </summary>
    public class StrongCirclePiece : CirclePiece
    {
        /// <summary>
        /// The amount to scale up the base circle to show it as a "strong" piece.
        /// </summary>
        private const float strong_scale = 1.5f;

        public StrongCirclePiece()
        {
            SymbolContainer.Scale = new Vector2(strong_scale);
        }

        public override Vector2 Size => new Vector2(base.Size.X, base.Size.Y * strong_scale);
    }
}
