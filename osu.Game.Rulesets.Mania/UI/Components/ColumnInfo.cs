// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    /// <summary>
    /// Provides information about the appearance of a <see cref="Column"/>.
    /// </summary>
    public abstract class ColumnInfo
    {
        /// <summary>
        /// The colour of the column without skins.
        /// </summary>
        public abstract Color4 DefaultAccentColour { get; }
        /// <summary>
        /// The width of the column without skins.
        /// </summary>
        public virtual float DefaultColumnWidth => Column.COLUMN_WIDTH;

        public abstract string NoteTextureName { get; }
        public abstract string HoldTextureName { get; }
        public abstract string HoldHeadTextureName { get; }
        public abstract string HoldTailTextureName { get; }
        public abstract string KeyUpTextureName { get; }
        public abstract string KeyDownTextureName { get; }
    }
}
