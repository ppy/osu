// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public abstract class ColumnType
    {
        public abstract Color4 ColumnColour { get; }
        public virtual float DefaultColumnWidth => Column.COLUMN_WIDTH;

        public virtual string NoteTexture => "mania-note1";
        public virtual string HoldTexture => "mania-note1L";
        public virtual string HoldHeadTexture => "mania-note1H";
        public virtual string HoldTailTexture => "mania-note1T";
        public virtual string KeyTexture => "mania-key1";
        public virtual string KeyDownTexture => "mania-key1D";

        /// <summary>
        /// Colour to use if notes are skinned.
        /// </summary>
        public virtual Color4 SkinnedColumnColour => new Color4(31, 31, 31, 255);
    }
}
