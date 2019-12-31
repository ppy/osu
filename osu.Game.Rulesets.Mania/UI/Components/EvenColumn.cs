// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class EvenColumn : ColumnType
    {
        public override Color4 ColumnColour => new Color4(6, 84, 0, 255);

        public override string NoteTexture => "mania-note2";
        public override string HoldTexture => "mania-note2L";
        public override string HoldHeadTexture => "mania-note2H";
        public override string HoldTailTexture => "mania-note2T";
        public override string KeyTexture  => "mania-key2";
        public override string KeyDownTexture => "mania-key2D";

        public override Color4 SkinnedColumnColour => new Color4(47, 47, 47, 255);
    }
}
