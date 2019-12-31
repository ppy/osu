// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public abstract class ColumnType
    {
        public abstract Color4 ColumnColour { get; }
        public virtual float DefaultColumnWidth => Column.COLUMN_WIDTH;

        public abstract string NoteTextureName { get; }
        public abstract string HoldTextureName { get; }
        public abstract string HoldHeadTextureName { get; }
        public abstract string HoldTailTextureName { get; }
        public abstract string KeyUpTextureName { get; }
        public abstract string KeyDownTextureName { get; }

        /// <summary>
        /// Colour to use if notes are skinned.
        /// </summary>
        public abstract Color4 SkinnedColumnColour { get; }
    }
}
