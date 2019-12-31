// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class SpecialColumnType : ColumnType
    {
        public override Color4 ColumnColour => new Color4(0, 48, 63, 255);
        public override float DefaultColumnWidth => 70;

        public override string NoteTextureName => "mania-noteS";
        public override string HoldTextureName => "mania-noteSL";
        public override string HoldHeadTextureName => "mania-noteSH";
        public override string HoldTailTextureName => "mania-noteST";
        public override string KeyUpTextureName => "mania-keyS";
        public override string KeyDownTextureName => "mania-keySD";

        public override Color4 SkinnedColumnColour => new Color4(63, 63, 63, 255);
    }
}
