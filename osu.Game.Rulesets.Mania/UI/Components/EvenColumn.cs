// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class EvenColumn : ColumnInfo
    {
        public override Color4 DefaultAccentColour => new Color4(6, 84, 0, 255);

        public override string NoteTextureName => "mania-note2";
        public override string HoldTextureName => "mania-note2L";
        public override string HoldHeadTextureName => "mania-note2H";
        public override string HoldTailTextureName => "mania-note2T";
        public override string KeyUpTextureName => "mania-key2";
        public override string KeyDownTextureName => "mania-key2D";
    }
}
