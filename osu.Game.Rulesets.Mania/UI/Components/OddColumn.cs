// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class OddColumn : ColumnInfo
    {
        public override Color4 DefaultAccentColour => new Color4(94, 0, 57, 255);

        public override string NoteTextureName => "mania-note1";
        public override string HoldTextureName => "mania-note1L";
        public override string HoldHeadTextureName => "mania-note1H";
        public override string HoldTailTextureName => "mania-note1T";
        public override string KeyUpTextureName => "mania-key1";
        public override string KeyDownTextureName => "mania-key1D";
    }
}
