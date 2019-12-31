// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class SpecialColumn : ColumnType
    {
        public override Color4 ColumnColour => new Color4(0, 48, 63, 255);
        public override float DefaultColumnWidth => 70;

        public override string NoteTexture => "mania-noteS";
        public override string HoldTexture => "mania-noteSL";
        public override string HoldHeadTexture => "mania-noteSH";
        public override string HoldTailTexture => "mania-noteST";
        public override string KeyTexture => "mania-keyS";
        public override string KeyDownTexture => "mania-keySD";

        public override Color4 SkinnedColumnColour => new Color4(63, 63, 63, 255);
    }
}
